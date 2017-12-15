using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParser
{
    public class SecData
    {
        public string Symbol { get; set; }
        public int SicCode { get; set; }
        public string SicDescription { get; set; }
        public string BuisnessAddress { get; set; }
        public string MailingAddress { get; set; }
        public long AuditFees { get; set; }
        public long TaxFees { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal NetIncome { get; set; }
        public string AuditorName { get; set; }
        public string LinkTo10K { get; set; }
        public string LinkToDef14A { get; set; }

        public List<YahooManager> Managers { get; set; }

        public SecData()
        {
            SicCode = 0;
            SicDescription = "";
            BuisnessAddress = "";
            AuditFees = 0;
            TaxFees = 0;
            TotalRevenue = 0;
            NetIncome = 0;
            AuditorName = "";
        }
        public override string ToString()
        {
            return $"\nSic code:{this.SicCode};\nBusiness Address:{this.BuisnessAddress};\nMailing Address:{this.MailingAddress};\nSic Description:{this.SicDescription};\nAudit Fees:{this.AuditFees};\nTax Fees:{this.TaxFees};\nAuditor Name:{this.AuditorName};\n";

        }
    }
}
