using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaFaceTrackingALXRtoVMCP.Movement
{
    internal class Movement
    {
        public Face Face { get; } = new();
        public Gaze[] Gazes { get; } = new Gaze[2];
        public Gaze GazeL { get { return Gazes[0]; } }
        public Gaze GazeR { get { return Gazes[1]; } }

        public Movement()
        {
            Gazes[0] = new Gaze();
            Gazes[1] = new Gaze();
        }

        /// <summary>
        /// alxr-client(korejan改造版)が送信してきたデータでトラッキングデータを更新する。
        /// </summary>
        /// <param name="rawdata">alxr-client(korejan改造版)にTCP接続すると送信してくるデータ。FaceExpression 63個、左目、右目のデータが格納されている。</param>
        public void Update(byte[] rawdata)
        {
            // rawdataをfloat[]にコピーしたとすると以下のデータが格納されている。
            // rawdataf[0] Brow_Lowerer_L
            // ...
            // rawdataf[62] Upper_Lip_Raiser_R
            // rawdataf[63] gaze[0].isValid
            // rawdataf[64] gaze[0].orientation.x
            // rawdataf[65] gaze[0].orientation.y
            // rawdataf[66] gaze[0].orientation.z
            // rawdataf[67] gaze[0].orientation.w
            // rawdataf[68] gaze[0].position.x
            // rawdataf[69] gaze[0].position.y
            // rawdataf[70] gaze[0].position.z
            // rawdataf[71] gaze[1].isValid
            // rawdataf[72] gaze[1].orientation.x
            // rawdataf[73] gaze[1].orientation.y
            // rawdataf[74] gaze[1].orientation.z
            // rawdataf[75] gaze[1].orientation.w
            // rawdataf[76] gaze[1].position.x
            // rawdataf[77] gaze[1].position.y
            // rawdataf[78] gaze[1].position.z

            int faceItem = (int)Face.Expression.Max;
            int gazeItem = 8;

            Face.Update(rawdata);
            GazeL.Update(rawdata, faceItem * 4);
            GazeR.Update(rawdata, (faceItem + gazeItem) * 4);
        }
    }
}
