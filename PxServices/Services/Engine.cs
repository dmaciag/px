
using System;
using System.Collections.Generic;
using System.Linq;
using PricedX.Models;
using PxServices.Enums;
using PxServices.Interfaces;
using PxServices.Models;

namespace PxServices.Services
{
    public class Engine : IEngine
    {
        private static int _logBase = 2;

        public LineModel GetLine(IList<Point> p, IDictionary<DateTime, LineModel> dict, DateTime dt, DateTime leftBoundDt)
        {
            if (dict.ContainsKey(leftBoundDt))
                return dict[leftBoundDt];

            var line = new LineModel();

            int dateDiff;
            decimal priceDiff;
            if (p.Last().Date == dt)
            {
                var leftPosition = p.Count() - 2;
                var rightPosition = p.Count() - 1;

                dateDiff = GetDifferenceBetweenDates(p[leftPosition].Date, p[rightPosition].Date);
                priceDiff = p[rightPosition].ClosingPrice - p[leftPosition].ClosingPrice;

                line.Slope = priceDiff / (decimal)dateDiff;
                line.Intercept = p[leftPosition].ClosingPrice;
                line.DateBegin = p[leftPosition].Date.Date;

                dict[dt] = line;

                return line;
            }

            // O(n) is acceptable
            var rightwardPoints = p.Where(o => o.Date >= leftBoundDt).ToList();
            int k = 0;

            while (++k < rightwardPoints.Count())
            {
                if (rightwardPoints[k].Date.DayOfWeek != DayOfWeek.Saturday && rightwardPoints[k].Date.DayOfWeek != DayOfWeek.Sunday)
                    break;
            }

            if (k >= rightwardPoints.Count())
            {
                k = rightwardPoints.Count() - 1;
            }

            if (k < 0)
                k = 0;

            dateDiff = k == 0 ? 0 : GetDifferenceBetweenDates(rightwardPoints[0].Date, rightwardPoints[k].Date);
            priceDiff = k == 0 ? 0 : rightwardPoints[k].ClosingPrice - rightwardPoints[0].ClosingPrice;

            if (dateDiff == 0)
                dateDiff = 1;

            line.Slope = priceDiff / dateDiff;
            line.Intercept = rightwardPoints[0].ClosingPrice;
            line.DateBegin = rightwardPoints[0].Date.Date;

            dict[leftBoundDt] = line;

            return line;
        }
        
        public decimal GetPrice(IList<Point> points, IDictionary<DateTime, LineModel> dict, DateTime targetDate, EngineIndex idx, ref DateTime leftBoundDt, out bool pointFound, out bool isForwardExtrapolation)
        {
            decimal price = -1.0M;
            var lastDate = points.Last().Date;
            isForwardExtrapolation = false;
            if(targetDate > lastDate)
                isForwardExtrapolation = true;

            pointFound = false;
            for (int i = idx.StartIndex; i < points.Count; i++)
            {
                var date = points[i].Date;
                idx.Set(i);
                if (date == targetDate)
                {
                    price = points.First(o => o.Date == targetDate).ClosingPrice;
                    leftBoundDt = targetDate;
                    pointFound = true;
                    return price;
                }

                if (date > targetDate)
                    break;
            }

            return InterpolatePrice(points, dict, targetDate, ref leftBoundDt);
        }

        public decimal InterpolatePrice(IList<Point> points, IDictionary<DateTime, LineModel> dict, DateTime targetDate, ref DateTime leftBoundDt)
        {
            var lineModel = GetLine(points, dict, targetDate, leftBoundDt);
            var price = lineModel.Slope * GetDifferenceBetweenDates(lineModel.DateBegin, targetDate) + lineModel.Intercept;

            return price;
        }

        public decimal GetLogDistance(decimal limitRatio, decimal currentPriceRatio, DirectionPotential directionPotential, DistributionType distributionType)
        {
            if (currentPriceRatio > limitRatio && directionPotential == DirectionPotential.Upward)
                currentPriceRatio = limitRatio;

            if (currentPriceRatio < limitRatio && directionPotential == DirectionPotential.Downward)
                currentPriceRatio = limitRatio;

            var directionFactor = directionPotential == DirectionPotential.Downward ? -1 : 1;
            var linearDistance = directionFactor * (decimal)Math.Log((double)(limitRatio / currentPriceRatio), _logBase);
            return distributionType == DistributionType.Parabolic ? (decimal)Math.Pow((double)linearDistance, _logBase) : linearDistance;
        }

