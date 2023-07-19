using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net.Repository.Hierarchy;
using PxServices.Services;

namespace PxServices.Models
{
    public class Bound {
        public decimal M1;
        public decimal M2;
        public decimal B1;
        public decimal B2;

        public DateTime TopBoundOneDateTime;
        public DateTime TopBoundTwoDateTime;
        public DateTime BottomBoundOneDateTime;
        public DateTime BottomBoundTwoDateTime;

        //public bool IsValid => TopBoundOneDateTime == default(DateTime) || TopBoundTwoDateTime == default(DateTime) ||
        //                       BottomBoundOneDateTime == default(DateTime) || BottomBoundTwoDateTime == default(DateTime);
        
        public Bound(decimal m1, decimal m2, decimal b1, decimal b2)
        {
            M1 = m1;
            M2 = m2;
            B1 = b1;
            B2 = b2;
        }

        public Bound(string jttBoundTopOneDt, string jttBoundTopTwoDt, string jttBoundBottomOneDt, string jttBoundBottomTwoDt)
        {
            DateTime.TryParse(jttBoundTopOneDt, out TopBoundOneDateTime);
            DateTime.TryParse(jttBoundTopTwoDt, out TopBoundTwoDateTime);
            DateTime.TryParse(jttBoundBottomOneDt, out BottomBoundOneDateTime);
            DateTime.TryParse(jttBoundBottomTwoDt, out BottomBoundTwoDateTime);
        }
    }
}