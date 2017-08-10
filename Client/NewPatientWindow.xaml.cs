using Common.Logging;
using Common.Logging.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    using Model;
    using Repo;
    using System.Globalization;

    /// <summary>
    /// Interaction logic for NewPatient.xaml
    /// </summary>
    public partial class NewPatientWindow : Window
    {
        private const string DatePickerWatermark = "Выберите дату";

        private SerialPort serial;
        private ILog log;

        public NewPatientWindow()
        {
            log = LogManager.GetLogger("NewPatientWindow");

            InitializeComponent();
            
            string portStr = string.Empty;
            var str = ConfigurationManager.AppSettings["barcodeScannerPort"];
            if (str != null)
            {
                try
                {
                    serial = new SerialPort(str, 9600, Parity.None, 8, StopBits.One);
                    serial.DataReceived += new SerialDataReceivedEventHandler(serial_DataReceived);
                    serial.Open();
                }
                catch (Exception e)
                {
                    log.Error(string.Format("Can't open serial port. Reason: {0}", e.Message));
                }
            }
            else
            {
                log.Error(string.Format("Can't extract barcode scanner port name from app settings!"));
            }
        }

        private void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var input = serial.ReadLine();

            /// TODO: убрать из сканнера отправку символа \r

            if (input.EndsWith("\r"))
            {
                input = input.Remove(input.Length - 1);
            }

            if (input.Length == 12)
            {
                Dispatcher.Invoke(() => iinTextBox.Text = input);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            birthdateDatePicker.SetWatermarkText(DatePickerWatermark);
            iinTextBox.Focus();
        }

        private void iinTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex regex = new Regex("^[0-9]{12}$");
            string iin = (sender as TextBox).Text;

            if (!string.IsNullOrEmpty(iin) && regex.Match(iin).Success)
            {
                string year = iin.Substring(0, 2);
                string month = iin.Substring(2, 2);
                string day = iin.Substring(4, 2);

                byte chr = byte.Parse(iin.Substring(6, 1));

                Gender gender = (chr % 2 == 0) ? Gender.Female : Gender.Male;
                year = (17 + (chr + 1) / 2).ToString() + year;

                birthdateDatePicker.SelectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));

                if (gender == Gender.Male)
                {
                    maleRadioButton.IsChecked = true;
                }
                else
                {
                    femaleRadioButton.IsChecked = true;
                }

                if (iin.Equals("921210350914"))
                {
                    firstNameTextBox.Text = "Алексей";
                    middleNameTextBox.Text = "Григорьевич";
                    lastNameTextBox.Text = "Попов";
                    tpTextBox.Text = "2.0";
                    scdTextBox.Text = "3.0";
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            serial.Close();
        }

        private void startExamineBtn_Click(object sender, RoutedEventArgs e)
        {
            Patient p = new Patient();
            p.FirstName = firstNameTextBox.Text;
            p.MiddleName = middleNameTextBox.Text;
            p.LastName = lastNameTextBox.Text;
            p.IIN = iinTextBox.Text;

            // TODO: Нормальный вывод ошибок
            try
            {
                p.TP = double.Parse(tpTextBox.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте формат записи поля \"TP\" на правильность. Ошибка:\n\n" + ex.Message);
                return;
            }

            try
            {
                p.SCD = double.Parse(scdTextBox.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Проверьте формат записи поля \"SCD\" на правильность. Ошибка:\n\n" + ex.Message);
                return;
            }

            if (!maleRadioButton.IsChecked.Value && !femaleRadioButton.IsChecked.Value)
            {
                MessageBox.Show("Вы должны выбрать пол пациента!");
                return;
            }

            p.Gender = maleRadioButton.IsChecked.Value ? Gender.Male : Gender.Female;

            if (!birthdateDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Вы должны выбрать дату рождения пациента!");
                return;
            }

            p.Birthdate = birthdateDatePicker.SelectedDate.Value;

            p = PatientsRepo.Instance.Add(p);
            if (p == null)
            {
                MessageBox.Show("Не удалось сохранить пациента. Проверьте заполненные поля на наличие ошибок.");
                return;
            }

            ExaminesWindow window = new ExaminesWindow(p);
            window.Owner = Owner;
            window.Show();
            Close();
        }
    }
}
