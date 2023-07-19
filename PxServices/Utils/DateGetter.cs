using PxServices.Models;
using PxServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PricedX.Utils
{
    public interface IDateGetter
    {
        DateTime GetFirstDate(IList<Point> y, IList<Point> x);
        DateTime GetLastDate(IList<Point> y, IList<Point> x);
    }


    public class InnerDateGetter : IDateGetter
    {
        public DateTime GetFirstDate(IList<Point> y, IList<Point> x)
        {
            if (y.FirstOrDefault().Date < x.FirstOrDefault().Date)
                return x.FirstOrDefault().Date;
            else
                return y.FirstOrDefault().Date;
        }

        public DateTime GetLastDate(IList<Point> y, IList<Point> x)
        {
            if (y.Last().Date < x.Last().Date)
                return x.Last().Date;
            else
                return y.Last().Date;
        }
    }

    public class OuterDateGetter : IDateGetter
    {
        public DateTime GetFirstDate(IList<Point> y, IList<Point> x)
        {
            if (y.FirstOrDefault().Date < x.FirstOrDefault().Date)
                return y.FirstOrDefault().Date;
            else
                return x.FirstOrDefault().Date;
        }

        public DateTime GetLastDate(IList<Point> y, IList<Point> x)
        {
            if (y.Last().Date < x.Last().Date)
                return x.Last().Date;
            else
                return y.Last().Date;
        }
    }
}
