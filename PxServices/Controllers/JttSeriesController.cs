using Microsoft.AspNetCore.Mvc;
using PxServices.Interfaces;
using PxServices.Models;

namespace PxServices.Controllers
{
    [Route("jtt-series")]
    [ApiController]
    public class JttSeriesController : ControllerBase
    {
        public IJttSeriesAlgo _jttSeriesAlgo { get; set; }

        public JttSeriesController(IJttSeriesAlgo jttSeriesAlgo)
        {
            _jttSeriesAlgo = jttSeriesAlgo;
        }

        [HttpGet("get-series")]
        public JttResult GetJttSeries([FromQuery] JttArgs jttArgs)
        {
            var result = _jttSeriesAlgo.RunAlgo(jttArgs);
            return result;
        }
    }
}
