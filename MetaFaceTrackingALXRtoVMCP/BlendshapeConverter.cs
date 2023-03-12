using MetaFaceTrackingALXRtoVMCP.Movement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MetaFaceTrackingALXRtoVMCP
{
    internal class BlendshapeConverter
    {
        public class BlendValueBuffer
        {
            /// <summary>
            /// VPCProtocolに送信するブレンドシェイプ名。
            /// 標準ブレンドシェイプの場合はVRMに格納されているBlendshapeGroupsのpresetNameの値をVMCにあわせて変換したやつ。
            /// 拡張ブレンドシェイプの場合はVRMに格納されているBlendshapeGroupsのnameの値。
            /// </summary>
            public string Name = "";

            /// <summary>
            /// trueの場合、VMCProtocolで送信しない。
            /// 衣装切り替えなど表情以外に使用しているブレンドシェイプを変更したくないときに使用する。
            /// </summary>
            public bool Ignore;

            /// <summary>
            /// VPCProtocolに送信するブレンドシェイプの値。
            /// 0.0f～1.0f。
            /// </summary>
            public float Value;
        }

        public List<BlendshapeMapping> MappingList;

        /// <summary>
        /// VMCProtocolで送信するBlendshapeの値をためるリスト
        /// </summary>
        public BlendValueBuffer[] BlendValueBuffers = Array.Empty<BlendValueBuffer>();

        /// <summary>
        /// VMCProtocolで送信しないBlendshapeのリスト
        /// </summary>
        public List<string> NoSendBlendshapes = new();

        private readonly Vrm _vrm = new();

        public BlendshapeConverter(string vrmFilePath, List<BlendshapeMapping> blendhsapeMappingList, List<string> noSendBlendshapes)
        {
            _vrm.Load(vrmFilePath);

            MappingList = blendhsapeMappingList;

            noSendBlendshapes.ForEach(item =>
            {
                if (item.Length > 0)
                {
                    NoSendBlendshapes.Add(item);
                }
            });
            ConstructBlendValueBuffers();
        }

        private void ConstructBlendValueBuffers()
        {
            // BlendshapeGroupsのpresetNameはすべて小文字。
            // VMC0.52b19だと標準ブレンドシェイプの名前にはenum BlendShapePresetをtoStringした値を使っているので一致するよう変換しておく。
            // (case insensitiveなので変更不要だけどVMC側で処理が減るようにあわせておく)
            var presetNameForVMC = new Dictionary<string, string>
                {
                    { "NEUTRAL", "Neutral" },
                    { "A", "A" },
                    { "I", "I" },
                    { "U", "U" },
                    { "E", "E" },
                    { "O", "O" },
                    { "BLINK", "Blink" },
                    { "JOY", "Joy" },
                    { "ANGRY", "Angry" },
                    { "SORROW", "Sorrow" },
                    { "FUN", "Fun" },
                    { "LOOKUP", "LookUp" },
                    { "LOOKDOWN", "LookDown" },
                    { "LOOKLEFT", "LookLeft" },
                    { "LOOKRIGHT", "LookRight" },
                    { "BLINK_L", "Blink_L" },
                    { "BLINK_R", "Blink_R" },
                };

            BlendValueBuffers = new BlendValueBuffer[_vrm.BlendShapeNames.Count];

            var index = -1;
            _vrm.BlendShapeNames.ForEach(blendshapeName =>
            {
                index += 1;
                var buffer = new BlendValueBuffer
                {
                    Ignore = NoSendBlendshapes.Exists(x => x.ToLower().Equals(blendshapeName.Name.ToLower())),
                    Value = 0.0f
                };

                if (string.Compare(blendshapeName.PresetName, "unknown", true) == 0)
                {
                    buffer.Name = blendshapeName.Name;
                }
                else
                {
                    if (presetNameForVMC.ContainsKey(blendshapeName.PresetName.ToUpper()))
                    {
                        buffer.Name = presetNameForVMC[blendshapeName.PresetName.ToUpper()];
                    }
                    else
                    {
                        buffer.Name = blendshapeName.PresetName;
                    }
                }

                BlendValueBuffers[index] = buffer;

                MappingList.ForEach(mapping =>
                {
                    if (mapping.BlendshapeName.ToLower().Equals(blendshapeName.Name.ToLower()))
                    {
                        mapping.BlendshapeIndex = index;
                    }
                });
            });
        }

        public void ApplyTrackingData(Movement.Movement movement)
        {
            foreach (var proxy in BlendValueBuffers)
            {
                proxy.Value = 0;
            }

            MappingList.ForEach(mapping =>
            {
                if (mapping.BlendshapeIndex >= 0)
                {
                    var weight = movement.Face[mapping.FaceExpression];
                    var reweight = mapping.GetCurveY(weight);
                    BlendValueBuffers[mapping.BlendshapeIndex].Value += (float)reweight;
                }
            });
        }
    }
}
