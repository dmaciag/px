using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PxServices.Models
{
    public class TickerDataRow
    {
        public string Ticker { get; set; }
        public decimal ClosingPrice { get; set; }
        public int Outlook { get; set; }
    }

    public class TickerDataResultSet
    {
        public IList<TickerDataRow> TickerDataRows { get; set; }
    }
}