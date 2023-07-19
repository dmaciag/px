namespace PxServices.Models
{
    public class PhaseSeriesConfig
    {
        public int Id{ get; set; }
        public string TickerShift { get; set; }
        public string TickerHistorical { get; set; }
        public string TickerInflation { get; set; }
        public DateTime StartDateShift { get; set; }
        public DateTime StartDateHistorical { get; set; }
        public decimal Offset { get; set; }
    }
}
