using Demo.Core.IFactory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JhEmrController : ControllerBase
    {
        private readonly ILogger<JhEmrController> _logger;
        private readonly IJhEmrService _emrService;

        public JhEmrController(ILogger<JhEmrController> logger, IJhEmrService emrService)
        {
            this._logger = logger;
            this._emrService = emrService;
        }
        [HttpPost,Route("jhmk")]
        public string jhmk([FromBody]dynamic dy)
        {
            JObject jObject = Function.GetJObject<JObject>(dy);
            if (!jObject.ContainsKey("hoscode"))
            {
                return "没有找到对应的参数hoscode";
            }
            if (!jObject.ContainsKey("clincNO"))
            {
                return "没有找到对应的参数clincNO";
            }
            string hoscode = jObject.GetValue("hoscode", StringComparison.OrdinalIgnoreCase).ToString();
            string clincNO = jObject.GetValue("clincNO", StringComparison.OrdinalIgnoreCase).ToString();
            this._logger.LogWarning($"接口调用入参：hoscode:{hoscode},clincNO:{clincNO}");
            return this._emrService.GetEmrData(hoscode, clincNO);
        }
    }
}
