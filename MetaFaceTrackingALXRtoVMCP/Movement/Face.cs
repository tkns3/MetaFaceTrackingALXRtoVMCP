using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaFaceTrackingALXRtoVMCP.Movement
{
    internal class Face
    {
        public enum Expression : int
        {
            BrowLowererL = 0,
            BrowLowererR = 1,
            CheekPuffL = 2,
            CheekPuffR = 3,
            CheekRaiserL = 4,
            CheekRaiserR = 5,
            CheekSuckL = 6,
            CheekSuckR = 7,
            ChinRaiserB = 8,
            ChinRaiserT = 9,
            DimplerL = 10,
            DimplerR = 11,
            EyesClosedL = 12,
            EyesClosedR = 13,
            EyesLookDownL = 14,
            EyesLookDownR = 15,
            EyesLookLeftL = 16,
            EyesLookLeftR = 17,
            EyesLookRightL = 18,
            EyesLookRightR = 19,
            EyesLookUpL = 20,
            EyesLookUpR = 21,
            InnerBrowRaiserL = 22,
            InnerBrowRaiserR = 23,
            JawDrop = 24,
            JawSidewaysLeft = 25,
            JawSidewaysRight = 26,
            JawThrust = 27,
            LidTightenerL = 28,
            LidTightenerR = 29,
            LipCornerDepressorL = 30,
            LipCornerDepressorR = 31,
            LipCornerPullerL = 32,
            LipCornerPullerR = 33,
            LipFunnelerLB = 34,
            LipFunnelerLT = 35,
            LipFunnelerRB = 36,
            LipFunnelerRT = 37,
            LipPressorL = 38,
            LipPressorR = 39,
            LipPuckerL = 40,
            LipPuckerR = 41,
            LipStretcherL = 42,
            LipStretcherR = 43,
            LipSuckLB = 44,
            LipSuckLT = 45,
            LipSuckRB = 46,
            LipSuckRT = 47,
            LipTightenerL = 48,
            LipTightenerR = 49,
            LipsToward = 50,
            LowerLipDepressorL = 51,
            LowerLipDepressorR = 52,
            MouthLeft = 53,
            MouthRight = 54,
            NoseWrinklerL = 55,
            NoseWrinklerR = 56,
            OuterBrowRaiserL = 57,
            OuterBrowRaiserR = 58,
            UpperLidRaiserL = 59,
            UpperLidRaiserR = 60,
            UpperLipRaiserL = 61,
            UpperLipRaiserR = 62,
            Max = 63
        }

        private readonly float[] expressions = new float[(int)Face.Expression.Max];

        public Face()
        {
        }

        public void Update(byte[] rawdata)
        {
            Buffer.BlockCopy(rawdata, 0, expressions, 0, expressions.Length * 4);

            // Recover true eyes closed values
            EyesClosedL += EyesLookDownL;
            EyesClosedR += EyesLookDownR;
        }

        public float this[Expression expression]
        {
            get
            {
                return this.expressions[(int)expression];
            }
        }

        public float this[int expression]
        {
            get
            {
                return this.expressions[expression];
            }
        }

        public float EyesClosedL
        {
            get { return expressions[(int)Face.Expression.EyesClosedL]; }
            set { expressions[(int)Face.Expression.EyesClosedL] = value; }
        }

        public float EyesClosedR
        {
            get { return expressions[(int)Face.Expression.EyesClosedR]; }
            set { expressions[(int)Face.Expression.EyesClosedR] = value; }
        }

        public float EyesLookDownL
        {
            get { return expressions[(int)Expression.EyesLookDownL]; }
            set { expressions[(int)Expression.EyesLookDownL] = value; }
        }

        public float EyesLookDownR
        {
            get { return expressions[(int)Expression.EyesLookDownR]; }
            set { expressions[(int)Expression.EyesLookDownR] = value; }
        }
    }
}
