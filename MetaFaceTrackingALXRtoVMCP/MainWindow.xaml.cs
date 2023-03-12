using MetaFaceTrackingALXRtoVMCP.Movement;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MetaFaceTrackingALXRtoVMCP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Tracking? tracking;
        readonly MainWindowViewModel _model;

        public MainWindow()
        {
            InitializeComponent();
            _model = (MainWindowViewModel)DataContext;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAppConfig();
            _model.MainText = Tracking.MappingListString(_model.BlendshapeMappingList, null, null);
        }

        private static string AppConfigPath()
        {
            var location = Environment.ProcessPath;
            var name = Path.GetFileNameWithoutExtension(location);
            var dir = Path.GetDirectoryName(location);
            return $"{dir}{Path.DirectorySeparatorChar}{name}.json";
        }

        private void LoadAppConfig()
        {
            var appConfPath = AppConfigPath();
            if (File.Exists(appConfPath))
            {
                var appConf = Utility.Json.DeserializeFromLocalFile<AppConfig>(appConfPath) ?? new AppConfig();
                LoadWorkerConfig(appConf.ConfigPath);
            }
        }

        private void SaveAppConfig(string workerConfigPath)
        {
            var appConfPath = AppConfigPath();
            var appConf = new AppConfig()
            {
                ConfigPath = workerConfigPath
            };
            Utility.Json.SerializeToLocalFile(appConf, appConfPath, Newtonsoft.Json.Formatting.Indented);
        }

        private bool LoadWorkerConfig(string workerConfigPath)
        {
            var setting = new Newtonsoft.Json.JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };
            try
            {
                var config = Utility.Json.DeserializeFromLocalFile<WorkerConfig>(workerConfigPath, setting);
                if (config != null)
                {
                    _model.UpdateConfig(config);
                    _model.ConfigPath = Path.GetDirectoryName(workerConfigPath) + "\r\n" + Path.GetFileName(workerConfigPath);
                    return true;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }

            _model.UpdateConfig(new WorkerConfig());
            _model.ConfigPath = "(not selected)";
            return false;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (tracking == null)
            {
                tracking = new Tracking(
                    _model.VrmFile,
                    _model.BlendshapeMappingList,
                    _model.NoSendBlendshapes);

                tracking.Start(
                    _model.AlxrConnAddress,
                    int.Parse(_model.AlxrConnPort),
                    _model.VmcpSendAddress,
                    int.Parse(_model.VmcpSendPort),
                    UpdateTextBlock);

                _model.ButtonText = "Stop";
                _model.IsStop = false;
            }
            else
            {
                tracking.Stop();
                tracking = null;
                _model.ButtonText = "Start";
                _model.IsStop = true;
            }
        }

        private void UpdateTextBlock(string text, CancellationToken token)
        {
            Dispatcher.InvokeAsync(
                new Action(() =>
                {
                    _model.MainText = text;
                }),
                DispatcherPriority.Normal,
                token);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSONファイル(*.json)|*.json|全てのファイル(*.*)|*.*"
            };

            var isClickOk = dialog.ShowDialog() ?? false;

            // 開くボタン以外が押下された場合
            if (!isClickOk)
            {
                return;
            }

            var isSuccess = LoadWorkerConfig(dialog.FileName);
            if (isSuccess)
            {
                SaveAppConfig(dialog.FileName);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSONファイル(*.json)|*.json|全てのファイル(*.*)|*.*"
            };

            var isClickOk = dialog.ShowDialog() ?? false;

            // 保存ボタン以外が押下された場合
            if (!isClickOk)
            {
                return;
            }

            var setting = new Newtonsoft.Json.JsonSerializerSettings()
            {
                Converters = new List<Newtonsoft.Json.JsonConverter>()
                {
                    new Newtonsoft.Json.Converters.StringEnumConverter()
                },
                Formatting = Newtonsoft.Json.Formatting.Indented
            };
            Utility.Json.SerializeToLocalFile(_model._config, dialog.FileName, setting);

            _model.ConfigPath = Path.GetDirectoryName(dialog.FileName) + "\r\n" + Path.GetFileName(dialog.FileName);
            SaveAppConfig(dialog.FileName);
        }
    }

    internal class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point()
        {
        }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    internal class BlendshapeMapping
    {
        public Face.Expression FaceExpression { get; set; }

        public string BlendshapeName { get; set; } = "";

        public List<Point> Curve
        {
            get
            {
                return _curve;
            }
            set
            {
                _curve = value;
                CalcSlope();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal int BlendshapeIndex = -1;

        private List<Point> _curve = new();

        private double[] _slope = new double[1];

        public BlendshapeMapping()
        {
        }

        public BlendshapeMapping(int faceExpression, string blendshapeName, params (double, double)[] curve)
        {
            FaceExpression = (Face.Expression)faceExpression;
            BlendshapeName = blendshapeName;
            foreach (var c in curve)
            {
                _curve.Add(new Point(c.Item1, c.Item2));
            }
            CalcSlope();
        }

        private void CalcSlope()
        {
            if (_curve.Count < 2) return;

            _slope = new double[_curve.Count - 1];
            for (var i = 0; i < _curve.Count - 1; i++)
            {
                _slope[i] = (_curve[i + 1].Y - _curve[i].Y) / (_curve[i + 1].X - _curve[i].X);
            }
        }

        public double GetCurveY(double x)
        {
            // 閾値を超えた場合は丸める
            if (x < 0) x = 0;
            if (x > 1) x = 1;

            // どのカーブか探す
            var i = -1;
            foreach (var p in _curve)
            {
                if (p.X > x)
                {
                    return _slope[i] * (x - _curve[i].X) + _curve[i].Y;
                }
                i++;
            }

            // x==1の場合
            return _curve[i - 1].Y;
        }
    }

    internal class AppConfig
    {
        public string ConfigPath { get; set; } = "";
    }

    internal class WorkerConfig
    {
        public string VmcpSendAddress { get; set; } = "127.0.0.1";
        public string VmcpSendPort { get; set; } = "39540";
        public string AlxrConnAddress { get; set; } = "127.0.0.1";
        public string AlxrConnPort { get; set; } = "13191";
        public string VrmFile { get; set; } = "default.vrm";
        public List<string> NoSendBlendshapes { get; set; } = new();
        public List<BlendshapeMapping> BlendshapeMappingList { get; set; } = new List<BlendshapeMapping>()
            {
                new BlendshapeMapping(0, "BrowDownLeft", (0d, 0d), (0.667d, 1d), (1d, 1d)),
                new BlendshapeMapping(1, "BrowDownRight", (0d, 0d), (0.667d, 1d), (1d, 1d)),
                new BlendshapeMapping(2, "CheekPuff", (0d, 0d), (0.757d, 0.5d), (1d, 0.5d)),
                new BlendshapeMapping(3, "CheekPuff", (0d, 0d), (0.757d, 0.5d), (1d, 0.5d)),
                new BlendshapeMapping(4, "CheekSquintLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(5, "CheekSquintRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(6, ""),
                new BlendshapeMapping(7, ""),
                new BlendshapeMapping(8, "MouthShrugLower", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(9, "MouthShrugUpper", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(10, "MouthDimpleLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(11, "MouthDimpleRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(12, "EyeBlinkLeft", (0d, 0d), (0.5d, 1d), (1d, 1d)),
                new BlendshapeMapping(13, "EyeBlinkRight", (0d, 0d), (0.5d, 1d), (1d, 1d)),
                new BlendshapeMapping(14, "EyeLookDownLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(15, "EyeLookDownRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(16, "EyeLookOutLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(17, "EyeLookInRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(18, "EyeLookInLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(19, "EyeLookOutRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(20, "EyeLookUpLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(21, "EyeLookUpRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(22, "BrowInnerUp", (0d, 0d), (0.5d, 0.5d), (1d, 0.5d)),
                new BlendshapeMapping(23, "BrowInnerUp", (0d, 0d), (0.5d, 0.5d), (1d, 0.5d)),
                new BlendshapeMapping(24, "JawOpen", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(25, "JawLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(26, "JawRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(27, "JawForward", (0d, 0d), (1d, 0.2d)),
                new BlendshapeMapping(28, "EyeSquintLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(29, "EyeSquintRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(30, "MouthFrownLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(31, "MouthFrownRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(32, "MouthSmileLeft", (0d, 0d), (0.68d, 1d), (1d, 1d)),
                new BlendshapeMapping(33, "MouthSmileRight", (0d, 0d), (0.68d, 1d), (1d, 1d)),
                new BlendshapeMapping(34, "MouthFunnel", (0d, 0d), (0.667d, 0.25d), (1d, 0.25d)),
                new BlendshapeMapping(35, "MouthFunnel", (0d, 0d), (0.667d, 0.25d), (1d, 0.25d)),
                new BlendshapeMapping(36, "MouthFunnel", (0d, 0d), (0.667d, 0.25d), (1d, 0.25d)),
                new BlendshapeMapping(37, "MouthFunnel", (0d, 0d), (0.667d, 0.25d), (1d, 0.25d)),
                new BlendshapeMapping(38, "MouthPressLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(39, "MouthPressRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(40, "MouthPucker", (0d, 0d), (0.681d, 0.5d), (1d, 0.5d)),
                new BlendshapeMapping(41, "MouthPucker", (0d, 0d), (0.681d, 0.5d), (1d, 0.5d)),
                new BlendshapeMapping(42, "MouthStretchLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(43, "MouthStretchRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(44, "MouthRollLower", (0d, 0d), (1d, 0.5d)),
                new BlendshapeMapping(45, "MouthRollUpper", (0d, 0d), (1d, 0.5d)),
                new BlendshapeMapping(46, "MouthRollLower", (0d, 0d), (1d, 0.5d)),
                new BlendshapeMapping(47, "MouthRollUpper", (0d, 0d), (1d, 0.5d)),
                new BlendshapeMapping(48, ""),
                new BlendshapeMapping(49, ""),
                new BlendshapeMapping(50, "MouthClose", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(51, "MouthLowerDownLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(52, "MouthLowerDownRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(53, "MouthLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(54, "MouthRight", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(55, "NoseSneerLeft", (0d, 0d), (0.5d, 1d), (1d, 1d)),
                new BlendshapeMapping(56, "NoseSneerRight", (0d, 0d), (0.5d, 1d), (1d, 1d)),
                new BlendshapeMapping(57, "BrowOuterUpLeft", (0d, 0d), (0.5d, 1d), (1d, 1d)),
                new BlendshapeMapping(58, "BrowOuterUpRight", (0d, 0d), (0.5d, 1d), (1d, 1d)),
                new BlendshapeMapping(59, "EyeWideLeft", (0d, 0d), (0.7d, 1d), (1d, 1d)),
                new BlendshapeMapping(60, "EyeWideRight", (0d, 0d), (0.7d, 1d), (1d, 1d)),
                new BlendshapeMapping(61, "MouthUpperUpLeft", (0d, 0d), (1d, 1d)),
                new BlendshapeMapping(62, "MouthUpperUpRight", (0d, 0d), (1d, 1d)),
            };
    }

    internal class MainWindowViewModel : ObservableBase
    {
        private string _ConfigPath = "(not selected)";
        private string _ButtonText = "Start";
        private string _MainText = "Blendshape List";
        private bool _IsStop = true;
        internal WorkerConfig _config = new();

        public string ConfigPath { get => _ConfigPath; set => SetProperty(ref _ConfigPath, value); }
        public string VmcpSendAddress { get => _config.VmcpSendAddress; set { _config.VmcpSendAddress = value; OnPropertyChanged(); } }
        public string VmcpSendPort { get => _config.VmcpSendPort; set { _config.VmcpSendPort = value; OnPropertyChanged(); } }
        public string AlxrConnAddress { get => _config.AlxrConnAddress; set { _config.AlxrConnAddress = value; OnPropertyChanged(); } }
        public string AlxrConnPort { get => _config.AlxrConnPort; set { _config.AlxrConnPort = value; OnPropertyChanged(); } }
        public string VrmFile { get => _config.VrmFile; set { _config.VrmFile = value; OnPropertyChanged(); } }
        public List<string> NoSendBlendshapes { get => _config.NoSendBlendshapes; set { _config.NoSendBlendshapes = value; OnPropertyChanged(); } }
        public List<BlendshapeMapping> BlendshapeMappingList { get => _config.BlendshapeMappingList; set { _config.BlendshapeMappingList = value; OnPropertyChanged(); } }
        public string ButtonText { get => _ButtonText; set => SetProperty(ref _ButtonText, value); }
        public string MainText { get => _MainText; set => SetProperty(ref _MainText, value); }
        public bool IsStop { get => _IsStop; set => SetProperty(ref _IsStop, value); }

        public void UpdateConfig(WorkerConfig config)
        {
            _config = config;
            OnPropertyChanged("VmcpSendAddress");
            OnPropertyChanged("VmcpSendPort");
            OnPropertyChanged("AlxrConnAddress");
            OnPropertyChanged("AlxrConnPort");
            OnPropertyChanged("VrmFile");
            OnPropertyChanged("NoSendBlendshapes");
        }
    }

    internal class ObservableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? name = null)
        {
            if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(name);
                return true;
            }
            return false;
        }
    }
}
