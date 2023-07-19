using System;

namespace PxServices.Models
{
    public class HistoricRatioPoint
    {
        public decimal Ratio { get; set; }
        public DateTime Date { get; set; }

        public HistoricRatioPoint(decimal ratio, DateTime date)
        {
            Ratio = ratio;
            Date = date;
        }
    }
}