using System.Linq;
using System.Web.Mvc;
using TourDulich.Areas.Admin.Filters;
using TourDulich.ModelView;
using TourDulich.Models;
using TourDulich.Services;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyDiemDonTourController : Controller
    {
        private readonly ModelDB _db = new ModelDB();
        private DiemDonTourStore Store => new DiemDonTourStore(Server.MapPath("~/App_Data/diemdon-tour.json"));

        public ActionResult Index(int tourId)
        {
            var tour = _db.Tours.FirstOrDefault(t => t.ID_Tour == tourId && !t.DaXoa);
            if (tour == null) return HttpNotFound();

            ViewBag.Tour = tour;
            return View(Store.GetByTour(tourId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(DiemDonTourView model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Thông tin điểm đón chưa hợp lệ.";
                return RedirectToAction("Index", new { tourId = model.ID_Tour });
            }

            Store.Add(model);
            TempData["Success"] = "Đã thêm điểm đón.";
            return RedirectToAction("Index", new { tourId = model.ID_Tour });
        }

        [HttpGet]
        public ActionResult Sua(int id)
        {
            var item = Store.Find(id);
            if (item == null) return HttpNotFound();

            ViewBag.Tour = _db.Tours.Find(item.ID_Tour);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(DiemDonTourView model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tour = _db.Tours.Find(model.ID_Tour);
                return View(model);
            }

            if (!Store.Update(model)) return HttpNotFound();

            TempData["Success"] = "Đã cập nhật điểm đón.";
            return RedirectToAction("Index", new { tourId = model.ID_Tour });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Xoa(int id, int tourId)
        {
            Store.Delete(id);
            TempData["Success"] = "Đã xóa điểm đón.";
            return RedirectToAction("Index", new { tourId });
        }
    }
}
