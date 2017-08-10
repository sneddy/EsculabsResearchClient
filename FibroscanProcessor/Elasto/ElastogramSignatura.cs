using System;
using System.Collections.Generic;
using Eklekto.Geometry;

namespace FibroscanProcessor.Elasto
{
    public class ElastogramSignatura
    {
        public static int Size = 8;

        public readonly double Area;
        public readonly double FibroAngle;
        public readonly double LeftAngle;
        public readonly double RightAngle;
        public readonly double RSquareLeft;
        public readonly double RSquareRight;
        public readonly double RelativeEstimationLeft;
        public readonly double RelativeEstimationRight;

        public VerificationStatus Answer;

        #region Constants
        private const double AreaNormalizator = (double)1/21000;
        private const int Accuracy = 4;

        #endregion    

        #region Property
        public List<double> Data => new List<double>
        {
            Area,
            FibroAngle,
            LeftAngle,
            RightAngle,
            RSquareLeft,
            RSquareRight,
            RelativeEstimationLeft,
            RelativeEstimationRight
        };

        public List<double> NormalizedSignatura => new List<double>
        {
                    Math.Round(Area * AreaNormalizator, Accuracy),
                    Math.Round(Math.Tan(FibroAngle) / Math.PI + 0.5, Accuracy),
                    Math.Round(Math.Tan(LeftAngle) / Math.PI + 0.5, Accuracy),
                    Math.Round(Math.Tan(RightAngle) / Math.PI + 0.5, Accuracy),
                    Math.Round(RSquareLeft / 100, Accuracy),
                    Math.Round(RSquareRight / 100, Accuracy),
                    Math.Round(RelativeEstimationLeft / 100, Accuracy),
                    Math.Round(RelativeEstimationRight / 100, Accuracy)
        };
        #endregion
        
        #region Constructors
        public ElastogramSignatura(List<double> inputSignature, VerificationStatus answer = VerificationStatus.NotCalculated, 
            bool isNormalized = false)
        {
            if (inputSignature.Count != Size)
                throw new ArgumentException();
            if (isNormalized)
            {
                Area = inputSignature[0] / AreaNormalizator;
                FibroAngle = Math.Atan((inputSignature[1] - 0.5)*Math.PI);
                LeftAngle = Math.Atan((inputSignature[2] - 0.5) * Math.PI);
                RightAngle = Math.Atan((inputSignature[3] - 0.5) * Math.PI);
                RSquareLeft = inputSignature[4] * 100;
                RSquareRight = inputSignature[5] * 100;
                RelativeEstimationLeft = inputSignature[6] * 100;
                RelativeEstimationRight = inputSignature[7] * 100;
            }
            else
            {
                Area = inputSignature[0];
                FibroAngle = inputSignature[1];
                LeftAngle = inputSignature[2];
                RightAngle = inputSignature[3];
                RSquareLeft = inputSignature[4];
                RSquareRight = inputSignature[5];
                RelativeEstimationLeft = inputSignature[6];
                RelativeEstimationRight = inputSignature[7];
            }

            Answer = answer;
        }

        public ElastogramSignatura(double area, double fibroAngle,
            double leftAngle, double rightAngle,
            double rSquareLeft, double rSquareRight,
            double relativeEstimationLeft, double relativeEstimationRight,
            VerificationStatus answer = VerificationStatus.NotCalculated)
        {
            Area = area;
            FibroAngle = fibroAngle;
            LeftAngle = leftAngle;
            RightAngle = rightAngle;
            RSquareLeft = rSquareLeft;
            RSquareRight = rSquareRight;
            RelativeEstimationLeft = relativeEstimationLeft;
            RelativeEstimationRight = relativeEstimationRight;

            Answer = answer;
        }

        public ElastogramSignatura(ElastoBlob inputBlob, Segment inputFibroLine, VerificationStatus answer = VerificationStatus.NotCalculated)
        {
            Area = inputBlob.Blob.Area;
            FibroAngle = inputFibroLine.Equation.Angle;
            LeftAngle = inputBlob.LeftApproximation.Angle;
            RightAngle = inputBlob.RightApproximation.Angle;
            RSquareLeft = inputBlob.RSquareLeft;
            RSquareRight = inputBlob.RSquareRight;
            RelativeEstimationLeft = inputBlob.RelativeEstimationLeft;
            RelativeEstimationRight = inputBlob.RelativeEstimationRight;

            Answer = answer;
        }
        #endregion


    }
}
