using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.IO;

namespace DataParser
{
    public static class ExcelConverter
    {
       public static void ToCSV()
        {
            File.WriteAllLines("output.csv", File.ReadLines("YH_GPRO.txt").GroupDelimited(line => line == "#")
                .Select(g => string.Join(",", g
                .Select(line => string.Join(line
                .Substring(line.IndexOf(": ") + 1)
                .Trim()
                .Replace("\"", "\"\""), "\"", "\"")))));
        }
        public static IEnumerable<IEnumerable<T>> GroupDelimited<T>(
        this IEnumerable<T> source, Func<T, bool> delimiter)
        {
            var g = new List<T>();
            foreach (var x in source)
            {
                if (delimiter(x))
                {
                    yield return g;
                    g = new List<T>();
                }
                else
                {
                    g.Add(x);
                }
            }
            yield return g;
        }
        public static void WriteArray( Worksheet worksheet,List<SecData> list)
        {
            var data = new object[list.Count+1, 16];
            data[0, 0] = "symbol";
            data[0, 1] = "sic code";
            data[0, 2] = "sic description";
            data[0, 3] = "buisness address";
            data[0, 4] = "mailing address";
            data[0, 5] = "auditor name";
            data[0, 6] = "audit fee";
            data[0, 7] = "tax fee";
            data[0, 8] = "total revenue(in thousand)";
            data[0, 9] = "net income(in thousand)";
            data[0, 10] = "Management";
            data[0, 11] = "Name";
            data[0, 12] = "Title";
            data[0, 13] = "Pay";
            data[0, 14] = "Exercise";
            data[0, 15] = "Age";

            for (var i = 1; i <= list.Count;++i)
            {
                
                data[i, 0] = list[i - 1].Symbol;
                data[i, 1] = list[i - 1].SicCode;
                data[i, 2] = list[i - 1].SicDescription;
                data[i, 3] = list[i - 1].BuisnessAddress;
                data[i, 4] = list[i - 1].MailingAddress;
                data[i, 5] = list[i - 1].AuditorName;
                data[i, 6] = list[i - 1].AuditFees;
                data[i, 7] = list[i - 1].TaxFees;
                data[i, 8] = list[i - 1].TotalRevenue;
                data[i, 9] = list[i - 1].NetIncome;
                data[i, 10] ="";
                foreach(var m in list[i-1].Managers)
                {
                    data[i, 11] +=m.Name+"\n";
                    data[i, 12] +=m.Title+"\n";
                    data[i, 13] +=m.Pay+"\n";
                    data[i, 14] +=m.Exercised+"\n";
                    data[i, 15] += m.Age.ToString()+"\n" ;
                }
              



            }
            var startCell = (Range)worksheet.Cells[1, 1];
            var endCell = (Range)worksheet.Cells[list.Count + 1, 16];
            var writeRange = worksheet.Range[startCell, endCell];

            writeRange.Value2 = data;
        }
    }
}
