using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;

namespace TourDulich.Controllers
{
    public class NguoidungController : Controller
    {
        private ModelDB _contextDB = new ModelDB();

        // GET: DangKy
        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(NguoiDung model)
        {
            if (ModelState.IsValid)
            {
                bool taiKhoanExists = _contextDB.NguoiDungs.Any(u => u.TaiKhoan == model.TaiKhoan);
                bool emailExists = _contextDB.NguoiDungs.Any(u => u.Email == model.Email);

                if (taiKhoanExists)
                {
                    TempData["Error"] = "Tài khoản đã tồn tại!";
                    return View();
                }

                if (emailExists)
                {
                    TempData["Error"] = "Email đã được sử dụng!";
                    return View();
                }

                try
                {
                    model.PhanQuyen = 1;
                    model.NgayTao = DateTime.Now;

                    _contextDB.NguoiDungs.Add(model);
                    _contextDB.SaveChanges();

                    TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    TempData["RedirectUrl"] = Url.Action("DangNhap", "Nguoidung");
                    return View(); 
                }
                catch (Exception)
                {
                    TempData["Error"] = "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại.";
                }
            }

            return View(model);
        }

        // GET: DangNhap
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(string taiKhoan, string matKhau)
        {
            var user = _contextDB.NguoiDungs
                .FirstOrDefault(u => u.TaiKhoan == taiKhoan && u.MatKhau == matKhau && u.PhanQuyen == 1);

            if (user != null)
            {
                Session["ID_NguoiDung"] = user.ID_NguoiDung;
                Session["HoTen"] = user.HoTen;
                Session["PhanQuyen"] = user.PhanQuyen;

                TempData["Success"] = "Đăng nhập thành công!";
                TempData["RedirectUrl"] = Url.Action("Index", "Home");
                return View(); 
            }

            TempData["Error"] = "Tài khoản hoặc mật khẩu không đúng!";
            return View();
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");

        }
    }
}
