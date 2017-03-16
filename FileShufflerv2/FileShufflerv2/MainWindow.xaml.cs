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
using Microsoft.SharePoint.Client;
using System.Net;
using System.Security;

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

        public string UserName { get; set; }
        public SecureString Password { get; set; }

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
            LoginScreen();

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
            //Making fileparameters available in ViewFile
            try
            {
                dropimage.Visibility = Visibility.Hidden;
                MyProgressRing.IsActive = true;
                string[] DroppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

                _source = await CalculateFileNames(DroppedFiles);
            }
            catch (Exception)
            {
                ShowMessageBox("ERROR", "One or more files are not formatted correctly.");
            }

            myDataGrid.ItemsSource = ViewSource;

            //Assigning suppliers to items in the list
            //try
            //{
            //    var assignedsuppliers = await CreatingSuppliersList(UserName, Password);
            //}
            //catch (Exception)
            //{

            //    ShowMessageBox("ERROR", "One or more files are not formatted correctly.");

            //}
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
            try
            {
                //await SendFilesToGalaxis();
            }
            catch (Exception)
            {

                throw;
            }
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

        private async void LoginScreen()
        {
            try
            {   //Create Login dialog
                LoginDialogSettings ms = new LoginDialogSettings()
                {
                    ColorScheme = MetroDialogColorScheme.Accented,
                    EnablePasswordPreview = true,
                    NegativeButtonVisibility = Visibility.Visible,
                    NegativeButtonText = "Cancel"
                };
                LoginDialogData ldata = await this.ShowLoginAsync("Login to Galaxis", "Enter your credentials", ms);

                if (ldata == null)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    //var result = await GalaxisLogin(ldata, SearchDirs);
                    Password = ldata.SecurePassword;
                    UserName = ldata.Username;

                }

                var assignedsuppliers = await CreatingSuppliersList(UserName, Password);


            }
            catch (Exception)
            {

                throw;
            }

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

        private async Task<List<string>> CreatingSuppliersList(string userName, SecureString password)
        {
            try
            {
                //Creating Supplier list
                var createSupplierListResult = await Task.Run(() =>
                {
                    List<string> Supplierlist = new List<string>();
                    // ClientContext - Get the context for the SharePoint Online Site               
                    using (ClientContext clientContext = new ClientContext("http://galaxis.axis.com/suppliers/Manufacturing/"))
                    {
                        // SharePoint Online Credentials    
                        clientContext.Credentials = new NetworkCredential(userName, password, "AXISNET");

                        // Get the SharePoint web  
                        Web web = clientContext.Web;
                        clientContext.Load(web, website => website.Webs, website => website.Title);

                        // Execute the query to the server  
                        clientContext.ExecuteQuery();

                        // Loop through all the webs  
                        foreach (Web subWeb in web.Webs)
                        {
                            if (subWeb.Title.Contains(" "))
                            {
                                subWeb.Title = subWeb.Title.Replace(" ", "_");
                            }
                            Supplierlist.Add(subWeb.Title.ToString());

                        }

                        Supplierlist.Remove(@"Manufacturing_Template_Site_0");
                        Supplierlist.Remove(@"manufacturing_template1");
                        Supplierlist.Remove(@"Junda_2");
                        Supplierlist.Remove(@"Goodway_2");
                        Supplierlist.Remove(@"Experimental2");

                    }
                    return Supplierlist;

                });

                return createSupplierListResult;
            }
            catch (Exception)
            {
                List<string> errorlist = new List<string>
                {
                    "Error"
                };
                return errorlist;
            }

        }

        //private static async Task AssigningSuppliers(string[] SupplierList)
        //{
        //    //Enumerating selected files towards supplierlist
        //    var result = await Task.Run(() =>
        //    {
        //        string[] SupplierArray = SearchDirs.ToArray();


        //    });
        //}

        public async Task<string> GalaxisLogin(LoginDialogData ldata, List<string> search)
        {
            var result = await Task.Run(() =>
            {
                using (ClientContext context = new ClientContext("http://galaxis.axis.com/"))
                {
                    context.Credentials = new NetworkCredential(ldata.Username, ldata.Password, "AXISNET");
                    string LoginStatus = "";
                    try
                    {
                        ClientContext clientContext = new ClientContext("http://galaxis.axis.com/suppliers/Manufacturing/");
                        Web oWebsite = clientContext.Web;
                        clientContext.Load(oWebsite, website => website.Webs, website => website.Title);
                        clientContext.ExecuteQuery();
                        foreach (Web orWebsite in oWebsite.Webs)
                        {
                            if (orWebsite.Title.Contains(" "))
                            {
                                orWebsite.Title = orWebsite.Title.Replace(" ", "_");
                            }

                            search.Add(orWebsite.Title);
                        }

                        //Remove directories that are not suppliers
                        search.Remove(@"Manufacturing_Template_Site_0");
                        search.Remove(@"manufacturing_template1");
                        search.Remove(@"Junda 2");
                        search.Remove(@"Goodway 2");
                        search.Remove(@"Experimental2");


                        for (int i = 0; i < search.Count; i++)
                        {
                            search[i] = @"\\galaxis.axis.com\suppliers\Manufacturing\" + search[i];
                        }

                        LoginStatus = "Login successful!";
                        return LoginStatus;
                    }
                    catch (Exception e)
                    {
                        LoginStatus = "Login failed";
                        return LoginStatus;
                    }


                }

            });
            return result;


        }
        //public async Task<string> SendFilesToGalaxis()
        //{
        //    var result = await Task.Run(() =>
        //    {



        //    });

        //    return result;
        //}




    }
}
