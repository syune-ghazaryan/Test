using HtmlAgilityPack;
using mshtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DataParser
{
    public class USSecReader
    {
        public string SiteURL
        {
            get
            {
                return "https://www.sec.gov";
            }
        }
        public string CIK { get; set; }
        public WebClient Client { get; set; }

        public string  Path10k {get;set;}
        public string Pathdef14a { get; set; }
        public SecData SecurityData { get; set; }
        public USSecReader(string symbol)
        {
            Client = new WebClient();
            CIK = GetCik(symbol);
        }
        public string Link14DefA { get; set; }
        public string Link10k { get; set; }
        private string GetCik(string symbol)
        {
            CIK = "";

            string url = $"https://www.sec.gov/cgi-bin/browse-edgar?CIK={symbol}&owner=exclude&action=getcompany&Find=Search";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(url);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//span");
            if (nodes == null)
            {
                return CIK;
            }
            foreach (var node in nodes)
            {
                if (node.Attributes.Contains("class") && node.Attributes["class"].Value == "companyName")
                {
                    CIK = Regex.Match(node.InnerText, @"\d+").Value;
                }
            }
            return CIK;
        }

        public string[] GetSicDescrAddress(string symbol)
        {
            string[] data = new string[4] { "", "", "","" };

            int sic = 0;
            string description = "";//description temp
            string[] address = new string[2] { "", "" };//address temp
            string url = $"https://www.sec.gov/cgi-bin/browse-edgar?CIK={symbol}&owner=exclude&action=getcompany&Find=Search";

            

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(url);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//p");
            int j = 0;
            if(nodes==null)
            {
                return null;
            }
            foreach (var node in nodes)
            {
                if (node.Attributes.Contains("class") && node.Attributes["class"].Value == "identInfo")
                {
                    string sicstr = "";
                    string raw = node.OuterHtml;
                    string pattern = @"(\d){4}((<\/a>) - (\S|\s)*<br>)S";
                    string matched = Regex.Match(raw, pattern).Value.Replace("</a>", "").Replace("<br>", "");
                    matched.Remove(matched.Length - 1, 1);
                    string sicCode = node.InnerText;
                    description = matched.Remove(0, 6);
                    data[1] = description;
                    for (int i = 5; i < 9; i++)
                    {
                        sicstr += sicCode[i];
                    }
                    Int32.TryParse(sicstr, out sic);
                    data[0] = sic.ToString();
                }
            }
            nodes = htmlDoc.DocumentNode.SelectNodes("//div");
            foreach (var node in nodes)
            {
                if (node.Attributes.Contains("class") && node.HasClass("mailer"))
                {
                    address[j] = node.InnerText;
                    
                    j++;
                }
            }

            data[2] = address[1].Replace("\n","").Replace("     ", " ");
            data[3] = address[0].Replace("\n","").Replace("     ", " ");
            return data;
        }
        public string Get14DEFALink(string symbol)
        {
            //searching for  def14a
            string urlDocument = "";
            if(CIK=="")
            {
                return urlDocument;
            }
            string urlForDef14a = $"https://www.sec.gov/cgi-bin/browse-edgar?action=getcompany&CIK={CIK}&type=DEF+14A&dateb=&owner=exclude&count=40";
            string link = "";
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(urlForDef14a);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//a");
            foreach (var node in nodes)
            {
                if (node.Id == "documentsbutton")
                {
                    link = node.GetAttributeValue("href", "");
                    break;
                }
            }
            if (String.IsNullOrEmpty(link))
                return urlDocument;
            //searching for .htm document
            string link2 = "";
            string url14a = String.Format(SiteURL + "{0}", link);
            htmlDoc = web.Load(url14a);
            nodes = htmlDoc.DocumentNode.SelectNodes("//a");
            foreach (var node in nodes)
            {
                string a = $@"\b({ CIK.TrimStart('0')})\b(\S)*.htm$";
                string b = node.GetAttributeValue("href", "");
                bool t = Regex.IsMatch(b, a);
                if (t)
                {
                    link2 = node.GetAttributeValue("href", "");
                }
            }
            //searching data(audit fee and tax fee) in .htm document
            if(link2=="")
                return "";
            else
              return urlDocument = SiteURL + link2;
        }

        public long[] GetFees( string urlDocument)
        {
           
            long[] fees = new long[2] { 0, 0 };
            if (String.IsNullOrEmpty(urlDocument))
            {
                return fees;
            }
         
            using (Stream data = File.OpenRead(urlDocument))
            {
                using (StreamReader reader = new StreamReader(data))
                {
                    string htmlContent3 = reader.ReadToEnd();
                    IHTMLDocument2 htmlDocument3 = (IHTMLDocument2)new HTMLDocument();

                    htmlDocument3.write(htmlContent3);
                    IHTMLElementCollection allElements3 = htmlDocument3.all;
                    foreach (IHTMLElement element in allElements3)
                    {
                        if (element.tagName == "TABLE" && !String.IsNullOrEmpty(element.outerText))
                        {
                            if (element.outerText.Contains("Audit Service Fees") || element.outerText.Contains("Audit fees") || element.outerText.Contains("Audit Fees") || element.outerText.Contains("Audit"))
                            {
                                string text = element.outerText.Replace(" ", "");
                                string pat = @"Audit(Service)*((F|f)ees)(\d)";
                                if (Regex.IsMatch(text, pat))
                                {
                                    string a = Regex.Match(text, pat).Value;
                                    string patt = @"\d";
                                    string b = Regex.Replace(Regex.Match(text, pat).Value, patt, "");
                                    text = text.Replace(Regex.Match(text, pat).Value, b);
                                }

                                string pattern = @"(Audit(Service)*(F|f)ees)(\$)*(\(\S\)*(\S))*\d{1,3}(,\d{3})*(\.\d+)?";

                                if (Regex.IsMatch(text, pattern))
                                {
                                    string str = Regex.Match(text, pattern).Value;
                                    int index = str.IndexOf("$");
                                    string a = Regex.Match(str, @"\$(\S|\s)*(\d*(\,)?\d+)*(\$)*").Value;
                                    string b = a.Replace("$", "").Trim();
                                    bool tr = Int64.TryParse(b.Replace(",", ""), out long auditFee);
                                    fees[0] = auditFee;//audit fee
                                    if (text.Contains("inthousands"))
                                        fees[0] = auditFee * 1000;//audit fee
                                }
                                else
                                {
                                    string pattern1 = @"Audit(\$)*(\(\S\)*(\S))*\d{1,3}(,\d{3})*(\.\d+)?";
                                    if (Regex.IsMatch(text, pattern1))
                                    {
                                        string str = Regex.Match(text, pattern1).Value;
                                        int index = str.IndexOf("$");
                                        string a = Regex.Match(str, @"\$(\S|\s)*(\d*(\,)?\d+)*(\$)*").Value;
                                        string b = a.Replace("$", "").Trim();
                                        bool tr = Int64.TryParse(b.Replace(",", ""), out long auditFee);
                                        fees[0] = auditFee;//audit fee
                                        if (text.Contains("inthousands"))
                                            fees[0] = auditFee * 1000;
                                    }

                                }

                            }

                            if (element.outerText.Contains("Tax Fees") || element.outerText.Contains("Tax fees") || element.outerText.Contains("Tax"))
                            {
                                string pattern = @"((Tax)(F|f)ees)(\$)*(\(\S\)*(\S))*\d{1,3}(,\d{3})*(\.\d+)?";
                                string text = element.outerText.Replace(" ", "");

                                if (Regex.IsMatch(text, pattern))
                                {
                                    text = Regex.Match(text, pattern).Value;
                                    string deleted = Regex.Match(text, @"\(\d\)").Value;
                                    string str = text;
                                    if (!String.IsNullOrEmpty(deleted))
                                        str = Regex.Match(text, pattern).Value.Replace(deleted, "");
                                    string a = Regex.Match(str, @"(([A-Za-z]+)|\D)*").Value;
                                    string digit = str.Replace(a, "");
                                    string b = digit.Replace("$", "").Trim();
                                    bool tr = Int64.TryParse(b.Replace(",", ""), out long taxFee);
                                    fees[1] = taxFee;//audit fee
                                    break;
                                }

                                else
                                {
                                    string pattern1 = @"(Tax)(\S)*(\$)*(\(\S\)*(\S))*\d{1,3}(,\d{3})*(\.\d+)?";
                                    if (Regex.IsMatch(text, pattern1))
                                    {
                                        string str = Regex.Match(text, pattern1).Value;
                                        int index = str.IndexOf("$");
                                        string a = Regex.Match(str, @"(\S|\s)*(\d*(\,)?\d+)*(\$)*").Value;
                                        string b = a.Replace("$", "").Trim();
                                        bool tr = Int64.TryParse(b.Replace(",", ""), out long taxFee);
                                        fees[1] = taxFee;//audit fee
                                        if (text.Contains("inthousands"))
                                            fees[1] = taxFee * 1000;

                                    }
                                }
                            }
                        }
                    }
                }
            }
            return fees;
        }

        public string Get10KLink(string symbol)
        {

            string Data10k = "";
            //searching for 10-k
            if(CIK=="")
            {
                return Data10k;
            }
            string urlFor10k = $"https://www.sec.gov/cgi-bin/browse-edgar?action=getcompany&CIK={CIK}&type=10-k&dateb=&owner=exclude&count=40";
            string link10k = "";

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(urlFor10k);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//a");
            foreach (var node in nodes)
            {
                if (node.Attributes.Contains("Id") && node.Id == "documentsbutton")
                {
                    link10k = node.GetAttributeValue("href", "");
                    break;
                }
            }

            //search for .htm document for 10-k
            string linkToDoc = "";
            string document10k = SiteURL + link10k;
            htmlDoc = web.Load(document10k);
            nodes = htmlDoc.DocumentNode.SelectNodes("//a");
            foreach (var node in nodes)
            {

                string a = String.Format(@"\b({0})\b(\S)*.htm$", CIK.TrimStart('0'));
                string b = node.GetAttributeValue("href", "");
                bool t = Regex.IsMatch(b, a);
                if (t)
                {
                    linkToDoc = node.GetAttributeValue("href", "");
                    break;
                }
            }
            string finalLink = SiteURL + linkToDoc;//link to download
            Link10k = finalLink;
            return Link10k;
        }

        public string GetAuditorName( string filePath)
        {

            string auditorName = "";
            if (String.IsNullOrEmpty(filePath))
                return auditorName;
            using (Stream data = File.OpenRead(filePath))
            {
                using (StreamReader reader = new StreamReader(data))
                {
                    string htmlContent = reader.ReadToEnd();
                    IHTMLDocument2 htmlDocument = (IHTMLDocument2)new HTMLDocument();

                    htmlDocument.write(htmlContent);
                    IHTMLElementCollection allElements = htmlDocument.all;

                    foreach (IHTMLElement element in allElements)
                    {
                        if (!String.IsNullOrEmpty(element.innerText) && element.innerText.Contains("/s/"))
                        {
                            Regex regex = new Regex(@"(\/s\/) (\S|\s)* \bLL(P|C)\b");
                            bool a = regex.IsMatch(element.innerText);
                            if (a)
                            {
                                auditorName = regex.Match(element.innerText).Value;
                                int positionOfNewLine = auditorName.IndexOf("\r\n");

                                if (positionOfNewLine >= 0)
                                {
                                    auditorName = auditorName.Substring(0, positionOfNewLine).Replace("/s/", "").TrimStart();
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return auditorName;
        }
         public decimal[] GetIncomes(string filePath)
         {

            decimal[] incomes = new decimal[2] { 0, 0 };
            

            var doc = new HtmlDocument();
            doc.Load(filePath);

            var nodes = doc.DocumentNode.SelectNodes("//table");
            foreach(var node in nodes)
            {
                if (node.InnerText.Contains("Total revenue") && (  node.InnerText.Contains((DateTime.Now.Year-1).ToString()) || node.InnerText.Contains(DateTime.Now.Year.ToString())))
                {
                    var trs = node.SelectNodes("tr");
                    foreach (var tr in trs)
                    {
                        string pattern = @"(Total)(\s)*(R|r)evenue(s)*";
                        if (Regex.IsMatch(tr.InnerText, pattern))
                        {
                            string pattern1 = @"\d{1,3}(,\d{3})*(\.\d+)?";
                            string row = tr.InnerText;
                            string matched= Regex.Match(row,pattern1).Value;
                            if(decimal.TryParse(matched,out decimal result))
                            {
                                incomes[0] = result;
                                if (result<1000000)
                                    incomes[0] = result*1000;
                            }
                        }
                       
                    }
                }
                if((node.InnerText.Contains("Net loss") || node.InnerText.Contains("Net income")) && node.InnerText.Contains("2016"))
                {
                    var trs = node.SelectNodes("tr");
                    string pattern2 = @"(Net)(\s)*((L|l)oss|(I|i)ncome)(s)*";
                    foreach (var tr in trs)
                    {
                        if (Regex.IsMatch(tr.InnerText, pattern2))
                        {
                            string pattern1 = @"\d{1,3}(,\d{3})*(\.\d+)?";
                            string row = tr.InnerText;
                            string matched = Regex.Match(row, pattern1).Value;
                            if (decimal.TryParse(matched, out decimal result))
                            {
                                incomes[1] = result;
                                if (result<1000000)
                                    incomes[1] = result * 1000;
                            }
                        }
                    }
                }
            }
            return incomes;
        }

        public  void DownloadFile(string symbol, string link, string form,string path)
        {
            // Try to create the directory.
            Directory.CreateDirectory(path);
            string fileName = path + "/" + symbol + "_" + form + ".htm";
            if (link != "")
            {
                Client.DownloadFile(link, fileName);
                if(form== "10-k")
                {
                    Path10k = fileName;
                }
                else
                {
                    Pathdef14a = fileName;
                }

            }
        }
    }
}
