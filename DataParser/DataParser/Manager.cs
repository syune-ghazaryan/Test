using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
namespace DataParser
{
    public class Manager
    {
        readonly static public int  CompanyCount = 350;
        public static void SecInfo()
        {
            List<Symbols> symbols = Enum.GetValues(typeof(Symbols))
                                    .Cast<Symbols>()
                                    .ToList();
            int count = symbols.Count;
           // int intervals = count / CompanyCount;
           
            //for (int i =0; i <intervals; ++i )
            //{
                List<SecData> list = DownloadForms();
                Application xlApp = new Application
                {
                    Visible = true
                };
                Workbook xlWb = xlApp.Workbooks.Add() as Workbook;
                Console.WriteLine(xlWb.Path);
                Worksheet xlSheet = xlWb.Sheets[1] as Worksheet;
                ExcelConverter.WriteArray(xlSheet, list);
                xlWb.SaveAs(@"C:\Users\User\source\repos\DataParser\DataParser\bin\Debug\excel\" + $"rss.xlsx");
                xlWb.Close();
                xlApp.Quit();
                Console.WriteLine("Ready!");
           
               
            //}
        }

        public static void YahooInfo()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Console.WriteLine("Yahoo: Enter SYMBOL in capitals:");
            string symbol = Console.ReadLine().ToUpper();
            List<Symbols> symbols = Enum.GetValues(typeof(Symbols))
                                    .Cast<Symbols>()
                                    .ToList();
            foreach (var item in symbols)

            {
                if (item.ToString() == symbol.ToString())
                {
                    YahooReader yr = new YahooReader();
                    string wpd = yr.GetAllData(symbol.ToString());

                    string[] profStr = wpd.ToString().Split('\n');

                    File.WriteAllLines("YH_" + symbol.ToString() + ".txt", profStr);

                    Console.WriteLine("Ready!");

                }
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Time elapsed: " + elapsedMs / 1000 + "." + elapsedMs % 1000 + "sec.");
        }
        
        public static List<SecData> DownloadForms()
        {
          

            List<SecData> list = new List<SecData>();
            List<Symbols> symbols = Enum.GetValues(typeof(Symbols))
                                  .Cast<Symbols>()
                                  .ToList();
           
            for (int i = 0; i <3; i++)
            {
                SecData Data = new SecData
                {
                    Symbol = symbols[i].ToString()
                };
                USSecReader secReader = new USSecReader(symbols[i].ToString());
                YahooReader yr = new YahooReader();
                string form1 = "10-k";

                string path = Environment.CurrentDirectory + $"\\Files\\{symbols[i]}";
               
               
                string filePath = path + "\\" + symbols[i] + "_" + form1 + ".htm";
                if (!File.Exists(filePath))
                {
                    string link10k = secReader.Get10KLink(symbols[i].ToString());
                    if (link10k != "")
                    {
                        secReader.DownloadFile(symbols[i].ToString(), link10k, form1, path,".htm");
                        Data.LinkTo10K = filePath;
                    }
                }
                else
                {
                    Data.LinkTo10K = filePath;
                }
                string form2 = "14_def_a";
                string filePath1 = path + "\\" + symbols[i] + "_" + form2 + ".htm";
               
                if (!File.Exists(filePath1))
                {
                    string linkdef14a = secReader.Get14DEFALink(symbols[i].ToString());
                    if (linkdef14a != "")
                    {
                        secReader.DownloadFile(symbols[i].ToString(), linkdef14a, form2, path,".htm");
                        Data.LinkToDef14A = filePath1;
                    }
                }
                else
                {
                    Data.LinkToDef14A = filePath1;
                }
                if (Data.LinkToDef14A != "")
                {
                    long[] fees = secReader.GetFees(Data.LinkToDef14A);
                    Data.AuditFees = fees[0];
                    Data.TaxFees = fees[1];
                }
                if(Data.LinkTo10K!="")
                {
                    Data.AuditorName = secReader.GetAuditorName(Data.LinkTo10K);
                    
                    List<YahooFinancials> list1 = yr.GetFinancial(symbols[i].ToString());
                    if(list1.Count>0)
                    {
                        Data.TotalRevenue = list1.FirstOrDefault().TotalRevenue;
                        Data.NetIncome = list1.FirstOrDefault().NetRevenue;

                    }
                    
                }
                string[] sicDesrAddress = secReader.GetSicDescrAddress(symbols[i].ToString());
                if (!String.IsNullOrEmpty(sicDesrAddress[0]))
                {
                    Data.SicCode = Int32.Parse(sicDesrAddress[0]);
                    Data.SicDescription = sicDesrAddress[1];
                    Data.BuisnessAddress = sicDesrAddress[2].Replace("Business Address", "");
                    Data.MailingAddress = sicDesrAddress[3].Replace("Mailing Address", "");
                  
                }

                Data.Managers = yr.GetManagers(symbols[i].ToString());
                list.Add(Data);
                Console.WriteLine($"{symbols[i]} is done");
            }

            return list;
        }

        public static void DownloadRss()
        {
            List<SecData> list = new List<SecData>();
            List<Symbols> symbols = Enum.GetValues(typeof(Symbols))
                                  .Cast<Symbols>()
                                  .ToList();

            for(int i=0;i<symbols.Count;i++)
            {
                SecData Data = new SecData
                {
                    Symbol = symbols[i].ToString()
                };
                USSecReader secReader = new USSecReader(symbols[i].ToString());


                string path = Environment.CurrentDirectory + $"\\Files\\{symbols[i]}";
                string form = "rss";
                string filePath = path + "\\" + symbols[i] + "_" + form + ".rss";
                if (!File.Exists(filePath))
                {
                    string rssLink = secReader.GetRss(symbols[i].ToString());
                    secReader.DownloadFile(symbols[i].ToString(), rssLink,form, path, ".rss");
                    Console.WriteLine(symbols[i]+" " );
                }
            }
         
        }

    }
}
