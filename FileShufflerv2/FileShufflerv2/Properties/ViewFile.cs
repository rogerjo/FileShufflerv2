﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileShufflerv2.Properties
{
    class ViewFile
    {
        public string Extension { get; set; }
        public string SourceLocation { get; set; }
        public string FileSize { get; set; }
        public string PartNo { get; set; }
        public string CopySite { get; set; }
        public string FileName { get; set; }
        public bool SiteFound { get; set; }
        public string Supplier { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
        public string NewFileName { get; set; }
        public string FolderName { get; set; }
        public string FileDescription { get; set; }




        public ViewFile()
        {

        }

        public static ObservableCollection<ViewFile> _source = new ObservableCollection<ViewFile>();
        public static ObservableCollection<ViewFile> ViewSource
        {
            get
            {
                return _source;
            }
        }
    }
}