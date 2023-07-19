using System;

namespace PxServices.Models
{
    public class PhaseSeriesArgs
    {
        public string TickerShift { get; set; }
        public string TickerHistorical { get; set; }
        public string TickerInflation { get; set; }

        public DateTime StartDateShift { get; set; } 
        public DateTime StartDateHistorical { get; set; }
        public decimal OffSet { get; set; }
    }
}