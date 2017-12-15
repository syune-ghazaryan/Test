using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParser
{
    public class YahooProfile
    {
        public string Sector { get; set; }
        public string Industry { get; set; }
        public string FullName { get; set; }
        public string FullAddress { get; set; }
        public string Description { get; set; }

        public string MarketCap { get; set; }

        List<YahooManager> Managers { get; set; }
        public override string ToString()
        {
            string[] words = this.Description.Split(' ');
            StringBuilder sb = new StringBuilder();
            int currLength = 0;
            foreach (string word in words)
            {
                if (currLength + word.Length + 1 < 120) // +1 accounts for adding a space
                {
                    sb.AppendFormat(" {0}", word);
                    currLength = (sb.Length % 120);
                }
                else
                {
                    sb.AppendFormat("{0}{1}", Environment.NewLine, word);
                    currLength = 0;
                }
            }

            return $"Full name:{this.FullName};\nFull Address:{this.FullAddress};\nSector:{this.Sector};\nIndustry:{this.Industry};\nMarket Cap: {this.MarketCap.ToString()};\n\nDescription:\n{sb.Replace("\n","")};";
        }
    }

    public class YahooFinancials
    {
        public decimal TotalRevenue { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal TotalAssets { get; set; }
        public int FiscalYear { get; set; }

        public  YahooFinancials()
        {
            TotalRevenue =0;
            TotalAssets = 0;
            FiscalYear = 0;
            NetRevenue = 0;
        }

        public override string ToString()
        {
            return $"\nFiscal Year:{this.FiscalYear};\nTotal Revenue:{this.TotalRevenue};\nNet Revenue:{this.NetRevenue};\nTotal Assets:{this.TotalAssets};\n";
        }
    }

    
    public class YahooData 
    {
        public YahooProfile Profile { get; set; }
        public List<YahooFinancials> Financials { get; set; }
       
      
        public override string ToString()
        {
           string yfl = "";
           foreach (YahooFinancials financial in Financials)
           {
               yfl += financial.ToString();
           }
           return "\n"+this.Profile.ToString() + "\n" + yfl+"\n";
        }
    }
    
    public class YahooManager
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Pay { get; set; }
        public int Age { get; set; }
        public string Exercised { get; set; }
        public YahooManager()
        {
            Name = "";
            Title = "";
            Pay = "";
            Age = 0;
            Exercised = "";
        }

    }
}
