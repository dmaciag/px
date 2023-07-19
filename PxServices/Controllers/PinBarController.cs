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
    [Route("pin-bar")]
    [ApiController]
    public class PinBarController : ControllerBase
    {
        public IPinBarAlgo _pinBarAlgo { get; set; }
        public IPinBarRepository _pinBarRepository { get; set; }


        public PinBarController(
            IPinBarAlgo pinBarAlgo,
            IPinBarRepository pinBarRepository
        )
        {
            _pinBarAlgo = pinBarAlgo;
            _pinBarRepository = pinBarRepository;
        }

        [HttpGet("start-algo")]
        public void RunAlgo([FromQuery] PinBarAlgoConfig config)
        {
            _pinBarAlgo.StartAlgo(config);
        }
    }
}
