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
    /// <summary>
    /// Interaction logic for ViewImageWindow.xaml
    /// </summary>
    public partial class ViewImageWindow : Window
    {
        private string title;
        private BitmapSource source;
        private int width;
        private int height;


        public ViewImageWindow(string title, BitmapSource source, int width, int height)
        {
            InitializeComponent();

            this.title = title;
            this.source = source;
            this.width = width;
            this.height = height;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = title;
            image.Width = width;
            image.Height = height;
            image.Source = source;
        }
    }
}
