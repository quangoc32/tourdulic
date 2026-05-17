using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.Services;

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
        public ActionResult DangKy(NguoiDung model, string XacNhanMatKhau)
        {
            if (ModelState.IsValid)
            {
                if (model.MatKhau != XacNhanMatKhau)
                {
                    TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                    return View(model);
                }

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
                    model.MatKhau = PasswordHasher.HashPassword(model.MatKhau);

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
                .FirstOrDefault(u => u.TaiKhoan == taiKhoan && u.PhanQuyen == 1);

            if (user != null)
            {
                if (!PasswordHasher.VerifyPassword(user.MatKhau, matKhau))
                {
                    TempData["Error"] = "Tài khoản hoặc mật khẩu không đúng!";
                    return View();
                }

                if (PasswordHasher.NeedsRehash(user.MatKhau))
                {
                    user.MatKhau = PasswordHasher.HashPassword(matKhau);
                    _contextDB.SaveChanges();
                }

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
