using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            CultureInfo CultureInfo = new CultureInfo("ru-RU");

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(
                    CultureInfo.IetfLanguageTag)));
            base.OnStartup(e);
        }
    }
}
