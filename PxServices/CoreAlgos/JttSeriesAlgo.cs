using PxServices.Enums;
using PxServices.Interfaces;
using PxServices.Models;
using PxServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PxServices.CoreAlgos
{
    public class JttSeriesAlgo : IJttSeriesAlgo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IDataRetrievalService _dataRetrievalService { get; set; }
        public IEngine _engine { get; set; }

        private const int _defaultDollarAmt = 1000;
        private const DistributionType _distributionType = DistributionType.Parabolic;

        public JttSeriesAlgo(IDataRetrievalService dataRetrievalService, IEngine engine)
        {
            _dataRetrievalService = dataRetrievalService;
            _engine = engine;
        }

        public JttResult RunAlgo(JttArgs jttArgs)
        {
            if (string.IsNullOrEmpty(jttArgs.TickerA) || string.IsNullOrEmpty(jttArgs.TickerB))
            {
                Logger.Warn($"Null arguments into Ratio/GetPoints: tickerMain: {jttArgs.TickerA} + tickerBenchmark={jttArgs.TickerB}");
                return null;
            }

            var dataPointsTop = _dataRetrievalService.GetPoints(jttArgs.TickerA);
            var dataPointsBot = _dataRetrievalService.GetPoints(jttArgs.TickerB);

            var config = new Config(jttArgs.StartAmount, jttArgs.StartYear, jttArgs.StartMonth, jttArgs.LastTickerAPrice, jttArgs.LastTickerBPrice, _distributionType);

            if (config.HasLastPredictedPrice)
            {
                dataPointsTop.Add(new Point(config.LastTopTickerPrice, DateTime.Now));
                dataPointsBot.Add(new Point(config.LasBotTickerPrice, DateTime.Now));
            }

            var portfolio = new Portfolio();
            portfolio.Add(new Instrument(jttArgs.TickerA, jttArgs.PctA, dataPointsTop));
            portfolio.Add(new Instrument(jttArgs.TickerB, jttArgs.PctB, dataPointsBot));

            var boundSet = MakeBound(
                jttArgs.JttBoundTopOneDt,
                jttArgs.JttBoundTopTwoDt,
                jttArgs.JttBoundBottomOneDt,
                jttArgs.JttBoundBottomTwoDt,
                jttArgs.JttBoundTopValueOne,
                jttArgs.JttBoundTopValueTwo,
                jttArgs.JttBoundBottomValueOne,
                jttArgs.JttBoundBottomValueTwo
            );

            if (boundSet == null)
            {
                Logger.Error($"Bound data object is NULL or Invalid with params: " +
                             $"jttBoundTopOneDt: {jttArgs.JttBoundTopOneDt}, jttBoundTopTwoDt:{jttArgs.JttBoundTopTwoDt}, " +
                             $"jttBoundBottomOneDt:{jttArgs.JttBoundBottomOneDt}, jttBoundBottomTwoDt: {jttArgs.JttBoundBottomTwoDt}.");
                return null;
            }

            if (jttArgs.StartAmount == 0)
                config.TrySetAmount(_defaultDollarAmt);

            Logger.Info($"Jtt Config details: {config}");

            var ratioPoints = GetResultingDataPoints(
                portfolio,
                config,
                out var upperBoundPoints,
                out var lowerBoundPoints,
                out var jttPoints,
                out var topTickerPoints,
                out var lowerTickerPoints,
                boundSet
             );

            Logger.Info($"Synthetic Portfolio done. Ratio result count: {ratioPoints.Count()}.");

            var log2DistanceUpward = _engine.GetLogDistance(
                upperBoundPoints.Last().ClosingPrice,
                ratioPoints.Last().ClosingPrice,
                DirectionPotential.Upward,
                _distributionType
            );

            var log2DistanceDownward = _engine.GetLogDistance(
                lowerBoundPoints.Last().ClosingPrice,
                ratioPoints.Last().ClosingPrice,
                DirectionPotential.Downward,
                _distributionType
            );

            var targetPctAllocationToTop = (int)((log2DistanceUpward) / (log2DistanceUpward + log2DistanceDownward) * 100);
            var targetPctAllocationToBottom = 100 - targetPctAllocationToTop;

            return new JttResult()
            {
                RatioPoints = ratioPoints,
                UpperBoundPoints = upperBoundPoints,
                LowerBoundPoints = lowerBoundPoints,
                JttPoints = jttPoints,
                TopTickerPoints = topTickerPoints,
                LowerTickerPoints = lowerTickerPoints,
                EndingAmount = (int)jttPoints[jttPoints.Count - 1].ClosingPrice,
                YearlyGainTop = _engine.GetYearlyGain(topTickerPoints[0], topTickerPoints.Last()),
                YearlyGainJtt = _engine.GetYearlyGain(jttPoints[0], jttPoints.Last()),
                YearlyGainBottom = _engine.GetYearlyGain(lowerTickerPoints[0], lowerTickerPoints.Last()),
                JttTargetAllocTop = targetPctAllocationToTop,
                JttTargetAllocBottom = targetPctAllocationToBottom,
                Log2DistanceUpward = decimal.Round(log2DistanceUpward, 2),
                Log2DistanceDownard = decimal.Round(log2DistanceDownward, 2)
            };
        }

        private IList<Point> GetResultingDataPoints(Portfolio syntheticPortfolio, Config syntheticConfiguration, out IList<Point> upperBoundPoints, out IList<Point> lowerBoundPoints, out IList<Point> jttPoints, out IList<Point> upperPoints, out IList<Point> lowerPoints, BoundSet boundSet, bool rebalancing = false)
        {
            upperBoundPoints = new List<Point>();
            lowerBoundPoints = new List<Point>();
            jttPoints = new List<Point>();
            upperPoints = new List<Point>();
            lowerPoints = new List<Point>();

            if (!syntheticPortfolio.IsValid())
            {
                Logger.Info("Invalid Portfolio! Not making.");
                return new List<Point>();
            }

            if (!IsValid(boundSet))
            {
                Logger.Warn($"boundDataObj is Invalid. Not proceeding with synthetic calc!");
                return new List<Point>();
            }

            //only supports 2 at a time
            var points = new List<Point>();
            var instruments = syntheticPortfolio.GetInstruments();
            int indxA = 0;
            int indxB = 0;

            var instrumentTop = instruments[0];
            var instrumentBottom = instruments[1];

            int maxLenA = instrumentTop.DataPoints.Count;
            int maxLenB = instrumentBottom.DataPoints.Count;

            decimal startWithCash = syntheticConfiguration.StartAmount;

            var topQty = 0.0M;
            var bottomQty = 0.0M;

            var originalTopQty = 0.0M;
            var originalBottomQty = 0.0M;

            var continueLooping = true;
            while (continueLooping)
            {
                continueLooping = false;

                while (indxA < maxLenA - 1 && instrumentTop.DataPoints[indxA].Date < instrumentBottom.DataPoints[indxB].Date)
                {
                    indxA++;
                    continueLooping = true;
                }

                while (indxB < maxLenB - 1 && instrumentTop.DataPoints[indxA].Date > instrumentBottom.DataPoints[indxB].Date)
                {
                    indxB++;
                    continueLooping = true;
                }

                var sameDate = instrumentTop.DataPoints[indxA].Date == instrumentBottom.DataPoints[indxB].Date;
                if (!sameDate || !IsPassedLeftDateBound(instrumentTop.DataPoints[indxA].Date, syntheticConfiguration))
                    continue;

                var currentDate = instrumentTop.DataPoints[indxA].Date;
                var pointTop = instrumentTop.DataPoints[indxA];
                var pointBottom = instrumentBottom.DataPoints[indxB];

                var xCoordinate = _engine.ConvertDateToExcelInt(currentDate);

                var yUpperLimit = _engine.GetLogTransformedValue(boundSet.UpperBoundLine, xCoordinate);
                var yLowerLimit = _engine.GetLogTransformedValue(boundSet.LowerBoundLine, xCoordinate);

                var currentPriceRatio = instrumentTop.DataPoints[indxA].ClosingPrice / instrumentBottom.DataPoints[indxB].ClosingPrice;

                var log2DistanceUpward = _engine.GetLogDistance(yUpperLimit, currentPriceRatio, DirectionPotential.Upward, syntheticConfiguration.DistributionType);
                var log2DistanceDownward = _engine.GetLogDistance(yLowerLimit, currentPriceRatio, DirectionPotential.Downward, syntheticConfiguration.DistributionType);

                if (topQty == 0 && bottomQty == 0)
                {
                    originalTopQty = startWithCash / pointTop.ClosingPrice;
                    originalBottomQty = startWithCash / pointBottom.ClosingPrice;
                }

                var notionalValue = (topQty == 0 && bottomQty == 0) ? startWithCash : (pointTop.ClosingPrice * topQty + pointBottom.ClosingPrice * bottomQty);
                RepositionQuantities(ref topQty, ref bottomQty, notionalValue, log2DistanceDownward, log2DistanceUpward, pointTop, pointBottom);

                var totalCashWorth = topQty * pointTop.ClosingPrice + bottomQty * pointBottom.ClosingPrice;

                var syntheticPoint = new Point(pointTop.ClosingPrice / pointBottom.ClosingPrice, currentDate);
                var upperBoundPoint = new Point(yUpperLimit, currentDate);
                var lowerBoundPoint = new Point(yLowerLimit, currentDate);
                var jttPoint = new Point(totalCashWorth, currentDate);
                var upperPoint = new Point(originalTopQty * pointTop.ClosingPrice, currentDate);
                var lowerPoint = new Point(originalBottomQty * pointBottom.ClosingPrice, currentDate);

                points.Add(syntheticPoint);
                upperBoundPoints.Add(upperBoundPoint);
                lowerBoundPoints.Add(lowerBoundPoint);
                jttPoints.Add(jttPoint);
                upperPoints.Add(upperPoint);
                lowerPoints.Add(lowerPoint);

                if (sameDate && indxA < maxLenA - 1)
                {
                    indxA++;
                    continueLooping = true;
                }
            }

            return points;
        }

        public bool IsValid(BoundSet boundSet)
        {
            return boundSet != null;
        }

        private void RepositionQuantities(ref decimal topQty, ref decimal bottomQty,
            decimal notionalValue, decimal log2DistanceDownward, decimal log2DistanceUpward,
            Point pointTop, Point pointBottom)
        {
            if (log2DistanceDownward == 0)
            {
                bottomQty = 0;
                topQty = notionalValue / pointTop.ClosingPrice;

                return;
            }
            
            if (log2DistanceUpward == 0)
            {
                topQty = 0;
                bottomQty = notionalValue / pointBottom.ClosingPrice;

                return;
            }

            bottomQty = notionalValue /
                            ((pointBottom.ClosingPrice) * (log2DistanceUpward / log2DistanceDownward + 1));
            topQty = (log2DistanceUpward * pointBottom.ClosingPrice * bottomQty) /
                     (log2DistanceDownward * pointTop.ClosingPrice);
        }

        private bool IsPassedLeftDateBound(DateTime date, Config syntheticConfiguration)
        {
            if (syntheticConfiguration == null)
                return true;

            return syntheticConfiguration.StartYear <= date.Year && syntheticConfiguration.StartMonth <= date.Month;
        }

        private BoundSet MakeBound(
            string jttBoundTopOneDt, string jttBoundTopTwoDt,
            string jttBoundBottomOneDt, string jttBoundBottomTwoDt,
            decimal jttBoundTopOneValue, decimal jttBoundTopTwoValue,
            decimal jttBoundBottomOneValue, decimal jttBoundBottomTwoValue
            )
        {
            var topTwoDt = DateTime.MinValue;
            var topOneDt = DateTime.MinValue;
            var bottomTwoDt = DateTime.MinValue;
            var bottomOneDt = DateTime.MinValue;

            var isValidToMakeBound =
                DateTime.TryParse(jttBoundTopOneDt, out topOneDt) &&
                DateTime.TryParse(jttBoundTopTwoDt, out topTwoDt) &&
                DateTime.TryParse(jttBoundBottomOneDt, out bottomOneDt) &&
                DateTime.TryParse(jttBoundBottomTwoDt, out bottomTwoDt) &&
                jttBoundTopOneValue != 0 &&
                jttBoundTopTwoValue != 0 &&
                jttBoundBottomOneValue != 0 &&
                jttBoundBottomTwoValue != 0;

            if (!isValidToMakeBound)
                return null;

            var topTwoPoints = new Tuple<Point, Point>(
                new Point(jttBoundTopOneValue, topOneDt), 
                new Point(jttBoundTopTwoValue, topTwoDt)
            );

            var bottomTwoPoints = new Tuple<Point, Point>(
                new Point(jttBoundBottomOneValue, bottomOneDt), 
                new Point(jttBoundBottomTwoValue, bottomTwoDt)
            );

            var upperCoordinateSet = new CoordinateSet()
            {
                x2 = _engine.ConvertDateToExcelInt(topTwoPoints.Item2.Date),
                x1 = _engine.ConvertDateToExcelInt(topTwoPoints.Item1.Date),
                y2 = _engine.CalcLog(topTwoPoints.Item2.ClosingPrice),
                y1 = _engine.CalcLog(topTwoPoints.Item1.ClosingPrice)
            };

            var bottomCoordinateSet = new CoordinateSet()
            {
                x2 = _engine.ConvertDateToExcelInt(bottomTwoPoints.Item2.Date),
                x1 = _engine.ConvertDateToExcelInt(bottomTwoPoints.Item1.Date),
                y2 = _engine.CalcLog(bottomTwoPoints.Item2.ClosingPrice),
                y1 = _engine.CalcLog(bottomTwoPoints.Item1.ClosingPrice)
            };

            return new BoundSet(upperCoordinateSet, bottomCoordinateSet);
        }
    }
}