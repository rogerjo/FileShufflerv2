using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileShufflerv2
{
    public class ViewFile
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
        public int AmountOfSplits { get; set; }
        public FileInfo FileInformation { get; set; }

        public ViewFile()
        {

        }

        public ViewFile(string Filepath)
        {
            FileInformation = new FileInfo(Filepath);
        }
    }
}
