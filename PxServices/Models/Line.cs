using System;
using PxServices.Services;

namespace PxServices.Models
{
    public class CoordinateSet
    {
        public decimal y2 { get; set; }
        public decimal y1 { get; set; }
        public decimal x2 { get; set; }
        public decimal x1 { get; set; }
    }

    public class Line
    {
        public decimal M;
        public decimal B;

        public Line(CoordinateSet cs)
        {            
            M = (cs.y2 - cs.y1) /(cs.x2 - cs.x1);
            B = (cs.y2 - M * cs.x2);
        }

        public override string ToString() => $"M:{Decimal.Round(M,2)}, B:{Decimal.Round(B,2)}";
    }
}