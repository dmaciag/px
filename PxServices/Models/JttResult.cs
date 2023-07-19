using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PxServices.Models
{
    public class JttResult
    {
        public IList<Point> RatioPoints { get; set; }
        public IList<Point> UpperBoundPoints { get; set; }
        public IList<Point> LowerBoundPoints { get; set; }
        public IList<Point> JttPoints { get; set; }
        public IList<Point> TopTickerPoints { get; set; }
        public IList<Point> LowerTickerPoints { get; set; }
        public int EndingAmount { get; set; }
        public decimal YearlyGainTop { get; set; }
        public decimal YearlyGainJtt { get; set; }
        public decimal YearlyGainBottom { get; set; }
        public decimal JttTargetAllocTop { get; set; }
        public decimal JttTargetAllocBottom { get; set; }
        public decimal Log2DistanceUpward { get; set; }
        public decimal Log2DistanceDownard { get; set; }
    }
}