using System;
using Eklekto.Geometry;

namespace FibroscanProcessor.Elasto
{
    public class OldElastoClassificator
    {
        const double RMax = 0.9;
        const double AMax = 0.92;
        const double VeryStrongAngleDif = 6;//6
        const double StrongAngleDif = 8;//6
        const double MeanAngleDif = 10;//9
        const double WeakAngleDif = 12;
        const double AngleLimit = 12.5;


        public VerificationStatus Classiffy(ElastoBlob targetObject, Segment fibroLine)
        {
            if (CheckForNull(targetObject,fibroLine)) return VerificationStatus.NotCalculated;
            int area = targetObject.Blob.Area;
            ReflectionedLine leftLine = targetObject.LeftApproximation;
            ReflectionedLine rightLine = targetObject.RightApproximation;
            double rSquareLeft = targetObject.RSquareLeft;
            double rSquareRight = targetObject.RSquareRight;
            double aLeft = targetObject.RelativeEstimationLeft;
            double aRight = targetObject.RelativeEstimationRight;


            if (area < 4000)
                return VerificationStatus.Uncertain;
            if (area < 6000)
            {
                if ((angleDifference(leftLine, fibroLine.Equation)<VeryStrongAngleDif) &&
                    (angleDifference(leftLine, rightLine) < VeryStrongAngleDif) &&
                    (IsGoodTilt(leftLine)) &&
                    IsGoodApproximation(leftLine, rSquareLeft, aLeft) &&
                    IsGoodApproximation(rightLine, rSquareRight, aRight))
                    return VerificationStatus.Correct;
                return VerificationStatus.Uncertain;
            }
            if (area < 8000)
            {
                if (!IsGoodApproximation(leftLine, rSquareLeft, aLeft) ||
                    !IsGoodApproximation(rightLine, rSquareRight, aRight))
                    return VerificationStatus.Uncertain;
                if ((angleDifference(leftLine, fibroLine.Equation) < WeakAngleDif) &&
                    (angleDifference(leftLine, rightLine) < WeakAngleDif) &&
                    (IsGoodTilt(leftLine)))
                    return VerificationStatus.Correct;
                return VerificationStatus.Incorrect;
            }
            if (area < 10000)
            {
                if (!IsGoodApproximation(leftLine, rSquareLeft, aLeft) ||
                                    !IsGoodApproximation(rightLine, rSquareRight, aRight))
                    return VerificationStatus.Uncertain;
                if ((angleDifference(leftLine, fibroLine.Equation) < MeanAngleDif) &&
                    (angleDifference(leftLine, rightLine) < MeanAngleDif) &&
                    (IsGoodTilt(leftLine)))
                    return VerificationStatus.Correct;
                return VerificationStatus.Incorrect;
            }
            if (area < 13500)
            {
                if (!IsGoodApproximation(leftLine, rSquareLeft, aLeft) ||
                                    !IsGoodApproximation(rightLine, rSquareRight, aRight))
                    return VerificationStatus.Uncertain;
                if ((angleDifference(leftLine, fibroLine.Equation) < StrongAngleDif) &&
                    (angleDifference(leftLine, rightLine) < StrongAngleDif) &&
                    (IsGoodTilt(leftLine)))
                    return VerificationStatus.Correct;
                return VerificationStatus.Incorrect;
            }

            if (area < 21000)
            {
                if (IsGoodApproximation(leftLine, rSquareLeft, aLeft) &&
                    IsGoodApproximation(rightLine, rSquareRight, aRight))
                    return VerificationStatus.Incorrect;
                return VerificationStatus.Uncertain;
            }
            return VerificationStatus.Uncertain;

        }

        private double angleDifference(ReflectionedLine firstLine, ReflectionedLine secondLine)
        {
            return Math.Abs(firstLine.Angle - secondLine.Angle);
        }

        private bool IsGoodTilt(ReflectionedLine line)
        {
            if (Math.Atan(line.A) > 0)
                return true;
             return false;
        }

        private bool IsGoodApproximation(ReflectionedLine approxLine, double rSquare, double relativeEstimation)
        {
            double angle = Math.Abs(approxLine.Angle);
            if ((angle < AngleLimit) && (relativeEstimation > AMax))
                    return true;
            if ((angle < AngleLimit) && (relativeEstimation <= AMax))
                    return false;
            if (rSquare > RMax)
                    return true;
            return false;
        }

        private bool CheckForNull(ElastoBlob targetObject, Segment fibroLine)
        {
            if (targetObject.Blob.Area == 0)
                return true;

            if (targetObject.LeftApproximation == null)
                return true;

            if (targetObject.RightApproximation == null)
                return true;

            if (fibroLine == null)
                return true;

            if (Math.Abs(targetObject.RSquareLeft) < Double.Epsilon)
                return true;

            if (Math.Abs(targetObject.RSquareLeft) < Double.Epsilon)
                return true;

            if (Math.Abs(targetObject.RSquareRight) < Double.Epsilon)
                return true;

            if (Math.Abs(targetObject.RelativeEstimationRight) < Double.Epsilon)
                return true;

            if (Math.Abs(targetObject.RelativeEstimationLeft) < Double.Epsilon)
                return true;
            return false;
        }
    }
}
