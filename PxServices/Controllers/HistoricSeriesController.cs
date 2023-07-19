using Microsoft.AspNetCore.Mvc;
using PxServices.Interfaces;
using PxServices.Models;

namespace PxServices.Controllers
{
    [Route("historic-series")]
    [ApiController]
    public class HistoricSeriesController : ControllerBase
    {
        public IHistoricRatioAlgo _historicRatioAlgo { get; set; }

        public HistoricSeriesController(IHistoricRatioAlgo historicRatioAlgo)
        {
            _historicRatioAlgo = historicRatioAlgo;
        }


        [HttpGet("get-series")]
        public HistoricRatioSeries GetHistoricRatios([FromQuery] HistoricRatioArgs args)
        {
            var result = _historicRatioAlgo.GetHistoricRatioSeries(args);
            return result;
        }
    }
}
