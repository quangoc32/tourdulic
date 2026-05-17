using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.Areas.Admin.Filters;
using TourDulich.ModelView;
using TourDulich.Services;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyLienHeController : Controller
    {
        private ModelDB _contextDB = new ModelDB();

        private AdminEmailSettingStore EmailStore
        {
            get { return new AdminEmailSettingStore(Server.MapPath("~/App_Data/admin-email-settings.json")); }
        }

        public ActionResult Index(string searchString)
        {
            var query = _contextDB.LienHes.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(lh => lh.HoTen.Contains(searchString) || 
                                          lh.Email.Contains(searchString) || 
                                          lh.TieuDe.Contains(searchString));
            }

            var list = query.OrderByDescending(x => x.NgayGui).ToList();
            ViewBag.SearchString = searchString;

            var model = new AdminEmailSettingPageView
            {
                LienHes = list,
                EmailSetting = EmailStore.Get()
            };

            return View(model);
        }

        [HttpPost]
        public JsonResult LuuCauHinhEmail(AdminEmailSettingView model)
        {
            try
            {
                EmailStore.SaveSmtp(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult LuuGmailNhan(AdminEmailRecipientView model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.TenNguoiNhan) || string.IsNullOrWhiteSpace(model.Email))
                    return Json(new { success = false, message = "Vui lòng nhập tên và Gmail nhận thông báo." });

                if (model.Id > 0)
                {
                    if (!EmailStore.UpdateRecipient(model))
                        return Json(new { success = false, message = "Không tìm thấy Gmail cần cập nhật." });
                }
                else
                {
                    EmailStore.AddRecipient(model);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult GuiThuGmailAdmin()
        {
            try
            {
                var emailService = new LienHeEmailService(EmailStore);
                string errorMessage;
                if (!emailService.TrySendTestEmail(out errorMessage))
                {
                    return Json(new { success = false, message = errorMessage });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XoaGmailNhan(int id)
        {
            try
            {
                if (!EmailStore.DeleteRecipient(id))
                    return Json(new { success = false, message = "Không tìm thấy Gmail cần xóa." });

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DanhDauDaXuLy(int id)
        {
            try
            {
                var lh = _contextDB.LienHes.Find(id);
                if (lh != null)
                {
                    lh.TrangThai = "Đã xử lý";
                    _contextDB.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy yêu cầu liên hệ." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XoaLienHe(int id)
        {
            try
            {
                var lh = _contextDB.LienHes.Find(id);
                if (lh != null)
                {
                    _contextDB.LienHes.Remove(lh);
                    _contextDB.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy yêu cầu liên hệ." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
