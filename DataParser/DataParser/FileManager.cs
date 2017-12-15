using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataParser
{
    public class FileManager
    {
        public string FileName { get; set; }
        public string URL { get; set; }

        public List<string> CsvToList(string path)
        {
            List<string> links = new List<string>();
            links = File.ReadAllText(path).Split(',').ToList();
            return links;
        }
        public void DownloadFile(string url)
        {
            
            WebClient client = new WebClient();
            foreach(var item in CsvToList(Environment.CurrentDirectory+@""))
            client.DownloadFile(url, Environment.CurrentDirectory + @"\files\file.pdf");
        }
        
    }
}
