using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PxServices.Models
{
    public class Instrument
    {
        public string Ticker { get; }
        public decimal Concentration { get; }
        public IList<Point> DataPoints { get; }

        public Instrument(string ticker, decimal conentration, IList<Point> points)
        {
            Ticker = ticker;
            Concentration = conentration;
            DataPoints = points;
        }
    }
}