        public IList<Point> GetMovingAverage(IList<Point> p, int xDay)
        {
            var movingAverage = new List<Point>();
            var movingPrices = new Queue<decimal>();
            decimal average = 0;

            for (int i = 0; i < p.Count(); i++)
            {
                var qCount = movingPrices.Count();
                var priceRemoved = 0.0m;
                var n = 0;
                if (qCount >= xDay)
                {
                    if (qCount >= 1)
                    {
                        priceRemoved = movingPrices.Dequeue();
                        n = 1;
                    }
                }

                movingPrices.Enqueue(p[i].ClosingPrice);
                average = (average * (movingPrices.Count() - 1 + n) + p[i].ClosingPrice - priceRemoved) / movingPrices.Count();
                var pointModel = new Point() { ClosingPrice = average, Date = p[i].Date };
                movingAverage.Add(pointModel);
            }

            return movingAverage;
        }

        public int GetDifferenceBetweenDates(DateTime dt1, DateTime dt2)
        {
            var dateDiff = 0;
            dt1 = dt1.Date;
            dt2 = dt2.Date;

            if (dt1 > dt2)
                return dateDiff;

            while (dt2 > dt1)
            {
                dt1 = dt1.AddDays(1);
                if (dt1.DayOfWeek == DayOfWeek.Saturday || dt1.DayOfWeek == DayOfWeek.Sunday)
                    continue;
                dateDiff++;
            }

            return dateDiff;
        }

        public decimal GetLogTransformedValue(Line line, decimal xCoord)
        {
            var power = (double)(line.M * xCoord + line.B);
            return GetInverseLog(_logBase, power);
        }

        public decimal ConvertDateToExcelInt(DateTime date)
        {
            var ticksInLong = ((decimal)date.Ticks) / 86400000.0M;
            var xCoordInExcelDayFormat = (ticksInLong - 6935930000.0M) / 10000.0M;
            return xCoordInExcelDayFormat;
        }

        public decimal CalcLog(decimal price)
        {
            return (decimal)Math.Log((double)price, _logBase);
        }

        public decimal GetYearlyGain(Point pointBefore, Point pointAfter)
        {
            var daysDiff = TimeSpan.FromTicks(pointAfter.Date.Ticks).Subtract(TimeSpan.FromTicks(pointBefore.Date.Ticks)).Days;
            var yearDifference = daysDiff / 365.25;
            var ratioAfterToBefore = (double)(pointAfter.ClosingPrice / pointBefore.ClosingPrice);

            return decimal.Round(((decimal)Math.Pow(ratioAfterToBefore, 1 / yearDifference) - 1) * 100, _logBase);

        }

        public bool ExistsPoint(IList<Point> y, DateTime dt, out Point pointModel)
        {
            pointModel = new Point();
            if (y.Count(p => p.Date == dt) == 0)
            {
                pointModel = y.First(p => p.Date == dt);
                return true;
            }

            return false;
        }

        public decimal CalculateMonthlyMtgPmt(decimal loanAmount, decimal rateAsPct)
        {
            //P = L[c(1 + c)n]/[(1 + c)n - 1]
            var loan = Convert.ToDouble(loanAmount);
            //var scalingFactor = (double)1000.0;
            var scalingFactor = (double)1.0M;
            var monthlyRate = (double)rateAsPct / 12 / 100;
            var xp = Math.Pow((double)(1 + monthlyRate), 360);
            var res = scalingFactor * loan * (monthlyRate * Math.Pow((double)(1 + monthlyRate), 360)) / (Math.Pow((double)(1 + monthlyRate), 360) - 1);
            return Convert.ToDecimal(res);
        }

        private decimal GetInverseLog(int logBase, double power)
        {
            return (decimal)Math.Pow(logBase, power);
        }

    }
}