using LyncPortable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Washroom.Models
{
    public class StatusViewModel
    {
        public string GetStatus()
        {
            //if (System.IO.File.Exists(Server.MapPath("~/flag.txt")))
            //{
            //    return Json(true);
            //}
            //else
            //{
            //    return Json(false);
            //}

            var client = new LyncHttpClientPortable();
            client.Init();

            return client.GetStatus();
        }
    }
}