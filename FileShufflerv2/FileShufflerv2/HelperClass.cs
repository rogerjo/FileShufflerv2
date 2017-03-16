using MahApps.Metro.Controls.Dialogs;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileShufflerv2
{
    public class HelperClass
    {
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





    }
}