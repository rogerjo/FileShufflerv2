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
        public static List<string> DocuSetsList = new List<string>();

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

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Accent currentAccent = ThemeManager.GetAccent(Settings.Default.ThemeColour);
            ThemeManager.ChangeAppStyle(Application.Current, currentAccent, ThemeManager.DetectAppStyle(Application.Current).Item1);

            //Creating Loginscreen and testing
            var loginresult = await LoginScreen();

            //Create a list of suppliers from Galaxis
            SearchDirs = await CreatingSuppliersListAsync(UserName, Password);

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
                MyProgressRing.Visibility = Visibility.Visible;
                myDataGrid.Visibility = Visibility.Visible;

                string[] DroppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                _source = await CalculateFileNamesAsync(DroppedFiles);

            }
            catch (Exception)
            {
                ShowMessageBox("ERROR", "One or more files are not formatted correctly.");
            }

            myDataGrid.ItemsSource = ViewSource;

            //Assigning suppliers to items in the list
            try
            {
                //Iterating through all DocuSets on all Supplier Sites  
                
                await CreateDocuSetsListAsync(SearchDirs);
            }
            catch (Exception)
            {
                ShowMessageBox("ERROR", "One or more files are not formatted correctly.");
            }

            //Starting to assign paths to Viewfiles
            AssignPathsToItems();

            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;
            //ShowMessageBox("ADDED", "Files have been added. Check the files and remember to press the button to send them.");
        }
        private void AssignPathsToItems()
        {
            //string[] SupplierArray = DocuSetsList.ToArray();

            foreach (var item in _source)
            {
                //if (DocuSetsList.Any(str => str.Contains(item.PartNo)))
                //{
                //    ShowMessageBox("Yes", "Success");
                //}
                foreach (string PO in DocuSetsList)
                {
                    if (PO.Contains(item.PartNo))
                    {
                        item.SiteFound = true;
                        item.Supplier = "Bertil";
                        item.FolderName = PO;
                    }
                }

            }


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
                var result = await SendFilesToGalaxis();
                if (result)
                {
                    ShowMessageBox("Success", "Files uploaded correctly");
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<bool> SendFilesToGalaxis()
        {
            try
            {
                var sendresult = await Task.Run(() =>
                {
                    return true;
                });
                return true;
            }
            catch (Exception)
            {
                return false;
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
        private async Task<bool> LoginScreen()
        {
            LoginDialogSettings ms = new LoginDialogSettings()
            {
                ColorScheme = MetroDialogColorScheme.Accented,
                EnablePasswordPreview = true,
                NegativeButtonVisibility = Visibility.Visible,
                NegativeButtonText = "Cancel"
            };
            try
            {   //Create Login dialog
                LoginDialogData ldata = await this.ShowLoginAsync("Login to Galaxis", "Enter your credentials", ms);

                if (ldata == null)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    Password = ldata.SecurePassword;
                    UserName = ldata.Username;
                }

                using (ClientContext ctx = new ClientContext("http://galaxis.axis.com/suppliers/Manufacturing/"))
                {

                    // SharePoint Online Credentials    
                    ctx.Credentials = new NetworkCredential(UserName, Password, "AXISNET");

                    // Get the SharePoint web  
                    Web web = ctx.Web;
                    ctx.Load(web, website => website.Webs, website => website.Title);
                    try
                    {
                        // Execute the query to the server  
                        ctx.ExecuteQuery();
                        return true;
                    }
                    catch (Exception)
                    {
                        MetroDialogSettings ff = new MetroDialogSettings()
                        {
                            ColorScheme = MetroDialogColorScheme.Accented
                        };
                        await this.ShowMessageAsync("LOGIN INCORRECT:", "Application will close down", MessageDialogStyle.Affirmative, ff);

                        Application.Current.Shutdown();
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
        private async Task<bool> CreateDocuSetsListAsync(List<string> _searchdirs)

        {
            MetroDialogSettings ms = new MetroDialogSettings()
            {
                ColorScheme = MetroDialogColorScheme.Accented
            };
            var controller = await this.ShowProgressAsync("Please wait...", "Searching all Supplier Sites for you!", false, ms);

            controller.Maximum = _searchdirs.Count;
            controller.Minimum = 0;

            var result = await Task.Run(() =>
            {
                //List<string> ResultList = new List<string>();
                int counter = 0;
                foreach (string location in _searchdirs)
                {
                    string searchSite = $"http://galaxis.axis.com/suppliers/Manufacturing/{location}/";
                    counter++;

                    controller.SetProgress((double)counter);

                    using (ClientContext ctx = new ClientContext(searchSite))
                    {
                        var POlist = ctx.Web.Lists.GetByTitle("Part Overview Library");

                        var query = new CamlQuery()
                        {
                            //Query for all NewDocuSets in Part Overview Library
                            ViewXml = @"<View><Query><Where><Eq><FieldRef Name='ContentTypeId'/><Value Type='Text'>0x0120D520005FF5F128F273FA40A49E7863E8A599C6005CFA6F34D39D6A4586AFCE307190091E</Value></Eq></Where></Query></View>"
                        };

                        var POListItems = POlist.GetItems(query);


                        //ctx.Load(POListItems);

                        ctx.Load(POListItems, li => li.Include(i => i["FileRef"]));
                        ctx.ExecuteQuery();

                        foreach (ListItem item in POListItems)
                        {
                            string o = item["FileRef"].ToString();
                            DocuSetsList.Add(o);
                        }
                    }
                }
                return true;
            });

            await controller.CloseAsync();
            return true;
        }
        private async Task<ObservableCollection<ViewFile>> CalculateFileNamesAsync(string[] DroppedFiles)
        {
            try
            {
                var resultato = await Task.Run(() =>
                {
                    ObservableCollection<ViewFile> templist = new ObservableCollection<ViewFile>();

                    foreach (string filepath in DroppedFiles)
                    {
                        //Creating FileInfo object of path
                        ViewFile item = new ViewFile(filepath);

                        string[] names = item.FileInformation.Name.Split(new Char[] { '_', '.' });

                        item.PartNo = names[0];
                        item.FileSize = (item.FileInformation.Length / 1024).ToString() + " kB";

                        //Extension is PDF = Drawing and Version info
                        if (item.FileInformation.Extension == ".PDF" | item.FileInformation.Extension == ".pdf")
                        {
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
                            item.Version = names[1] + "." + names[2];
                            item.Extension = ".PDF";
                        }
                        //Extension is STP = 3D Data and Version info
                        else
                        {
                            item.AmountOfSplits = 3;
                            item.FileDescription = "3D Data";
                            item.Extension = ".STP";
                            item.Version = names[1] + "." + names[2];
                        }


                        //Setting Status field 
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

                        if (item.FileInformation.Extension == ".PDF" & item.FileDescription == "Deco Spec")
                        {
                            item.FileName = $"{names[0]}D_{names[1]}_{names[2]}{names[names.Length]}";
                        }
                        else if (item.Extension == ".PDF")
                        {
                            item.FileName = $"{names[0]}D_{names[1]}_{names[2]}{item.Extension}";
                        }

                        templist.Add(item);
                    }
                    return templist;

                });

                return resultato;

            }
            catch (Exception)
            {
                return new ObservableCollection<ViewFile>();
            }

        }
        private async Task<List<string>> CreatingSuppliersListAsync(string userName, SecureString password)
        {
            try
            {
                //Creating Supplier list
                var createSupplierListResult = await Task.Run(async () =>
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
                        try
                        {
                            // Execute the query to the server  
                            clientContext.ExecuteQuery();

                        }
                        catch (Exception)
                        {
                            MetroDialogSettings ms = new MetroDialogSettings()
                            {
                                ColorScheme = MetroDialogColorScheme.Accented
                            };
                            await this.ShowMessageAsync("ERROR:", "Login incorrect", MessageDialogStyle.Affirmative, ms);

                            Application.Current.Shutdown();
                        }

                        // Loop through all the webs  
                        foreach (Web subWeb in web.Webs)
                        {
                            if (subWeb.Title.Contains(" "))
                            {
                                subWeb.Title = subWeb.Title.Replace(" ", "_");
                            }
                            if (subWeb.Title.Contains("å"))
                            {
                                subWeb.Title = subWeb.Title.Replace("å", "a");
                            }
                            if (subWeb.Title.Contains("ä"))
                            {
                                subWeb.Title = subWeb.Title.Replace("ä", "a");
                            }
                            if (subWeb.Title.Contains("ö"))
                            {
                                subWeb.Title = subWeb.Title.Replace("ö", "o");
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
    }
}
