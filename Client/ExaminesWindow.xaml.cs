using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    using Microsoft.Win32;
    using Model;
    using MongoRepository;
    using System.IO;
    using System.Windows.Media.Effects;

    /// <summary>
    /// Interaction logic for ExaminesWindow.xaml
    /// </summary>
    public partial class ExaminesWindow : Window
    {
        private readonly Patient _patient;
        private List<TableExamine> _examines = new List<TableExamine>();

        public ExaminesWindow()
        {
            InitializeComponent();
        }

        public ExaminesWindow(Patient patient) : this()
        {
            _patient = patient;

            if (_patient != null)
            {
                RefreshExaminesList();
            }
        }

        private void RefreshExaminesList()
        {
            var examinesRepo = new MongoRepository<Examine>();

            var patientExamines = examinesRepo.Where(ex => ex.PatientId == _patient.Id)
                .OrderByDescending(ex => ex.CreatedAt).ToList();

            if (_examines != null && _examines.Any())
            {
                _examines.Clear();
            }

            _examines = new List<TableExamine>();

            var i = patientExamines.Count;
            foreach (var examine in patientExamines)
            {
                _examines.Add(new TableExamine(examine, i));
                --i;
            }
        }

        private void newMeasureBtn_Click(object sender, RoutedEventArgs e)
        {
            //ExamineWindow window = new ExamineWindow();
            //window.ShowDialog();
        }

        private void ShowExamine(object sender, RoutedEventArgs e)
        {
            TableExamine tableExamine = ((FrameworkElement)sender).DataContext as TableExamine;

            MongoRepository<Examine> examinesRepo = new MongoRepository<Examine>();
            var examine = examinesRepo.FirstOrDefault(x => x.Id == tableExamine.Guid);

            if (examine != null)
            {
                var window = new ExamineWindow(_patient, examine);
                window.Owner = this;
                window.ShowDialog();
                RefreshExaminesList();
            }
            else
            {
                MessageBox.Show("Не удалось найти обследование");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_patient != null)
            {
                nameLabel.Content = $"{_patient.LastName} {_patient.FirstName} {_patient.MiddleName}";
                examinesGrid.ItemsSource = _examines;
            }
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void importFbixBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            var blur = new BlurEffect { Radius = 4 };
            Effect = blur;

            var loader = new LoaderWindow { Owner = this };
            loader.Show();

            var examine = FibxParser.Instance.Import(openFileDialog.FileName, _patient.Id);

            if (examine != null && IsLoaded)
            {
                RefreshExaminesList();
                examinesGrid.ItemsSource = _examines;
            }

            loader.Close();
            Effect = null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var window = Application.Current.MainWindow as PatientsWindow;

            if (window == null)
            {
                return;
            }

            window.RefreshPatientsList();
            window.Activate();
        }

 
    }
}
