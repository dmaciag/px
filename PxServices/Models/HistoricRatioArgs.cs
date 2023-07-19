using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PxServices.Models
{
    public class HistoricRatioArgs
    {
        public string TickerBenchmark { get; set; }
        public string TickerMain { get; set; }
        public bool IsMtg { get; set;} 
        public bool UseRelativeAvg { get; set; }
        public bool Scale { get; set; }
    }
}