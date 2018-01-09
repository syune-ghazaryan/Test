using HtmlAgilityPack;
using mshtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataParser
{
    public class YahooReader 
    {
        public string SiteURL { get; set; }
        public string ProfileURL { get; set; }
        public string FinancialURL { get; set; }
        public WebClient Client { get; set; }
        public YahooProfile Profile { get; set; }
        public YahooFinancials Financials { get; set; }

        public YahooReader()
        {
            SiteURL = "https://finance.yahoo.com/";
            Client = new WebClient();
        }

        private YahooProfile GetProfile(string symbol)
        {
            string[] dat = new string[6] { "","","","","",""};

            string marketCapURL = $"https://finance.yahoo.com/quote/{symbol}?p={symbol}";
            
            ProfileURL= $"https://finance.yahoo.com/quote/{symbol}/profile?p={symbol}";

            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(marketCapURL);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//td");
            foreach (var node in nodes)
            {
                if (node.Attributes.Contains("data-test"))
                {
                    if(node.Attributes["data-test"].Value == "MARKET_CAP-value")
                    {
                       dat[5]= node.InnerText;
                    }
                }
            }
            htmlDoc = web.Load(ProfileURL);

            nodes = htmlDoc.DocumentNode.SelectNodes("//strong");
            int i = 0;
            foreach(var node in nodes)
            {
                if(!String.IsNullOrEmpty(node.InnerText) && !node.InnerText.Any(c => char.IsDigit(c)))
                {
                    dat[i] = node.InnerText;//sector & industry
                    i++;
                }
            }
            nodes = htmlDoc.DocumentNode.SelectNodes("//h3");
            foreach(var node in nodes)
            {
                if(node.Attributes.Contains("class") && node.Attributes["class"].Value.Contains("Mb(10px)"))
                {
                    dat[2] = node.InnerText;//full name
                }

            }
            nodes = htmlDoc.DocumentNode.SelectNodes("//p");
            foreach (var node in nodes)
            {

                if (node.Attributes.Contains("class"))
                {
                    string className = node.Attributes["class"].Value;
                    switch (className)
                    {
                        case "D(ib) W(47.727%) Pend(40px)":
                            dat[3] = node.InnerText;
                            break;
                        case "Mt(15px) Lh(1.6)":
                            dat[4] = node.InnerText;
                            break;
                    }
                }
               
            }
          
            YahooProfile Profile = new YahooProfile
            {
                Sector = dat[0],
                Industry = dat[1],
                FullName = dat[2],
                FullAddress = dat[3].Replace("\r\n", " "),
                Description = dat[4],
                MarketCap = dat[5]
                
            };
            return Profile;


        }
        /// <summary>
        /// /Get financials  by symbol name
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>financials for 3 years</returns>
        public List<YahooFinancials> GetFinancial(string symbol)
        {
            string url = String.Format("https://finance.yahoo.com/quote/{0}/financials?p={0}", symbol);
            //
            string url1 = String.Format("https://finance.yahoo.com/quote/{0}/balance-sheet?p={0}", symbol);
            List<int> fiscalYears = new List<int>();
            List<decimal> totalRevenue = new List<decimal>();
            List<decimal> netRevenue = new List<decimal>();
            List<decimal> totalAssets = new List<decimal>();
            List<YahooFinancials> list = new List<YahooFinancials>();
            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(url);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//td");
            foreach(var node in nodes)
            {
               
                if (node.Attributes.Contains("class"))
                {
                    string className = node.Attributes["class"].Value;

                    switch (className)
                    {
                        case "C($gray) Ta(end)":
                            DateTime dt = DateTime.Now;
                            DateTime.TryParse(node.InnerText, out dt);
                            fiscalYears.Add(dt.Year);
                            break;
                        case "Fz(s) Ta(end) Pstart(10px)":
                            Decimal.TryParse(node.InnerText.Replace(",",""), out decimal totalRevenues);
                            var spans = htmlDoc.DocumentNode.SelectNodes("//span").Where(x => x.HasClass("Fz(xs) C($gray) Mstart(25px) smartphone_Mstart(0px) smartphone_D(b) smartphone_Mt(5px)"));
                            if(spans.Count()>0 &&  spans.FirstOrDefault().InnerText.Contains("thousands"))
                            {
                                totalRevenues *= 1000;
                            }
                            totalRevenue.Add(totalRevenues);
                            break;
                        case "Fw(b) Ta(end) Py(8px) Pt(36px)":
                            Decimal.TryParse(node.InnerText.Replace(",",""), out decimal netIncomes);
                            var spans1 = htmlDoc.DocumentNode.SelectNodes("//span").Where(x => x.HasClass("Fz(xs) C($gray) Mstart(25px) smartphone_Mstart(0px) smartphone_D(b) smartphone_Mt(5px)"));
                            if (spans1.Count() > 0 && spans1.FirstOrDefault().InnerText.Contains("thousands"))
                            {
                                netIncomes *= 1000;
                            }
                            netRevenue.Add(netIncomes);
                            break;

                    }

                    
                }
               
            }

            htmlDoc = web.Load(url1);
            nodes = htmlDoc.DocumentNode.SelectNodes("//td");
            foreach(var node in nodes)
            {
                if(node.Attributes.Contains("class") && node.Attributes["class"].Value == "Fw(b) Fz(s) Ta(end) Pb(20px)")
                {
                    Decimal.TryParse(node.InnerText, out decimal totAs);
                    totalAssets.Add(totAs);
                }
            }
        
            if(fiscalYears.Count<=0)
            {
                return list;
            }
            for (int i = 0; i < fiscalYears.Count; i++)
            {
                YahooFinancials fin = new YahooFinancials
                {
                    FiscalYear = fiscalYears[i],
                    NetRevenue = netRevenue[i],
                    TotalAssets = totalAssets[i],
                    TotalRevenue = totalRevenue[i]
                };

                list.Add(fin);
            }
            return list;
        }

        public string GetAllData(string symbol)
        {
            YahooProfile profile = GetProfile(symbol);
            List<YahooFinancials> financials = GetFinancial(symbol);
            YahooData data = new YahooData
            {
                Profile = profile,
                Financials = financials
            };
            return data.ToString();
        }

        public List<YahooManager> GetManagers(string symbol)
        {
            string url = $@"https://finance.yahoo.com/quote/{symbol}/profile?p={symbol}";
            List<YahooManager> list = new List<YahooManager>();
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(url);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//tbody");
               // .Where(x=>x.HasClass("Bxz(bb) quote-subsection")).First();
            foreach(var s in nodes)
            {
                if (s.Attributes["data-reactid"].Value=="49")
                {
                    var trs = s.SelectNodes("//tr");
                    int y = 0;
                    for (int  j=2;j<trs.Count;j++)
                    {
                       
                        var tds = trs[j].SelectNodes("//td");
                        YahooManager mng = new YahooManager();
                        mng.Name = tds[2+y].InnerText;
                        mng.Title = tds[3+y].InnerText;
                        mng.Pay = tds[4+y].InnerText;
                        mng.Exercised = tds[5+y].InnerText;
                        Int32.TryParse(tds[6+y].InnerText, out int a);

                        mng.Age = a;
                        list.Add(mng);
                        y += 5;
                    }
                }
                break;
            }
            


            return list;
        }

    }
}
