using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FileShufflerv2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //add Axis theme
            ThemeManager.AddAccent("Axis Theme", new Uri("pack://application:,,,/FileShufflerv2;component/Resources/AxisTheme.xaml"));
            

            base.OnStartup(e);
        }
    }
}
