using System.Collections.Generic;
using System.IO;
using System.Linq;
using static MetaFaceTrackingALXRtoVMCP.Vrm;

namespace MetaFaceTrackingALXRtoVMCP
{
    internal class Vrm
    {
        public class BlendShapeName
        {
            /// <summary>
            /// VRMに格納されているBlendShapeGroupの配列インデックス
            /// </summary>
            public int Index;

            /// <summary>
            /// VRMに格納されているBlendShapeGroupのnameの値
            /// </summary>
            public string Name = "";

            /// <summary>
            /// VRMに格納されているBlendShapeGroupのpresetNameの値
            /// </summary>
            public string PresetName = "";
        }

        /// <summary>
        /// 現在読み込んでいるVRMファイルのパス
        /// </summary>
        public string Path;

        /// <summary>
        /// VRMに格納されているブレンドシェイプの名前のリスト
        /// </summary>
        public List<BlendShapeName> BlendShapeNames;

        public Vrm()
        {
            Path = "";
            BlendShapeNames = new();
        }

        public void Load(string path)
        {
            Path = "";
            BlendShapeNames.Clear();

            if (File.Exists(path))
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var br = new BinaryReader(fs);

                // magic "glTF"
                var magic = br.ReadUInt32();
                if (magic != 0x46546C67)
                {
                    return;
                }

                // version
                _ = br.ReadUInt32();

                // length
                var length = br.ReadUInt32();
                if (length != fs.Length)
                {
                    return;
                }

                // chunkLength
                var chunkLength = br.ReadUInt32();
                if (chunkLength > (length - 20))
                {
                    return;
                }

                // chunkType "JSON"
                var chunkType = br.ReadUInt32();
                if (chunkType != 0x4e4f534a)
                {
                    return;
                }

                // chunkData
                var chunkData = br.ReadBytes((int)chunkLength);

                // パース
                var jsonNode = System.Text.Json.Nodes.JsonNode.Parse(chunkData);
                var blendShapeGroups = jsonNode?["extensions"]?["VRM"]?["blendShapeMaster"]?["blendShapeGroups"]?.AsArray().Select((x, i) =>
                {
                    return (index: i, name: x?["name"]?.ToString(), presetName: x?["presetName"]?.ToString());
                });
                if (blendShapeGroups == null)
                {
                    return;
                }

                // パース結果格納
                foreach (var (index, name, presetName) in blendShapeGroups)
                {
                    BlendShapeNames.Add(new BlendShapeName { Index = index, Name = name ?? "", PresetName = presetName ?? "" });
                }
                Path = path;
            }

            if (Path.Length == 0)
            {
                BlendShapeNames.Add(new BlendShapeName { Index = 0, Name = "Neutral", PresetName = "neutral" });
                BlendShapeNames.Add(new BlendShapeName { Index = 1, Name = "A", PresetName = "a" });
                BlendShapeNames.Add(new BlendShapeName { Index = 2, Name = "I", PresetName = "i" });
                BlendShapeNames.Add(new BlendShapeName { Index = 3, Name = "U", PresetName = "u" });
                BlendShapeNames.Add(new BlendShapeName { Index = 4, Name = "E", PresetName = "e" });
                BlendShapeNames.Add(new BlendShapeName { Index = 5, Name = "O", PresetName = "o" });
                BlendShapeNames.Add(new BlendShapeName { Index = 6, Name = "Blink", PresetName = "blink" });
                BlendShapeNames.Add(new BlendShapeName { Index = 7, Name = "Joy", PresetName = "joy" });
                BlendShapeNames.Add(new BlendShapeName { Index = 8, Name = "Angry", PresetName = "angry" });
                BlendShapeNames.Add(new BlendShapeName { Index = 9, Name = "Sorrow", PresetName = "sorrow" });
                BlendShapeNames.Add(new BlendShapeName { Index = 10, Name = "Fun", PresetName = "fun" });
                BlendShapeNames.Add(new BlendShapeName { Index = 11, Name = "LookUp", PresetName = "lookup" });
                BlendShapeNames.Add(new BlendShapeName { Index = 12, Name = "LookDown", PresetName = "lookdown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 13, Name = "LookLeft", PresetName = "lookleft" });
                BlendShapeNames.Add(new BlendShapeName { Index = 14, Name = "LookRight", PresetName = "lookright" });
                BlendShapeNames.Add(new BlendShapeName { Index = 15, Name = "Blink_L", PresetName = "blink_l" });
                BlendShapeNames.Add(new BlendShapeName { Index = 16, Name = "Blink_R", PresetName = "blink_r" });
                BlendShapeNames.Add(new BlendShapeName { Index = 17, Name = "BrowDownLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 18, Name = "BrowDownRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 19, Name = "BrowInnerUp", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 20, Name = "BrowOuterUpLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 21, Name = "BrowOuterUpRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 22, Name = "CheekPuff", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 23, Name = "CheekSquintLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 24, Name = "CheekSquintRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 25, Name = "EyeBlinkLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 26, Name = "EyeBlinkRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 27, Name = "EyeLookDownLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 28, Name = "EyeLookDownRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 29, Name = "EyeLookInLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 30, Name = "EyeLookInRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 31, Name = "EyeLookOutLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 32, Name = "EyeLookOutRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 33, Name = "EyeLookUpLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 34, Name = "EyeLookUpRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 35, Name = "EyeSquintLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 36, Name = "EyeSquintRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 37, Name = "EyeWideLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 38, Name = "EyeWideRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 39, Name = "JawForward", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 40, Name = "JawLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 41, Name = "JawOpen", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 42, Name = "JawRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 43, Name = "MouthClose", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 44, Name = "MouthDimpleLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 45, Name = "MouthDimpleRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 46, Name = "MouthFrownLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 47, Name = "MouthFrownRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 48, Name = "MouthFunnel", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 49, Name = "MouthLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 50, Name = "MouthLowerDownLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 51, Name = "MouthLowerDownRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 52, Name = "MouthPressLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 53, Name = "MouthPressRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 54, Name = "MouthPucker", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 55, Name = "MouthRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 56, Name = "MouthRollLower", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 57, Name = "MouthRollUpper", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 58, Name = "MouthShrugLower", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 59, Name = "MouthShrugUpper", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 60, Name = "MouthSmileLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 61, Name = "MouthSmileRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 62, Name = "MouthStretchLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 63, Name = "MouthStretchRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 64, Name = "MouthUpperUpLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 65, Name = "MouthUpperUpRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 66, Name = "NoseSneerLeft", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 67, Name = "NoseSneerRight", PresetName = "unknown" });
                BlendShapeNames.Add(new BlendShapeName { Index = 68, Name = "TongueOut", PresetName = "unknown" });
            }
        }
    }
}
