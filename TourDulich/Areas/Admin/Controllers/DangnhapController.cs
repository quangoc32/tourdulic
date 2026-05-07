using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;

namespace TourDulich.Areas.Admin.Controllers
{
    public class DangnhapController : Controller
    {
        private ModelDB _contextDB = new ModelDB();
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string taiKhoan, string matKhau)
        {
            var nguoiDung = _contextDB.NguoiDungs.FirstOrDefault(x => x.TaiKhoan == taiKhoan && x.MatKhau == matKhau);

            if (nguoiDung == null)
            {
                ViewBag.ThongBao = "Sai tài khoản hoặc mật khẩu!";
                return View();
            }

            Session["NguoiDung"] = nguoiDung;
            Session["Quyen"] = nguoiDung.PhanQuyen;
            Session["AdminName"] = nguoiDung.HoTen;

            if (nguoiDung.PhanQuyen == 0)
            {
                return RedirectToAction("Index", "HomeAdmin");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Dangnhap");
        }
    }
}