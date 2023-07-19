using PricedX.Models;
using PricedX.Utils;
using PxServices.Enums;
using PxServices.Interfaces;
using PxServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PxServices.CoreAlgos
{
    public class HistoricRatioAlgo : IHistoricRatioAlgo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IDataRetrievalService _dataRetrievalService { get; set; }
        public IEngine _engine { get; set; }
        public IDateGetter _dateGetter { get; set; }

        public int _roundCount { get; set; }


        public HistoricRatioAlgo(IEngine pointService, IDataRetrievalService dataRetrievalService, IEngine engine)
        {
            _dataRetrievalService = dataRetrievalService;
            _engine = engine;
            _dateGetter = new InnerDateGetter();
            _roundCount = 6;
        }

        public HistoricRatioSeries GetHistoricRatioSeries(HistoricRatioArgs args)
        {
            var calcType = args.IsMtg ? CalcType.Mortgage : CalcType.Default;
            if (!ValidateArgs(args))
                return null;

            var dataPointsTop = _dataRetrievalService.GetPoints(args.TickerMain);
            var dataPointsBottom =  _dataRetrievalService.GetPoints(args.TickerBenchmark);

            var ratioPoints = GetHistoricRatioPoints(dataPointsTop, dataPointsBottom, calcType, args.UseRelativeAvg, args.Scale);

            return new HistoricRatioSeries()
            {
                RatioPoints = ratioPoints
            };
        }

        public void SetRoundCount(int roundCount)
        {
            _roundCount = roundCount;
        }

        private IList<HistoricRatioPoint> GetHistoricRatioPoints(IList<Point> y, IList<Point> x, CalcType calcType, bool useRelativeAvg = false, bool scale = true)
        {
            var historicRatios = new List<HistoricRatioPoint>();

            var countY = y.Count();
            var countX = x.Count();

            if (countY == 0 || countX == 0)
                return historicRatios;

            var startDate = _dateGetter.GetFirstDate(y, x);
            var endDate = _dateGetter.GetLastDate(y, x);

            var dictY = new Dictionary<DateTime, LineModel>();
            var dictX = new Dictionary<DateTime, LineModel>();

            var leftBoundDtY = y.First().Date > x.First().Date ? y.First().Date : x.First().Date;
            var leftBoundDtX = leftBoundDtY;

            var priceSum = 0.0m;

            var yStartIndex = new EngineIndex(0);
            var xStartIndex = new EngineIndex(0);

            for (var dt = startDate.Date; dt.Date <= endDate.Date; dt = dt.AddDays(1))
            {
                if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var historicRatio = GetResultingPoint(y, x, calcType, dictY, dictX, dt, yStartIndex, xStartIndex, ref leftBoundDtY, ref leftBoundDtX);
                historicRatios.Add(historicRatio);
                priceSum += historicRatio.Ratio;
            }

            if (useRelativeAvg)
            {
                var average = priceSum / historicRatios.Count();
                foreach (var p in historicRatios)
                {
                    p.Ratio = p.Ratio / average;
                }
            }

            if (scale)
            {
                ScalePoints(historicRatios);
            }
            return historicRatios;
        }

        private void ScalePoints(IList<HistoricRatioPoint> historicRatios)
        {
            if (!historicRatios.Any())
                return;

            var scalingFactor = 0;
            var firstPoint = historicRatios[0];
            if (firstPoint.Ratio < 1.0M)
                scalingFactor = 1;
            if (firstPoint.Ratio < .1M)
                scalingFactor = 2;
            if (firstPoint.Ratio < .01M)
                scalingFactor = 3;
            if (firstPoint.Ratio < .001M)
                scalingFactor = 4;
            if (firstPoint.Ratio < .0001M)
                scalingFactor = 5;
            if (firstPoint.Ratio < .0001M)
                scalingFactor = 6;
            if (firstPoint.Ratio < .00001M)
                scalingFactor = 7;

            foreach (var p in historicRatios)
            {
                p.Ratio = (decimal)Math.Pow(10, scalingFactor) * p.Ratio;
            }
        }
        

        private HistoricRatioPoint GetResultingPoint(IList<Point> y, IList<Point> x, CalcType calcType, IDictionary<DateTime, LineModel> dictX, IDictionary<DateTime, LineModel> dictY, DateTime date, EngineIndex yStartIdx, EngineIndex xStartIdx, ref DateTime leftBoundDtY, ref DateTime leftBoundDtX)
        {
            var priceY = _engine.GetPrice(y, dictY, date, yStartIdx, ref leftBoundDtY, out _, out _);
            var priceX = _engine.GetPrice(x, dictX, date, xStartIdx, ref leftBoundDtX, out _, out _);

            if (priceX <= 0) // dirac delta hack
                priceX = 0.0000001M;

            var ratio = Math.Round(priceY / priceX, _roundCount);
            if (calcType == CalcType.Mortgage)
                ratio = _engine.CalculateMonthlyMtgPmt(priceY, priceX);

            return new HistoricRatioPoint(ratio, date);
        }

        private bool ValidateArgs(HistoricRatioArgs args)
        {
            if(string.IsNullOrEmpty(args.TickerMain) || string.IsNullOrEmpty(args.TickerBenchmark))
            {
                Logger.Warn("Null arguments into Ratio/GetPoints: tickerMain: "
                            + args.TickerMain + ", tickerBenchmark: " + args.TickerBenchmark);
                return false;
            }
            return true;
        }

    }
}