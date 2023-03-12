using MetaFaceTrackingALXRtoVMCP.Movement;
using MetaFaceTrackingALXRtoVMCP.Osc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MetaFaceTrackingALXRtoVMCP
{
    internal class Tracking
    {
        private Task? _task;
        private readonly byte[] _rawExpressions = new byte[(int)Face.Expression.Max * 4 + (8 * 2 * 4)];
        private readonly Movement.Movement _movement = new();
        private readonly BlendshapeConverter _converter;

        private CancellationTokenSource? tokenSource;

        public Tracking(string vrmFilePath, List<BlendshapeMapping> blendshapeMappingList, List<string> noSendBlendshapes)
        {
            _converter = new(vrmFilePath, blendshapeMappingList, noSendBlendshapes);
        }

        public void Start(string alxrAddress, int alxrPort, string vmcAddress, int vmcPort, Action<string, CancellationToken> updateTextBlock)
        {
            tokenSource = new CancellationTokenSource();
            _task = Task.Run(() => MyAction(alxrAddress, alxrPort, vmcAddress, vmcPort, updateTextBlock, tokenSource.Token));
        }

        public async void MyAction(string alxrAddress, int alxrPort, string vmcAddress, int vmcPort, Action<string, CancellationToken> updateTextBlock, CancellationToken token)
        {
            bool connected = false;
            TcpClient? client = null;
            NetworkStream? stream = null;

            updateTextBlock(MappingListString(_converter.MappingList, _movement, _converter.BlendValueBuffers), token);

            while (true)
            {
                try
                {
                    if (token.IsCancellationRequested) break;

                    // Attempt reconnection if needed
                    if (!connected || stream == null)
                    {
                        client = new TcpClient();
                        await client.ConnectAsync(alxrAddress, alxrPort, token);
                        stream = client.GetStream();
                        connected = true;
                    }

                    if (stream == null)
                    {
                        continue;
                    }

                    if (!stream.CanRead)
                    {
                        continue;
                    }

                    int offset = 0;
                    int readBytes;
                    do
                    {
                        readBytes = await stream.ReadAsync(_rawExpressions.AsMemory(offset, _rawExpressions.Length - offset), token);
                        offset += readBytes;
                    }
                    while (readBytes > 0 && offset < _rawExpressions.Length);

                    if (offset < _rawExpressions.Length && connected)
                    {
                        // Reconnect to the server if we lose connection
                        Thread.Sleep(1000);
                        connected = false;
                        stream.Close();
                        continue;
                    }

                    _movement.Update(_rawExpressions);
                    _converter.ApplyTrackingData(_movement);

                    using OscClient oscClient = new(vmcAddress, vmcPort);
                    var bundle = new Bundle();
                    foreach (var valueBuff in _converter.BlendValueBuffers)
                    {
                        if (!valueBuff.Ignore)
                        {
                            bundle.Add(new Message("/VMC/Ext/Blend/Val", valueBuff.Name, valueBuff.Value));
                        }
                    }
                    bundle.Add(new Message("/VMC/Ext/Blend/Apply"));
                    oscClient.Send(bundle);

                    updateTextBlock(MappingListString(_converter.MappingList, _movement, _converter.BlendValueBuffers), token);
                }
                catch (SocketException e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                    Thread.Sleep(1000);
                }
                catch (OperationCanceledException e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                    break;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.ToString());
                    Thread.Sleep(1000);
                }
            }

            client?.Close();
            stream?.Close();
        }

        public void Stop()
        {
            if (_task != null)
            {
                tokenSource?.Cancel();
                _task.Wait();
                _task = null;
                tokenSource?.Dispose();
            }
        }

        public static string MappingListString(List<BlendshapeMapping> mappingList, Movement.Movement? movement, BlendshapeConverter.BlendValueBuffer[]? blendValueBuffers)
        {
            var sb = new StringBuilder();
            sb.Append("[Facial Expression]".PadRight(22));
            sb.Append("\t            \t[Blendshape]\n");
            mappingList.ForEach(mapping =>
            {
                sb.Append(mapping.FaceExpression.ToString().PadRight(22));
                sb.Append('\t');
                sb.Append(string.Format("{0:F10}", (movement == null) ? 0f : movement.Face[mapping.FaceExpression]));
                if (mapping.BlendshapeIndex >= 0)
                {
                    sb.Append('\t');
                    sb.Append(mapping.BlendshapeName.PadRight(22));
                    sb.Append('\t');
                    sb.Append(string.Format("{0:F10}", (blendValueBuffers == null) ? 0f : blendValueBuffers[mapping.BlendshapeIndex].Value));
                }
                sb.Append('\n');
            });
            return sb.ToString();
        }
    }
}
