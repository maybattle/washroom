using LyncPortable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Washroom.Controllers
{
    public class HomeController : Controller
    {
      

        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public JsonResult GetStatus()
        {

            using (var client = new LyncHttpClientPortable())
            {
                client.Init();

                return Json(client.GetStatus(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}
