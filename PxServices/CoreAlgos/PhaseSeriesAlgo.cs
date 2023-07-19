using PricedX.Models;
using PricedX.Utils;
using PxServices.Interfaces;
using PxServices.Models;

namespace PxServices.CoreAlgos
{
    public class PhaseSeriesAlgo : IPhaseSeriesAlgo
    {
        public IDataRetrievalService _dataRetrievalService { get; set; }
        public IHistoricRatioAlgo _historicRatioAlgo { get; set; }
        public IEngine _engine { get; set; }
        public IDateGetter _dateGetter { get; set; }
        public int _roundCount { get; set; }


        public PhaseSeriesAlgo(IDataRetrievalService dataRetrievalService, IEngine engine, IHistoricRatioAlgo historicRatioAlgo)
        {
            _historicRatioAlgo = historicRatioAlgo;
            _dataRetrievalService = dataRetrievalService;
            _engine = engine;
            _dateGetter = new OuterDateGetter();
            _roundCount = 10;
        }

        private IList<Point>? Deflate(string mainTicker, string inflationTicker)
        {
            var deInflatedPoints = _historicRatioAlgo.GetHistoricRatioSeries(new HistoricRatioArgs()
            {
                TickerMain = mainTicker,
                TickerBenchmark = inflationTicker,
                IsMtg = false,
                UseRelativeAvg = false,
                Scale = false
            })?.RatioPoints.Select(o => new Point { ClosingPrice = o.Ratio, Date = o.Date }).ToList();

            return deInflatedPoints;
        }

        public PhaseSeries GetPhaseSeries(PhaseSeriesArgs args)
        {
            if (string.IsNullOrEmpty(args.TickerShift) || string.IsNullOrEmpty(args.TickerHistorical))
                return null;

            var seriesShift = new List<PhaseSeriesPoint>();
            var seriesHistorical = new List<PhaseSeriesPoint>();

            var mtSeries = new PhaseSeries(seriesShift, seriesHistorical);

            var pointsShift = _dataRetrievalService.GetPoints(args.TickerShift);
            var pointsHistorical = _dataRetrievalService.GetPoints(args.TickerHistorical);

            var deflate = !string.IsNullOrEmpty(args.TickerInflation) && args.TickerInflation.Trim() != "$";
            if (deflate)
            {
                _historicRatioAlgo.SetRoundCount(12);
                pointsShift = Deflate(args.TickerShift, args.TickerInflation);
                pointsHistorical = Deflate(args.TickerHistorical, args.TickerInflation);
            }

            if (!pointsShift.Any() || !pointsHistorical.Any())
                return null;


            var originationPointShift = FindOriginationPoint(pointsShift, args.StartDateShift, null, out DateTime? originationDateShift, out DayOfWeek? dayOfWeekShift);
            var originationPointHistorical = FindOriginationPoint(pointsHistorical, args.StartDateHistorical, dayOfWeekShift, out DateTime? originationDateHistorical, out DayOfWeek? dayOfWeekHistorical);

            if (originationDateShift == null || originationDateHistorical == null)
                return null;

            var goldenRatio = (1 - args.OffSet) *originationPointShift.ClosingPrice / originationPointHistorical.ClosingPrice;

            ApplyHistoricalScale(pointsHistorical, goldenRatio);
            ApplyHistoricalShift(pointsShift, originationDateShift.Value, originationDateHistorical.Value);

            var startDate = _dateGetter.GetFirstDate(pointsShift, pointsHistorical);
            var endDate = _dateGetter.GetLastDate(pointsShift, pointsHistorical);

            var extrapolationMapA = new Dictionary<DateTime, LineModel>();
            var extrapolationMapB = new Dictionary<DateTime, LineModel>();

            var shiftIndex = new EngineIndex(0);
            var historicalIndex = new EngineIndex(0);

            var leftBoundDtY = pointsShift.First().Date > pointsHistorical.First().Date ? pointsShift.First().Date : pointsHistorical.First().Date;
            var leftBoundDtX = leftBoundDtY;

            var foundFirstShiftPoint = false;
            var foundFirstHistoricalPoint = false;
            for (var dt = startDate.Date; dt.Date <= endDate.Date; dt = dt.AddDays(1))
            {
                if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                CalculatePhaseSeries(
                    pointsShift, pointsHistorical, 
                    extrapolationMapA, extrapolationMapB, dt.Date,
                    shiftIndex, historicalIndex,
                    seriesShift, seriesHistorical,
                    ref foundFirstShiftPoint, ref foundFirstHistoricalPoint,
                    ref leftBoundDtY, ref leftBoundDtX);
            }

            if (deflate)
                ScaleSeries(seriesShift, seriesHistorical);

            return mtSeries;
        }

