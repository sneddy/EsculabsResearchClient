using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public partial class TablePatient
    {
        public TablePatient() { }

        public TablePatient(Patient Patient, Examine LastExamine)
        {
            Id = Patient.Id;
            Name = string.Format("{0} {1} {2}", Patient.LastName, Patient.FirstName, Patient.MiddleName);

            if (LastExamine != null)
            {
                PhibrosisStage = LastExamine.ElastoExam.PhibrosisStage;
                LastExamineDate = LastExamine.CreatedAt;
                ExpertStatus = LastExamine.ElastoExam.ExpertStatus.ToString().ToLower();

                LocalStatus = LastExamine.ElastoExam.Valid ? "correct" : "incorrect";
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string PhibrosisStage { get; set; }

        public DateTime? LastExamineDate { get; set; }

        public string LocalStatus { get; set; }

        public string ExpertStatus { get; set; }

        public int PatientId { get; set; }

        public bool Filter(string name, DateTime? dateFrom, DateTime? dateTo)
        {
            if (!dateFrom.HasValue && !dateTo.HasValue)
            {
                return Name.ToLower().Contains(name.ToLower());
            }
            else
            {
                if (!dateFrom.HasValue)
                {
                    dateFrom = DateTime.MinValue;
                }

                if (!dateTo.HasValue)
                {
                    dateTo = DateTime.MaxValue;
                }

                return Name.ToLower().Contains(name.ToLower()) && LastExamineDate.HasValue &&
                    LastExamineDate.Value.Date >= dateFrom.Value.Date && LastExamineDate.Value.Date <= dateTo.Value.Date;
            }
        }
    }
}
