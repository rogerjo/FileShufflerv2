using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Media;
using MahApps.Metro;
using FileShufflerv2.Properties;

namespace FileShufflerv2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static readonly DependencyProperty ColorsProperty
= DependencyProperty.Register("Colors",
                          typeof(List<KeyValuePair<string, Color>>),
                          typeof(MainWindow),
                          new PropertyMetadata(default(List<KeyValuePair<string, Color>>)));

        public List<KeyValuePair<string, Color>> Colors
        {
            get { return (List<KeyValuePair<string, Color>>)GetValue(ColorsProperty); }
            set { SetValue(ColorsProperty, value); }
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;


            Colors = typeof(Colors)
                .GetProperties()
                .Where(prop => typeof(Color).IsAssignableFrom(prop.PropertyType))
                .Select(prop => new KeyValuePair<String, Color>(prop.Name, (Color)prop.GetValue(null)))
                .ToList();

            var theme = ThemeManager.DetectAppStyle(Application.Current);
            ThemeManager.ChangeAppStyle(this, theme.Item2, theme.Item1);

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid.ItemsSource = ViewFile._source;
            Accent currentAccent = ThemeManager.GetAccent(Settings.Default.ThemeColour);
            ThemeManager.ChangeAppStyle(Application.Current, currentAccent, ThemeManager.DetectAppStyle(Application.Current).Item1);
            //LoginScreen();

        }
        private void AccentSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedAccent = AccentSelector.SelectedItem as Accent;
            if (selectedAccent != null)
            {
                var theme = ThemeManager.DetectAppStyle(Application.Current);
                ThemeManager.ChangeAppStyle(Application.Current, selectedAccent, theme.Item1);

                Settings.Default.ThemeColour = selectedAccent.Name;
                Settings.Default.Save();

                Application.Current.MainWindow.Activate();
            }
        }

        private void Clear_button_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage grey = new BitmapImage(new Uri("Resources/download_grey.png", UriKind.Relative));
            dropimage.Source = grey;
            dropimage.Visibility = Visibility.Visible;
            dropimage.Opacity = 0.15;
            ViewFile._source.Clear();
        }

        private void DropBox_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void DropBox_DragOver(object sender, DragEventArgs e)
        {

        }

        private void DropBox_Drop(object sender, DragEventArgs e)
        {

        }

        private void Send_button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Helpbutton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
