using System;
using System.Collections.Generic;

namespace FibroscanProcessor.Elasto
{
    public class AgeTrainInfo
    {
        public int Count;
        public int Errors;
        public int TeachingNum;

        public AgeTrainInfo(int count, int error, int teachingNum)
        {
            Count = count;
            Errors = error;
            TeachingNum = teachingNum;
        }

        public List<int> infoList => new List<int>
        {
            Errors,
            TeachingNum
        };
    }
}
