using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public partial class TableExamine
    {
        public TableExamine() { }

        public TableExamine(Examine examine, int id = 0)
        {
            if (examine != null)
            {
                Guid = examine.Id;
                Id = id == 0 ? examine.Id : id.ToString();

                PhibrosisStage = examine.ElastoExam.PhibrosisStage;
                CreatedAt = examine.CreatedAt;
                ExpertStatus = examine.ElastoExam.ExpertStatus.ToString().ToLower();
                LocalStatus = examine.ElastoExam.Valid ? "correct" : "incorrect";
                
                MeasuresCount = examine.ElastoExam.Measures.Count();
                Med = examine.ElastoExam.Med;

                var iqrMed = examine.ElastoExam.IQRMed;
                IQRMed = iqrMed > 0 ? iqrMed.ToString() + "%" : "Нет данных";
            }
        }

        public string Guid { get; set; }
        
        public string Id { get; set; }
        
        public int MeasuresCount { get; set; }

        public string IQRMed { get; set; }

        public double Med { get; set; }

        public string PhibrosisStage { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string LocalStatus { get; set; }

        public string ExpertStatus { get; set; }

        public int PatientId { get; set; }
    }
}
