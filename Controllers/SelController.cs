using System.Collections;
using System;
using Demo.Core.IFactory;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Demo.Core.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class SelController:ControllerBase
    {
        private readonly IService _service;

        public SelController(IService service)
        {
            this._service=service;
        }

        [HttpGet,Route("GetOper")]
        public IActionResult GetOper(string UserName){
            string sql=$"SELECT u.account,u.USERNAME,u.PASSWORD FROM zjhis.PRIV_COM_USER u WHERE  (u.account='{UserName}' or u.username='{UserName}')";
            ArrayList array=this._service.GetTableResult(sql);
            return new JsonResult(array);
        }
    }
}
