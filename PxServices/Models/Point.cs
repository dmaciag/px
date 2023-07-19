using System;

namespace PxServices.Models
{
    public class Point
    {
        public decimal ClosingPrice { get; set; }
        public DateTime Date { get; set; }

        public Point()
        {

        }

        public Point(decimal closingPrice, DateTime date)
        {
            ClosingPrice = closingPrice;
            Date = date;
        }
    }
}