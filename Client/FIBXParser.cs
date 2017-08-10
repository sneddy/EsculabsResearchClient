using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Xml;
using Common.Logging;

namespace Client
{
    using FibroscanProcessor;
    using Model;
    using MongoRepository;
    using System.Globalization;
    using System.Windows;
    using System.Xml.Linq;

    class FibxParser
    {
        private static volatile FibxParser _instance;
        private static object _syncRoot = new object();

        private readonly ILog _log;

        public FibxParser()
        {
            _log = LogManager.GetLogger("FibxParser");
        }

        public static FibxParser Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new FibxParser();
                    }
                }

                return _instance;
            }
        }

        public Examine Import(string fileName, int patientId, int physicianId = 0)
        {
            if (string.IsNullOrEmpty(fileName) || patientId <= 0)
            {
                return null;
            }
            
            var tempPath = Path.GetTempPath();
            var slash = Path.DirectorySeparatorChar;

            if (string.IsNullOrEmpty(tempPath))
            {
                return null;
            }

            Examine e = null;

            tempPath += slash + "Balder";
            Directory.CreateDirectory(tempPath);

            tempPath += slash + Guid.NewGuid().ToString();
            Directory.CreateDirectory(tempPath);

            ZipFile.ExtractToDirectory(fileName, tempPath);

            var examReportFile = $"{tempPath}{slash}ExamReport.xml";
            if (File.Exists(examReportFile))
            {
                XDocument xml;
                try
                {
                    xml = XDocument.Load(examReportFile);

                }
                catch (XmlException exception)
                {
                    _log.ErrorFormat("Can't parse ExamReport.xml in \"{0}\": {1}", fileName, exception.Message);
                    MessageBox.Show("FIBX-файл повреждён или имеет неизвестный формат");

                    return null;
                }

                try
                {
                    e = new Examine();
                    e.ElastoExam = new ElastoExam();

                    var exam = xml.Descendants("Exam").FirstOrDefault();

                    e.CreatedAt = DateTime.Parse(exam.Descendants("Date").FirstOrDefault().Value);
                    e.PhysicianId = physicianId;
                    e.PatientId = patientId;

                    var result = exam.Descendants("Result").FirstOrDefault();

                    var sensorType = SensorType.Small;
                    switch (exam.Descendants("ExamType").FirstOrDefault().Value)
                    {
                    case ("S"):
                    case ("Small"):
                        sensorType = SensorType.Small;
                        break;
                    case ("M"):
                    case ("Medium"):
                        sensorType = SensorType.Medium;
                        break;
                    case ("XL"):
                        sensorType = SensorType.XL;
                        break;
                    default:
                        sensorType = SensorType.Small;
                        break;
                    }

                    e.ElastoExam.SensorType = sensorType;
                    e.ElastoExam.IQR = double.Parse(result.Descendants("StiffnessIQR").FirstOrDefault().Value, CultureInfo.InvariantCulture);
                    e.ElastoExam.Med = double.Parse(result.Descendants("StiffnessMedian").FirstOrDefault().Value, CultureInfo.InvariantCulture);
                    e.ElastoExam.Duration = int.Parse(result.Descendants("ExamDuration").FirstOrDefault().Value);
                    e.ElastoExam.WhiskerPlot = ImageFileToBase64(tempPath + slash + result.Descendants("WhiskerPlotImageLink").FirstOrDefault().Value);
                    e.ElastoExam.ExpertStatus = ExpertStatus.Pending;
                        
                    e.ElastoExam.Measures = new List<Measure>();
                    foreach (var measure in exam.Descendants("Measurements").FirstOrDefault().Descendants("Measure"))
                    {
                        Measure m = new Measure();

                        m.CreatedAt = DateTime.Parse(exam.Descendants("Time").FirstOrDefault().Value);
                        m.Stiffness = double.Parse(measure.Descendants("Stiffness").FirstOrDefault().Value, CultureInfo.InvariantCulture);

                        var sourceFile = tempPath + slash + measure.Descendants("ImageLink").FirstOrDefault().Value;

                        Image source = Image.FromFile(sourceFile);

                        FibroscanImage prod = new FibroscanImage(source);
                        m.ResultMerged = ImageToBase64(prod.Merged);
                        m.Source = ImageFileToBase64(sourceFile);

                        e.ElastoExam.Measures.Add(m);
                    }

                    // TODO: Validation check
                    e.ElastoExam.Valid = e.ElastoExam.Validate();

                    var examines = new MongoRepository<Examine>();
                    examines.Add(e);                        
                }
                catch (Exception exception)
                {
                    _log.ErrorFormat("Can't parse FIBX: {0}", fileName, exception.Message);
                    MessageBox.Show("Не удалось распознать FIBX-файл");
                    e = null;
                }
            }
            else
            {
                MessageBox.Show("ExamReport.xml не найден в файле\n\n" + fileName + "\n\nОперация прервана.");
            }

            Directory.Delete(tempPath, true);

            return e;
        }

        private string ImageFileToBase64(string fileName)
        {
            using (Image image = Image.FromFile(fileName))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    m.Close();

                    return Convert.ToBase64String(imageBytes);
                }
            }
        }

        private string ImageToBase64(Image image)
        {
            using (image)
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    m.Close();

                    return Convert.ToBase64String(imageBytes);
                }
            }
        }
    }
}
