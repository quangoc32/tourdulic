using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;

using TourDulich.Areas.Admin.Filters;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyNguoidungController : Controller
    {
        private ModelDB _contextDB = new ModelDB();

        public ActionResult Index(string searchString)
        {
            var query = _contextDB.NguoiDungs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.HoTen.Contains(searchString) || 
                                         u.TaiKhoan.Contains(searchString) || 
                                         u.Email.Contains(searchString));
            }

            ViewBag.SearchString = searchString;

            var users = query.ToList();
            return View(users);
        }

        [HttpGet]
        public ActionResult SuaNguoiDungPartial(int id)
        {
            var user = _contextDB.NguoiDungs.Find(id);
            if (user == null) return HttpNotFound();
            return PartialView("_SuaNguoiDungPartial", user);
        }

        [HttpPost]
        public JsonResult LuuSuaNguoiDung(NguoiDung model)
        {
            try
            {
                var existing = _contextDB.NguoiDungs.Find(model.ID_NguoiDung);
                if (existing == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

                existing.HoTen = model.HoTen;
                existing.Email = model.Email;
                existing.SoDienThoai = model.SoDienThoai;
                existing.DiaChi = model.DiaChi;

                if (!string.IsNullOrEmpty(model.MatKhau))
                {
                    existing.MatKhau = model.MatKhau;
                }

                _contextDB.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XoaNguoiDung(int id)
        {
            try
            {
                var user = _contextDB.NguoiDungs.Find(id);
                if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

                _contextDB.NguoiDungs.Remove(user);
                _contextDB.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}