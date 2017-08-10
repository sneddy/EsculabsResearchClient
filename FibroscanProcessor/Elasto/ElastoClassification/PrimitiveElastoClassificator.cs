using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FibroscanProcessor.Elasto
{
    class PrimitiveElastoClassificator : ElastoClassificator
    {
        const double RMax = 0.9;
        const double AMax = 0.92;
        const double VeryStrongAngleDif = 6;//6
        const double StrongAngleDif = 8;//6
        const double MeanAngleDif = 10;//9
        const double WeakAngleDif = 12;
        const double AngleLimit = 12.5;


        public override VerificationStatus Classiffy(ElastogramSignatura workingSignatura)
        {
            int area = (int) workingSignatura.Area;
            double leftAngle = workingSignatura.LeftAngle;
            double rightAngle = workingSignatura.RightAngle;
            double fibroAngle = workingSignatura.FibroAngle;
            double rSquareLeft = workingSignatura.RSquareLeft;
            double rSquareRight = workingSignatura.RSquareRight;
            double aLeft = workingSignatura.RelativeEstimationLeft;
            double aRight = workingSignatura.RelativeEstimationRight;

            if (area < 4000)
                return VerificationStatus.Uncertain;
            if (area < 6000)
            {
                if ((Math.Abs(leftAngle - fibroAngle) < VeryStrongAngleDif) &&
                    (Math.Abs(leftAngle - rightAngle) < VeryStrongAngleDif) &&
                    (leftAngle> 0) &&
                    IsGoodApproximation(leftAngle, rSquareLeft, aLeft) &&
                    IsGoodApproximation(rightAngle, rSquareRight, aRight))
                    return VerificationStatus.Correct;
                return VerificationStatus.Uncertain;
            }
            if (area < 8000)
            {
                if (!IsGoodApproximation(leftAngle, rSquareLeft, aLeft) ||
                    !IsGoodApproximation(rightAngle, rSquareRight, aRight))
                    return VerificationStatus.Uncertain;
                if ((Math.Abs(leftAngle - fibroAngle) < WeakAngleDif) &&
                    (Math.Abs(leftAngle - rightAngle) < WeakAngleDif) &&
                    (leftAngle > 0))
                    return VerificationStatus.Correct;
                return VerificationStatus.Incorrect;
            }
            if (area < 10000)
            {
                if (!IsGoodApproximation(leftAngle, rSquareLeft, aLeft) ||
                                    !IsGoodApproximation(rightAngle, rSquareRight, aRight))
                    return VerificationStatus.Uncertain;
                if ((Math.Abs(leftAngle - fibroAngle) < MeanAngleDif) &&
                                   (Math.Abs(leftAngle - rightAngle) < MeanAngleDif) &&
                                    (leftAngle > 0))
                    return VerificationStatus.Correct;
                return VerificationStatus.Incorrect;
            }
            if (area < 13500)
            {
                if (!IsGoodApproximation(leftAngle, rSquareLeft, aLeft) ||
                                    !IsGoodApproximation(rightAngle, rSquareRight, aRight))
                    return VerificationStatus.Uncertain;
                if ((Math.Abs(leftAngle - fibroAngle) < StrongAngleDif) &&
                                   (Math.Abs(leftAngle - rightAngle) < StrongAngleDif) &&
                                    (leftAngle > 0))
                    return VerificationStatus.Correct;
                return VerificationStatus.Incorrect;
            }

            if (area < 21000)
            {
                if (IsGoodApproximation(leftAngle, rSquareLeft, aLeft) &&
                    IsGoodApproximation(rightAngle, rSquareRight, aRight))
                    return VerificationStatus.Incorrect;
                return VerificationStatus.Uncertain;
            }
            return VerificationStatus.Uncertain;

        }

        private bool IsGoodApproximation(double angle, double rSquare, double relativeEstimation)
        {
            if ((angle < AngleLimit) && (relativeEstimation > AMax))
                return true;
            if ((angle < AngleLimit) && (relativeEstimation <= AMax))
                return false;
            if (rSquare > RMax)
                return true;
            return false;
        }
    }
}
