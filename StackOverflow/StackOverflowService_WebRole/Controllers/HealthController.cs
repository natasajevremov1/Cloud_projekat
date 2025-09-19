using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StackOverflowService_WebRole.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet]
        [Route("health-monitoring")]
        public ActionResult HealthMonitoring()
        {
            return Content("OK");
        }
    }
}
