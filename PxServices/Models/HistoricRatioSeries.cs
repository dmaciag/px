using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PxServices.Models
{
    public class HistoricRatioSeries
    {
        public IList<HistoricRatioPoint> RatioPoints { get; set; }

        public HistoricRatioSeries()
        {
            RatioPoints = new List<HistoricRatioPoint>();
        }
    }
}