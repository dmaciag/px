using PxServices.Enums;
using PxServices.Interfaces;
using PxServices.Models;
using PxServices.Services;
using System.Collections.Generic;
using System.Linq;

namespace PxServices.CoreAlgos
{
    public class TickerInfoAlgo : ITickerInfoAlgo
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TickerDataResultSet GetTickerInfos(TickerDataSetArgs args)
        {
            var tickerDataRows = new List<TickerDataRow>()
            {
                new TickerDataRow()
                {
                    Ticker = "NVDA",
                    ClosingPrice = 285.1M,
                    Outlook = 8
                },
                new TickerDataRow()
                {
                    Ticker = "AAPL",
                    ClosingPrice = 185.44M,
                    Outlook = 7
                },
                new TickerDataRow()
                {
                    Ticker = "BRK-B",
                    ClosingPrice = 266.11M,
                    Outlook = 6
                },
                new TickerDataRow()
                {
                    Ticker = "HD",
                    ClosingPrice = 54.52M,
                    Outlook = 5
                }
            };

            return new TickerDataResultSet()
            {
                TickerDataRows = tickerDataRows
            };
        }
    }
}