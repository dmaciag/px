using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PxServices.Interfaces;
using PxServices.Models;
using PxServices.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PxServices.Controllers
{
    [Route("phase-series")]
    [ApiController]
    public class PhaseSeriesController : ControllerBase
    {
        public IPhaseSeriesAlgo _phaseSeriesAlgo { get; set; }
        public IPhaseSeriesRepository _phaseSeriesRepository { get; set; }


        public PhaseSeriesController(
            IPhaseSeriesAlgo phaseSeriesAlgo,
            IPhaseSeriesRepository phaseSeriesRepository
        )
        {
            _phaseSeriesAlgo = phaseSeriesAlgo;
            _phaseSeriesRepository = phaseSeriesRepository;
        }

        [HttpGet("get-series")]
        public PhaseSeries GetPhaseSeries([FromQuery] PhaseSeriesArgs args)
        {
            var result = _phaseSeriesAlgo.GetPhaseSeries(args);
            return result;
        }

        [HttpGet("get-config")]
        public IList<PhaseSeriesConfig> GetConfigs()
        {
            return _phaseSeriesRepository.GetConfigs();
        }

        [HttpPost("save-config")]
        public IActionResult SaveConfig(PhaseSeriesConfig phaseSeriesConfig)
        {
             _phaseSeriesRepository.SaveConfig(phaseSeriesConfig);
            return Ok(200);
        }

        [HttpDelete("delete-config")]
        public IActionResult DeleteConfig(int configId)
        {
            _phaseSeriesRepository.DeleteConfig(configId);
            return Ok(200);
        }


    }
}
