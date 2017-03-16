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
using System.Threading.Tasks;

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
        public static ObservableCollection<ViewFile> _source = new ObservableCollection<ViewFile>();
        public static ObservableCollection<ViewFile> ViewSource
        {
            get
            {
                return _source;
            }
        }

        public static List<string> SearchDirs = new List<string>();
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

        //private async void LoginScreen()
        //{
        //    try
        //    {   //Create Login dialog
        //        LoginDialogSettings ms = new LoginDialogSettings()
        //        {
        //            ColorScheme = MetroDialogColorScheme.Accented,
        //            EnablePasswordPreview = true,
        //            NegativeButtonVisibility = Visibility.Visible,
        //            NegativeButtonText = "Cancel"
        //        };
        //        LoginDialogData ldata = await this.ShowLoginAsync("Login to Galaxis", "Enter your credentials", ms);

        //        if (ldata == null)
        //        {
        //            Application.Current.Shutdown();
        //        }
        //        else
        //        {
        //            var result = await HelperClass.GalaxisLogin(ldata, SearchDirs);

        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}
        private void DropBox_DragOver(object sender, DragEventArgs e)
        {
            BitmapImage green = new BitmapImage(new Uri("Resources/download.png", UriKind.Relative));
            dropimage.Source = green;
            dropimage.Opacity = 0.40;
        }
        private void DropBox_DragLeave(object sender, DragEventArgs e)
        {
            BitmapImage grey = new BitmapImage(new Uri("Resources/download_grey.png", UriKind.Relative));
            dropimage.Source = grey;
            dropimage.Opacity = 0.15;
        }
        private async void DropBox_Drop(object sender, DragEventArgs e)
        {
            try
            {
                dropimage.Visibility = Visibility.Hidden;
                MyProgressRing.IsActive = true;
                string[] DroppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

                _source = await CalculateFileNames(DroppedFiles);
            }
            catch (IndexOutOfRangeException)
            {
                ShowMessageBox("ERROR", "One or more files are not formatted correctly.");
            }
            myDataGrid.ItemsSource = ViewSource;

            try
            {
                string[] SupplierArray = SearchDirs.ToArray();

                await AssigningSuppliers(SupplierArray);
                
            }
            catch (Exception)
            {

                ShowMessageBox("ERROR", "One or more files are not formatted correctly.");

            }


            MyProgressRing.IsActive = false;



            ShowMessageBox("ADDED", "Files have been added. Check the files and remember to press the button to send them.");
        }


        private void Clear_button_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage grey = new BitmapImage(new Uri("Resources/download_grey.png", UriKind.Relative));
            dropimage.Source = grey;
            dropimage.Visibility = Visibility.Visible;
            dropimage.Opacity = 0.15;
            _source.Clear();
        }
        private async void Send_button_Click(object sender, RoutedEventArgs e)
        {
            var result = await Task.Run(() =>
            {
                SendFilesToGalaxis();
                return "Files sent";
            });
        }

        private static void SendFilesToGalaxis()
        {
            throw new NotImplementedException();
        }

        private void Helpbutton_Click(object sender, RoutedEventArgs e)
        {
            string target = @"\\Storage03\hw-apps\ptc\fileshuffler\helpfiles\helpfile.html";

            System.Diagnostics.Process.Start(target);
        }
        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            object target = ((Button)sender).CommandParameter;
            System.Diagnostics.Process.Start("http:" + target.ToString());

        }
        public async void ShowMessageBox(string v1, string v2)
        {
            MetroDialogSettings ms = new MetroDialogSettings()
            {
                ColorScheme = MetroDialogColorScheme.Accented
            };
            await this.ShowMessageAsync(v1, v2, MessageDialogStyle.Affirmative, ms);
        }

        public async Task<ObservableCollection<ViewFile>> CalculateFileNames(string[] DroppedFiles)
        {
            try
            {
                var resultato = await Task.Run(() =>
                {
                    ObservableCollection<ViewFile> tempList = new ObservableCollection<ViewFile>();

                    foreach (string filepath in DroppedFiles)
                    {
                        //Creating FileInfo object of path
                        ViewFile item = new ViewFile();
                        FileInfo infoFile = new FileInfo(filepath);

                        string[] names = infoFile.Name.Split(new Char[] { '_', '.' });

                        item.PartNo = names[0];
                        item.FileSize = (infoFile.Length / 1024).ToString() + " kB";

                        if (infoFile.Extension == ".PDF" | infoFile.Extension == ".pdf")
                        {
                            item.Extension = ".PDF";
                        }

                        if (names.Length == 5)
                        {
                            item.AmountOfSplits = 3;
                            item.FileDescription = "Drawing";
                        }
                        else if (names.Length == 6)
                        {
                            item.AmountOfSplits = 4;
                            item.FileDescription = "Deco Spec";
                        }

                        if (names.Length != 5 && names.Length != 6)
                        {
                            item.Status = "Error";
                        }
                        else
                            switch (names[item.AmountOfSplits])
                            {
                                case "C":
                                case "c":
                                    item.Status = "Concept";
                                    break;
                                case "D":
                                case "d":
                                    item.Status = "Design";
                                    break;
                                case "P":
                                case "p":
                                    item.Status = "Pre-Released";
                                    break;
                                case "R":
                                case "r":
                                    item.Status = "Released";
                                    break;
                                default:
                                    item.Status = "Null";
                                    break;
                            }

                        //Add the newFilename property
                        //if (viewer.Extension == ".PDF" & Description == "Deco Spec")
                        //{
                        //    viewer.NewFileName = viewer.NewFileName = $"{viewer.PartNo}_deco{viewer.Extension}";
                        //}
                        if (infoFile.Extension == ".PDF" & item.FileDescription == "Deco Spec")
                        {
                            item.FileName = $"{names[0]}D_{names[1]}_{names[2]}{names[names.Length]}";
                        }
                        else if (item.Extension == ".PDF")
                        {
                            item.FileName = $"{names[0]}D_{names[1]}_{names[2]}{item.Extension}";
                        }

                        //else if (viewer.Extension == ".PDF")
                        //{
                        //    viewer.NewFileName = viewer.NewFileName = $"{viewer.PartNo}D_{names[1]}_{names[2]}{viewer.Extension}";

                        //}
                        tempList.Add(item);
                    }
                    return tempList;

                });

                return resultato;

            }
            catch (Exception)
            {
                ObservableCollection<ViewFile> errorList = new ObservableCollection<ViewFile>();
                return errorList;
            }







        }

        private async Task AssigningSuppliers(string[] supplierArray)
        {
            throw new NotImplementedException();
        }


    }
}
