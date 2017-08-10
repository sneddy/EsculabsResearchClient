using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// <summary>
    /// Interaction logic for LoaderWindow.xaml
    /// </summary>
    public partial class LoaderWindow : Window
    {
        private int _dotCount = 0;

        public LoaderWindow()
        {
            InitializeComponent();
        }

        private void TimerCallback(object state)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                _dotCount++;

                if (_dotCount > 3)
                {
                    _dotCount = 0;
                }

                var text = "Пожалуйста, подождите";
                for (var i = 0; i < _dotCount; i++)
                {
                    text += ".";
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    label.Content = text;
                }));
            });

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Timer(TimerCallback, null, 0, 1000);
        }
    }
}
