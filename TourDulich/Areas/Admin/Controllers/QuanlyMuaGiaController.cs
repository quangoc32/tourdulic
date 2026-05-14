using System;
using System.Linq;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.Areas.Admin.Filters;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyMuaGiaController : Controller
    {
        private ModelDB _db = new ModelDB();

        public ActionResult Index()
        {
            var list = _db.MuaGias.OrderByDescending(m => m.NgayBatDau).ToList();
            return View(list);
        }

        public ActionResult Create()
        {
            var today = DateTime.Today;
            return View(new MuaGia 
            { 
                HeSoGia = 1.00m,
                NgayBatDau = today,
                NgayKetThuc = today.AddMonths(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MuaGia model)
        {
            string strHeSo = Request.Form["HeSoGia"];
            if (!string.IsNullOrEmpty(strHeSo))
            {
                if (decimal.TryParse(strHeSo.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedHeSo))
                {
                    model.HeSoGia = parsedHeSo;
                    ModelState.Remove("HeSoGia");
                }
            }

            if (model.NgayKetThuc < model.NgayBatDau)
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu.");

            if (ModelState.IsValid)
            {
                _db.MuaGias.Add(model);
                _db.SaveChanges();
                TempData["Success"] = "Thêm mùa giá thành công!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var mua = _db.MuaGias.Find(id);
            if (mua == null) return HttpNotFound();
            return View(mua);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MuaGia model)
        {
            string strHeSo = Request.Form["HeSoGia"];
            if (!string.IsNullOrEmpty(strHeSo))
            {
                if (decimal.TryParse(strHeSo.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedHeSo))
                {
                    model.HeSoGia = parsedHeSo;
                    ModelState.Remove("HeSoGia");
                }
            }

            if (model.NgayKetThuc < model.NgayBatDau)
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu.");

            if (ModelState.IsValid)
            {
                var mua = _db.MuaGias.Find(model.ID_MuaGia);
                if (mua == null) return HttpNotFound();

                mua.TenMua = model.TenMua;
                mua.NgayBatDau = model.NgayBatDau;
                mua.NgayKetThuc = model.NgayKetThuc;
                mua.HeSoGia = model.HeSoGia;
                mua.MoTa = model.MoTa;
                mua.IsActive = model.IsActive;
                _db.SaveChanges();

                TempData["Success"] = "Cập nhật mùa giá thành công!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var mua = _db.MuaGias.Find(id);
                if (mua == null)
                    return Json(new { success = false, message = "Không tìm thấy mùa giá." });

                _db.MuaGias.Remove(mua);
                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ToggleActive(int id)
        {
            var mua = _db.MuaGias.Find(id);
            if (mua == null) return Json(new { success = false });
            mua.IsActive = !mua.IsActive;
            _db.SaveChanges();
            return Json(new { success = true, isActive = mua.IsActive });
        }

        // API: Tính giá thực tế cho một ngày (dùng cho DetailsTour)
        public JsonResult TinhGia(int tourId, string ngay)
        {
            if (!DateTime.TryParse(ngay, out DateTime ngayKH))
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var tour = _db.Tours.Find(tourId);
            if (tour == null || tour.Gia == null)
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            decimal giaGoc = tour.Gia.Value;

            // Lấy mùa giá đang active, ngày khởi hành nằm trong khoảng
            var muaApDung = _db.MuaGias
                .Where(m => m.IsActive
                         && m.NgayBatDau <= ngayKH
                         && m.NgayKetThuc >= ngayKH)
                .OrderByDescending(m => m.HeSoGia) // Ưu tiên hệ số cao nhất khi overlap
                .FirstOrDefault();

            decimal giaThucTe = muaApDung != null
                ? Math.Round(giaGoc * muaApDung.HeSoGia, 0)
                : giaGoc;

            return Json(new
            {
                success = true,
                giaGoc,
                giaThucTe,
                tenMua = muaApDung?.TenMua,
                heSo = muaApDung?.HeSoGia,
                coMua = muaApDung != null,
                tangGia = muaApDung != null && muaApDung.HeSoGia > 1
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
