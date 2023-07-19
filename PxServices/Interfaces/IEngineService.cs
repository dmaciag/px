using PricedX.Models;
using PxServices.Enums;
using PxServices.Models;
using System;
using System.Collections.Generic;

namespace PxServices.Interfaces
{
    public interface IEngine
    {
        bool ExistsPoint(IList<Point> y, DateTime dt, out Point pointModel);
        IList<Point> GetMovingAverage(IList<Point> p, int xDay);
        decimal GetPrice(IList<Point> points, IDictionary<DateTime, LineModel> dict, DateTime targetDate, EngineIndex yStartIdx, ref DateTime leftBoundDt, out bool pointFound, out bool isForwardExtrapolation);
        int GetDifferenceBetweenDates(DateTime dt1, DateTime dt2);
        decimal GetLogDistance(decimal limitRatio, decimal currentPriceRatio, DirectionPotential directionPotential, DistributionType distributionType);
        decimal ConvertDateToExcelInt(DateTime date);
        decimal CalcLog(decimal price);
        decimal GetYearlyGain(Point pointBefore, Point pointAfter);
        decimal CalculateMonthlyMtgPmt(decimal loanAmount, decimal rateAsPct);
        decimal GetLogTransformedValue(Line line, decimal xCord);
    }
}
