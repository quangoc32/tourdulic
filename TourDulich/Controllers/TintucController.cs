using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Services;

namespace TourDulich.Controllers
{
    public class TintucController : Controller
    {
        public ActionResult Index()
        {
            var store = new TinTucLinkStore(Server.MapPath("~/App_Data/tintuc-links.json"));
            var tinTucs = store.GetAll()
                .Where(x => x.HienThi)
                .OrderByDescending(x => x.LaTinHot)
                .ThenByDescending(x => x.NgayTao)
                .ToList();

            return View(tinTucs);
        }
    }
}
