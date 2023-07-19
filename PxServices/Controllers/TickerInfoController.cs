using Microsoft.AspNetCore.Mvc;
using PxServices.Interfaces;
using PxServices.Models;

namespace PxServices.Controllers
{
    [Route("ticker-info")]
    [ApiController]
    public class TickerInfoController : ControllerBase
    {
        public ITickerInfoAlgo _tickerInfoAlgo { get; set; }

        public TickerInfoController(ITickerInfoAlgo tickerInfoAlgo)
        {
            _tickerInfoAlgo = tickerInfoAlgo;
        }


        [HttpGet("get-series")]
        public TickerDataResultSet GetTickerInfo([FromQuery] TickerDataSetArgs args)
        {
            var result = _tickerInfoAlgo.GetTickerInfos(args);
            return result;
        }
    }
}
