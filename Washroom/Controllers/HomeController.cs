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

        public JsonResult GetStatus()
        {
            //if (System.IO.File.Exists(Server.MapPath("~/flag.txt")))
            //{
            //    return Json(true);
            //}
            //else
            //{
            //    return Json(false);
            //}
            return null;
      
        }

        //[HttpGet]
        //public ActionResult DisplayStatus()
        //{
        //    //StatusViewModel model = get

        //    return View(model);
        //}
    }
}
