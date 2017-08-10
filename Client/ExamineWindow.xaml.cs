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
    using Model;
    using System.IO;

    /// <summary>
    /// Interaction logic for ExamineWindow.xaml
    /// </summary>
    public partial class ExamineWindow : Window
    {
        private Examine examine;
        private Patient patient;

        public ExamineWindow(Patient patient, Examine examine)
        {
            InitializeComponent();

            this.examine = examine;
            this.patient = patient;
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            nameLabel.Text = string.Format("{0} {1} {2}", patient.LastName, patient.FirstName, patient.MiddleName);
            dateLabel.Text = examine.CreatedAt.ToString();
            iqrLabel.Text = examine.ElastoExam.IQR.ToString();
            medLabel.Text = examine.ElastoExam.Med.ToString();

            UpdateSensorLabel(sensorLabel, examine.ElastoExam.SensorType);
            UpdateIqrMedLabel(iqrMedLabel, examine.ElastoExam.IQR, examine.ElastoExam.Med);

            scansLabel.Text = examine.ElastoExam.Measures.Count().ToString();
            correctScansLabel.Text = examine.ElastoExam.Measures.Where(x => x.IsCorrect).Count().ToString();

            measuresPanel.Children.Clear();

            foreach (var measure in examine.ElastoExam.Measures)
            {
                var preview = new MeasurePreview();
                preview.measure = measure;

                preview.image.Source = ImageFromBase64(measure.Source);

                UpdateMeasurePreviewStatus(preview, measure);

                preview.TouchDown += Preview_TouchDown;
                preview.MouseLeftButtonUp += Preview_MouseLeftButtonUp;

                measuresPanel.Children.Add(preview);
            }
        }

        // Touch event
        private void Preview_TouchDown(object sender, TouchEventArgs e)
        {
            throw new NotImplementedException();
        }

        // Mouse event
        private void Preview_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var preview = sender as MeasurePreview;

            ViewImageWindow window = new ViewImageWindow("Просмотр сканирования", ImageFromBase64(preview.measure.ResultMerged), 512, 384);
            window.Owner = this;
            window.ShowDialog();
        }

        private BitmapSource ImageFromBase64(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            MemoryStream m = new MemoryStream(bytes);

            var decoder = BitmapDecoder.Create(m, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            return decoder.Frames[0];
        }

        private void UpdateSensorLabel(TextBlock label, SensorType sensorType)
        {
            label.Text = sensorType.ToString();
            label.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void UpdateIqrMedLabel(TextBlock label, double iqr, double med)
        {
            if (med != 0 && iqr != 0)
            {
                var iqrMed = examine.ElastoExam.IQRMed;

                if (!iqrMed.HasValue)
                {
                    label.Text += "Нет данных";
                    label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF2323"));
                }
                else
                {
                    label.Text = iqrMed.ToString() + "%";

                    if (iqrMed < 30)
                    {
                        label.Text += " (некорректно)";
                        label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF2323"));
                    }
                    else
                    {
                        label.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }

        private void UpdateMeasurePreviewStatus(MeasurePreview measurePreview, Measure measure)
        {
            var statuses = new List<VerificationStatus> { measure.ValidationElasto, measure.ValidationModeM, measure.ValidationModeA };
            var status = statuses.Min();

            string statusText;
            Brush background;
            switch (status)
            {
                case (VerificationStatus.Correct):
                    statusText = "Корректно";
                    background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB3FFC9"));
                    break;
                case (VerificationStatus.Incorrect):
                    statusText = "Некорректно";
                    background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFB3B3"));
                    break;
                case (VerificationStatus.Uncertain):
                    statusText = "Неоднозначно";
                    background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFF4B3"));
                    break;
                case (VerificationStatus.NotCalculated):
                default:
                    statusText = "Не вычислено";
                    background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFB3B3"));
                    break;
            }

            measurePreview.status.Content = statusText;
            measurePreview.status.Background = background;
        }

        private void dispersionBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewImageWindow window = new ViewImageWindow("Просмотр дисперсии", ImageFromBase64(examine.ElastoExam.WhiskerPlot), 832, 320);
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
