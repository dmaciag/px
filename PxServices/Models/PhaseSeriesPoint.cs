using System;

namespace PxServices.Models
{
    public class PhaseSeriesPoint
    {
        public decimal Price { get; set; }
        public DateTime Date { get; set; }

        public PhaseSeriesPoint(decimal price, DateTime date)
        {
            Price = price;
            Date = date;
        }
    }
}