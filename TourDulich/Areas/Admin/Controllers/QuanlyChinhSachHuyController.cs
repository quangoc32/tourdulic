using System;
using System.Linq;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.Areas.Admin.Filters;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyChinhSachHuyController : Controller
    {
        private ModelDB _db = new ModelDB();

        public ActionResult Index()
        {
            var list = _db.ChinhSachHuys.OrderByDescending(c => c.SoNgayTuHuy).ToList();
            return View(list);
        }

        public ActionResult Create()
        {
            return View(new ChinhSachHuy());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ChinhSachHuy model)
        {
            if (ModelState.IsValid)
            {
                _db.ChinhSachHuys.Add(model);
                _db.SaveChanges();
                TempData["Success"] = "Thêm chính sách hủy thành công!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var cs = _db.ChinhSachHuys.Find(id);
            if (cs == null) return HttpNotFound();
            return View(cs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ChinhSachHuy model)
        {
            if (ModelState.IsValid)
            {
                var cs = _db.ChinhSachHuys.Find(model.ID_ChinhSach);
                if (cs == null) return HttpNotFound();

                cs.SoNgayTuHuy = model.SoNgayTuHuy;
                cs.PhanTramHoan = model.PhanTramHoan;
                cs.MoTa = model.MoTa;
                _db.SaveChanges();

                TempData["Success"] = "Cập nhật chính sách thành công!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var cs = _db.ChinhSachHuys.Find(id);
                if (cs == null) return Json(new { success = false, message = "Không tìm thấy." });
                _db.ChinhSachHuys.Remove(cs);
                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Tính % hoàn tiền dựa trên số ngày trước khởi hành
        public JsonResult TinhHoanTien(int soNgay)
        {
            // Lấy chính sách phù hợp: điều kiện SoNgayTuHuy <= soNgay, lấy mức cao nhất
            var chinhSach = _db.ChinhSachHuys
                .Where(c => c.SoNgayTuHuy <= soNgay)
                .OrderByDescending(c => c.SoNgayTuHuy)
                .FirstOrDefault();

            return Json(new
            {
                phanTramHoan = chinhSach?.PhanTramHoan ?? 0,
                moTa = chinhSach?.MoTa ?? "Không được hoàn tiền"
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
