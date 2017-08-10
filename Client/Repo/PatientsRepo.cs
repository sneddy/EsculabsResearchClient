using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Repo
{
    using Model;
    using MongoRepository;
    using Common.Logging;

    class PatientsRepo : IRepository
    {
        private static volatile PatientsRepo instance;
        private static object syncRoot = new Object();

        private ILog log;
        private PgContext context = null;

        public static PatientsRepo Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new PatientsRepo();
                    }
                }

                return instance;
            }
        }

        public PatientsRepo()
        {
            log = LogManager.GetLogger("PatientsRepo");

            if (context == null)
            {
                context = new PgContext();
            }
        }

        public Patient Find(int id)
        {
            return context.Patients.Find(id);
        }

        public Patient Add(Patient patient)
        {
            Patient result = null;

            try
            {
                result = context.Patients.Add(patient);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                log.Error(string.Format("Can't insert data to db. Reason: {0}", e.Message));
                result = null;
            }

            return result;
        }

        public List<TablePatient> GetGridList()
        {
            List<Patient> patients = null;
            IOrderedQueryable<Examine> examinesPool = null;

            try {
                MongoRepository<Examine> examines = new MongoRepository<Examine>();

                patients = context.Patients.OrderByDescending(p => p.Id).ToList();
                var patientsIds = patients.Select(p => p.Id).ToList();
                examinesPool = examines.Where(ex => patientsIds.Contains(ex.PatientId))
                    .OrderByDescending(ex => ex.CreatedAt);
            }
            catch (Exception e)
            {
                log.Error(string.Format("Can't select data from db. Reason: {0}", e.Message));
                return null;
            }

            List<TablePatient> tablePatients = new List<TablePatient>();
            foreach (Patient patient in patients)
            {
                Examine examine = examinesPool.Where(ex => ex.PatientId == patient.Id).ToList().FirstOrDefault();

                tablePatients.Add(new TablePatient(patient, examine));
            }

            return tablePatients;
        }
    }
}
