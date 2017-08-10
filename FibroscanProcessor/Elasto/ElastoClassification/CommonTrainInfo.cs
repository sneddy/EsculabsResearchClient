using System.Collections.Generic;
using System.Linq;

namespace FibroscanProcessor.Elasto
{
    public class CommonTrainInfo
    {
        public List<AgeTrainInfo> Info;
        public bool IsTrained;
        public readonly int ErrorLimit;
        public readonly int ConvergenceThreshold;
        
        public CommonTrainInfo(int errorThreshold, int convergenceThreshold)
        {
            IsTrained = false;
            Info = new List<AgeTrainInfo>();
            ErrorLimit = errorThreshold;
            ConvergenceThreshold = convergenceThreshold;
        }

        public void AddAge(AgeTrainInfo ageInfo)
        {
            Info.Add(ageInfo);
        }

        //Very-very stupid conditions
        public bool IsConvergence()
        {
            if (Info.Last().TeachingNum < ConvergenceThreshold)
                return true;
            return false;
        }

        public bool IsFailedLearning()
        {
            if (Info.Last().Errors > ErrorLimit)
                return true;
            return false;
        }
    }
}
