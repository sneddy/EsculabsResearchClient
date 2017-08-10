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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Client
{
    using Model;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;
    using MongoDB.Driver.GridFS;
    using MongoDB.Driver.Linq;
    using MongoRepository;
    using System.Data.Entity;
    using System.Windows.Controls.Primitives;
    using Repo;
    using System.Globalization;

    /// <summary>
    /// Interaction logic for PatientsWindow.xaml
    /// </summary>
    public partial class PatientsWindow : Window
    {
        private const string DatePickerWatermark = "Выберите дату";

        private List<TablePatient> patients = new List<TablePatient> { };

        public PatientsWindow()
        {
            InitializeComponent();
        }

        public void RefreshPatientsList()
        {
            nameFilter.Text = "";
            fromDateFilter.SelectedDate = null;
            toDateFilter.SelectedDate = null;

            patients = PatientsRepo.Instance.GetGridList();
            patientsGrid.ItemsSource = patients;
        }

        private void RefreshFilter()
        {
            IEnumerable<TablePatient> list = patients;
            string name = nameFilter.Text;
            DateTime? dateFrom = fromDateFilter.SelectedDate;
            DateTime? dateTo = toDateFilter.SelectedDate;

            list = list.Where(x => (x.Filter(name, dateFrom, dateTo)));

            patientsGrid.ItemsSource = list;
        }
        
        private void ShowPatientExamines(object sender, RoutedEventArgs e)
        {
            TablePatient tablePatient = ((FrameworkElement)sender).DataContext as TablePatient;

            ExaminesWindow window = new ExaminesWindow(PatientsRepo.Instance.Find(tablePatient.Id));
            window.Owner = this;
            window.ShowDialog();

            RefreshPatientsList();
        }

        private void newPatientBtn_Click(object sender, RoutedEventArgs e)
        {
            NewPatientWindow window = new NewPatientWindow();
            window.Owner = this;
            window.ShowDialog();
            RefreshPatientsList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            fromDateFilter.SetWatermarkText(DatePickerWatermark);
            toDateFilter.SetWatermarkText(DatePickerWatermark);

            patients = PatientsRepo.Instance.GetGridList();

            if (patients == null || !patients.Any())
            {
                return;
            }

            patientsGrid.ItemsSource = patients;
        }

        private void nameFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshFilter();
        }

        private void dateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshFilter();
        }

        private void clearDateFilterButton_Click(object sender, RoutedEventArgs e)
        {
            fromDateFilter.SelectedDate = null;
            toDateFilter.SelectedDate = null;
        }

        private void clearNameFilterButton_Click(object sender, RoutedEventArgs e)
        {
            nameFilter.Text = "";
        }

        private void patientsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            nameFilter.IsEnabled = true;
            fromDateFilter.IsEnabled = true;
            toDateFilter.IsEnabled = true;
            clearNameFilterButton.IsEnabled = true;
            clearDateFilterButton.IsEnabled = true;
        }

        private void logoutBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
