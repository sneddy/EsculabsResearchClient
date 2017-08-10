using System;
using System.Collections.Generic;
using System.Linq;

namespace FibroscanProcessor.Elasto
{
    public class LearningElastoClassificator:ElastoClassificator
    {
        public List<ElastogramSignatura> TrainingSignatura;
        public List<ElastogramSignatura> Precedents;
        public List<double> SignatureRadiuses;
        

        public LearningElastoClassificator(List<ElastogramSignatura> trainingSignatura, List<ElastogramSignatura> precedents, 
              List<double> signatureRadiuses)
        {
            TrainingSignatura = trainingSignatura;
            Precedents = precedents;
            SignatureRadiuses = signatureRadiuses;
        }

        public AgeTrainInfo TrainOneAge(int ageSize, int startIndex)
        {
            int errors = 0;
            int precedentAdding = 0;
            //run for TrainingSignatures
            for (int localIndex = 0; localIndex < ageSize; localIndex++)
            {
                int globalIndex = startIndex + localIndex;
                //run for precedents
                bool isClassifiedSignature = false;
                foreach (ElastogramSignatura precedent in Precedents)
                {
                    if (IsSignatureCloseness(precedent, TrainingSignatura[globalIndex]) &&
                        (precedent.Answer != TrainingSignatura[globalIndex].Answer))
                    {
                        errors++;
                        isClassifiedSignature = true;
                        break;
                    }
                    if (IsSignatureCloseness(precedent, TrainingSignatura[globalIndex]) &&
                        (precedent.Answer == TrainingSignatura[globalIndex].Answer))
                    {
                        isClassifiedSignature = true;
                        break;
                    }
                }
                if (!isClassifiedSignature)
                {
                    Precedents.Add(TrainingSignatura[globalIndex]);
                    precedentAdding++;
                }
            }
            return new AgeTrainInfo(ageSize, errors, precedentAdding);
        }

        /// <summary>
        /// Adding new precedents
        /// </summary>
        /// <param name="ageSize">Size of one age</param>
        /// <param name="maxAgeIterations">Limit count of ages</param>
        /// <param name="goodTeachingRatio">convergence condition</param>
        /// <param name="errorPiece">maximum piece of errors</param>
        /// <returns>train successeful? </returns>
        public CommonTrainInfo Train(int ageSize, int maxAgeIterations, double goodTeachingPiece, double errorPiece  )
        {
            int maxAgeNumber = Math.Min(maxAgeIterations, TrainingSignatura.Count / ageSize);
            int errorLimitPerAge = (int) errorPiece*ageSize;
            int convergenceNum = (int) goodTeachingPiece*ageSize;
            CommonTrainInfo TeachInformation = new CommonTrainInfo(errorLimitPerAge, convergenceNum);

            for (int ageNumber = 0; ageNumber < maxAgeNumber; ageNumber++)
            {
                AgeTrainInfo ageInfo = TrainOneAge(ageSize, ageNumber * ageSize);
                TeachInformation.AddAge(ageInfo);
                if (TeachInformation.IsFailedLearning())
                {
                    return TeachInformation;
                }

                if (TeachInformation.IsConvergence())
                {
                    TeachInformation.IsTrained = true;
                    return TeachInformation;
                }
            }
            TeachInformation.IsTrained = true;//!!!mb temp!!!
            return TeachInformation;
        } 

        /// <summary>
        /// Run on all precedents and search first closing
        /// </summary>
        /// <param name="workingSignatura"></param>
        /// <returns> verification status of trained classificator</returns>
        public override VerificationStatus Classiffy(ElastogramSignatura workingSignatura)
        {
            if (Precedents.Count==0)
                return VerificationStatus.NotCalculated;

            return
                (from precedent in Precedents
                    where IsSignatureCloseness(precedent, workingSignatura)
                    select precedent.Answer).
                    DefaultIfEmpty(VerificationStatus.NotCalculated).First();
        }

        /// <summary>
        /// Check out a couple of signatures to closeness
        /// </summary>
        /// <param name="firstSign"></param>
        /// <param name="secondSign"></param>
        /// <returns></returns>
        public bool IsSignatureCloseness(ElastogramSignatura firstSign, ElastogramSignatura secondSign)
        {
            List<double> firstNormalizedSign = firstSign.NormalizedSignatura;
            List<double> secondNormalizedSign = secondSign.NormalizedSignatura;
            for (int i = 0; i < ElastogramSignatura.Size; i++)
                if (Math.Abs(firstNormalizedSign[i] - secondNormalizedSign[i]) >= SignatureRadiuses[i])
                    return false;
            return true;
        }
    }
}
