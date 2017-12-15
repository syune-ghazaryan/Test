using HtmlAgilityPack;
using mshtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DataParser
{
    class Program
    {
        static void Main(string[] args)
        {
            //Manager.YahooInfo();

            Manager.SecInfo();
            //changes are here ------
            //ExcelConverter.ToCSV();
            //string mk = GetMarketCap(Symbols.CYTR.ToString());
            //Console.WriteLine(mk);
            //FileManager fm = new FileManager();
            //fm.DownloadFile(@"https://www.aam.com/docs/default-source/annual-reports/2016-aam-annual-report.pdf?sfvrsn=93d10332_4");
            //Manager.DownloadForms();
            //Manager.MergeExcel();
        }
       
    }
}
