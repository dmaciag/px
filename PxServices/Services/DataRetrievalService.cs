using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using log4net.Repository.Hierarchy;
using PxServices.Enums;
using PxServices.Interfaces;
using PxServices.Models;

namespace PxServices.Services
{
    public class DataRetrievalService : IDataRetrievalService
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string ReplaceTickerFlag = "REPLACE_TICKER_FLAG";
        private readonly string PathToCsvInput = $@"F:\Downloads\{ReplaceTickerFlag}.csv";

        private readonly string DL = ".DL";
        private readonly string Zillow = ".z";
        private readonly string Self = "$";

        public IList<Point> GetPoints(string tickerArg)
        {
            var ticker = tickerArg.Replace(DL, "").Replace(DL.ToLower(), "");
            var useCsv = tickerArg.Contains(DL) || tickerArg.Contains(DL.ToLower());

            if (ticker == Self)
                return GetPointsFromSelf();

            if (useCsv)
                return GetPointsFromCsv(ticker);

            return GetPointsFromApi<Point>(ticker, ExtractPoint);
        }

        public IList<PinBar> GetPinBars(string ticker)
        {
            return GetPointsFromApi<PinBar>(ticker, ExtractPinBar);
        }

        private IList<Point> GetPointsFromSelf()
        {
            var points = new List<Point>();

            for (DateTime dt = new DateTime(1900, 1, 1); dt.Date < DateTime.Now.Date; dt = dt.AddDays(1))
            {
                if (dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday)
                {
                    points.Add(new Point() { ClosingPrice = 1.0M, Date = dt.Date });
                }
            }

            return points;
        }

        private IList<Point> GetPointsFromCsv(string ticker)
        {
            var isZillow = ticker.Contains(Zillow);
            ticker = ticker.Replace(Zillow, "");

            var points = new List<Point>();
            try
            {
                var bufferSize = 128;
                using (var fileStream = File.OpenRead(PathToCsvInput.Replace(ReplaceTickerFlag, ticker)))
                {
                    var lineCount = 0;
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            lineCount++;

                            if (!IsValidCsvLine(line, out var columnData) && !isZillow)
                            {
                                Logger.Error($"LineModel {lineCount} for {ticker} is not a valid line.");
                                continue;
                            }

                            var pointModel = GetPointFromCsvLine(columnData, isZillow);
                            if(pointModel != null)
                                points.Add(pointModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"CSV exception: {ex}.");
            }

            return isZillow ? points.OrderBy(o=>o.Date).ToList() : points;
        }

        private IList<T> GetPointsFromApi<T>(string ticker, Func<string[], T> ExtractData)
        {
            var points = new List<T>();
            var dataLines = GetHttpData(ticker);
            var count = 0;
            foreach (string dataLine in dataLines)
            {
                if (count <= 0)
                {
                    count++;
                    continue;
                }

                var arrElements = dataLine.Split(',');
                if (arrElements.Length < 5)
                    continue;
                try
                {
                    var point = ExtractData(arrElements);
                    points.Add(point);
                }
                catch (Exception ex)
                {
                    Logger.Debug($"PointModel error: {count}: {ex}.");
                }
            }

            return points;
        }

        private Point ExtractPoint(string[] arrElements)
        {
            var point = new Point()
            {
                Date = Convert.ToDateTime(arrElements[0]),
                ClosingPrice = Convert.ToDecimal(arrElements[4]),
            };

            return point;
        }

        private PinBar ExtractPinBar(string[] arrElements)
        {
            var pinBar = new PinBar()
            {
                Date = Convert.ToDateTime(arrElements[0]),
                Open = Convert.ToDecimal(arrElements[1]),
                High = Convert.ToDecimal(arrElements[2]),
                Low = Convert.ToDecimal(arrElements[3]),
                Close = Convert.ToDecimal(arrElements[4]),
                Volume = (int)Convert.ToDecimal(arrElements[5])
            };

            return pinBar;
        }

        private bool IsValidCsvLine(string line, out string[] columnData)
        {
            columnData = line.Split(',');
            decimal price;
            try
            {
                var condition1 = columnData.Length >= 2;
                var condition2 = Convert.ToDateTime(columnData[0]) != null;
                var condition3 = decimal.TryParse(columnData[1], out price);
                return condition1 && condition2 && condition3;
            }
            catch (Exception)
            {
                //bkup
                try
                {
                    var condition1 = columnData.Length >= 2;
                    var condition2 = Convert.ToInt32(columnData[0]) >= 0;
                    var condition3 = decimal.TryParse(columnData[1], out price);
                    return condition1 && condition2 && condition3;
                }
                catch (Exception) { }
            }

            return false;
        }

        private decimal ExtractZillowPrice(string zillowPriceStr)
        {
            var zPriceStr = zillowPriceStr?.Trim().Replace("$","").Replace("K","");
            if(string.IsNullOrEmpty(zPriceStr) ) { return -1.0M; }

            return Convert.ToDecimal(zPriceStr) * 1000;
        }

        private Point GetPointFromCsvLine(string[] columnData, bool isZillow)
        {
            try
            {
                var dt = isZillow ? DateTime.ParseExact(columnData[0], "MMM-yy", CultureInfo.InvariantCulture) : Convert.ToDateTime(columnData[0]);
                while (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                {
                    dt = dt.AddDays(1);
                }

                return new Point()
                {
                    ClosingPrice = isZillow ? ExtractZillowPrice(columnData[1]) : Convert.ToDecimal(columnData[1]),
                    Date = dt.Date
                };
                
            }
            catch (Exception ex)
            {

                try
                {
                    var dt = new DateTime(Convert.ToInt32(columnData[0]), 1, 1);
                    while (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                    {
                        dt = dt.AddDays(-1);
                    }

                    return new Point()
                    {
                        ClosingPrice = Convert.ToDecimal(columnData[1]),
                        Date = dt.Date
                    };
                }
                catch (Exception)
                {
                    Logger.Debug(
                        $"Cannot GetPointFromCsvLine: dt: {columnData[0]}, price: {columnData[1]}, Reason: {ex}.");
                }
            }

            return null;
        }

        private ICollection<string> GetHttpData(string ticker)
        {
            ICollection<string> lines = new List<string>();
            try
            {
                var url = @"https://stooq.pl/q/d/l/?s=" + ticker + "&i=d";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Method = "GET";
                request.Accept = "application/csv";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    var html = reader.ReadToEnd();
                    lines = html.Split(new[] { "\n" }, StringSplitOptions.None);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(Convert.ToString(ex));
            }
            return lines;
        }
    }
}