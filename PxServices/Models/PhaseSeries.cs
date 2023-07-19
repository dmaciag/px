using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PxServices.Models
{
    public class PhaseSeries
    {
        public IList<PhaseSeriesPoint> SeriesShifted { get; set; }
        public IList<PhaseSeriesPoint> SeriesHistorical { get; set; }

        public PhaseSeries(IList<PhaseSeriesPoint> seriesShifted, IList<PhaseSeriesPoint> seriesHistorical)
        {
            SeriesShifted = seriesShifted;
            SeriesHistorical = seriesHistorical;
        }
    }
}