        private void ScaleSeries(IList<PhaseSeriesPoint> seriesShift, IList<PhaseSeriesPoint> seriesHistorical)
        {
            if (!seriesShift.Any())
                return;

            var scalingFactor = 0;
            var firstPoint = seriesShift[0];

            var price = firstPoint.Price;
            while (price < 1.0M)
            {
                price *= 10;
                scalingFactor++;
            }

            // have to do both by same scaling factor
            foreach (var p in seriesShift)
            {
                p.Price = (decimal)Math.Pow(10, scalingFactor) * p.Price;
            }
            foreach (var p in seriesHistorical)
            {
                p.Price = (decimal)Math.Pow(10, scalingFactor) * p.Price;
            }
        }

        private void ApplyHistoricalShift(IList<Point> pointsToShift, DateTime originationDateShift, DateTime originationDateHistorical)
        {
            var diffSpan = originationDateShift - originationDateHistorical;
            foreach (var pointToShift in pointsToShift)
            {
                pointToShift.Date = pointToShift.Date.Subtract(diffSpan);
            }
        }

        private void CalculatePhaseSeries(
            IList<Point> pointsShift, IList<Point> pointsHistorical,
            IDictionary<DateTime, LineModel> dictA, IDictionary<DateTime, LineModel> dictB, DateTime date,
            EngineIndex shiftIndex, EngineIndex historicalIndex,
            IList<PhaseSeriesPoint> seriesShift, IList<PhaseSeriesPoint> seriesHistorical,
            ref bool alreadyFoundFirstShiftPoint, ref bool alreadyFoundFirstHistoricalPoint,
            ref DateTime leftBoundDtY, ref DateTime leftBoundDtX
        )
        {
            var priceShift = _engine.GetPrice(pointsShift, dictA, date, shiftIndex, ref leftBoundDtY, out bool foundPointShift, out bool isForwardExtrapolationShift);
            var priceHistorical = _engine.GetPrice(pointsHistorical, dictB, date, historicalIndex, ref leftBoundDtX, out bool foundPointHistorical, out bool isForwardExtrapolationHistorical);

            var mtfPointShift = new PhaseSeriesPoint(Math.Round(priceShift, _roundCount), date);
            var mtfPointHistorical = new PhaseSeriesPoint(Math.Round(priceHistorical, _roundCount), date);

            if(!alreadyFoundFirstShiftPoint)
                alreadyFoundFirstShiftPoint = foundPointShift;
            if(!alreadyFoundFirstHistoricalPoint)
                alreadyFoundFirstHistoricalPoint = foundPointHistorical;

            if (alreadyFoundFirstShiftPoint && !isForwardExtrapolationShift)
                seriesShift.Add(mtfPointShift);
            if (alreadyFoundFirstHistoricalPoint && !isForwardExtrapolationHistorical) 
                seriesHistorical.Add(mtfPointHistorical);
        }

        private void ApplyHistoricalScale(IList<Point> points, decimal goldenRatio)
        {
            foreach (var point in points)
            {
                point.ClosingPrice *= goldenRatio;
            }
        }

        private Point FindOriginationPoint(IList<Point> points, DateTime targetDate, DayOfWeek? targetDayOfWeek, out DateTime? actualTargetDate, out DayOfWeek? dayOfWeek)
        {
            actualTargetDate = null;
            dayOfWeek = null;
            foreach (var point in points)
            {
                var date = point.Date;
                if(date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                if (targetDate <= date && (targetDayOfWeek == null || date.DayOfWeek == targetDayOfWeek))
                {
                    actualTargetDate = date;
                    dayOfWeek = date.DayOfWeek;
                    return point;
                }
            }

            return null;
        }
    }
}