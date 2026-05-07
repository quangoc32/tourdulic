using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;

namespace TourDulich.Controllers
{
    public class LienHeController : Controller
    {
        private ModelDB _contextDB = new ModelDB();

        // GET: LienHe
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GuiLienHeAjax(LienHe model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.HoTen) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.NoiDung))
                {
                    return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin bắt buộc." });
                }

                model.NgayGui = DateTime.Now;
                model.TrangThai = "Chưa xử lý";

                _contextDB.LienHes.Add(model);
                _contextDB.SaveChanges();

                return Json(new { success = true, message = "Gửi liên hệ thành công! Chúng tôi sẽ phản hồi sớm nhất." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi gửi: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _contextDB.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}