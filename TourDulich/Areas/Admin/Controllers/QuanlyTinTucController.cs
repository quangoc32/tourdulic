using System.Linq;
using System.Web.Mvc;
using TourDulich.Areas.Admin.Filters;
using TourDulich.ModelView;
using TourDulich.Services;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyTinTucController : Controller
    {
        private TinTucLinkStore Store => new TinTucLinkStore(Server.MapPath("~/App_Data/tintuc-links.json"));

        public ActionResult Index(string searchString)
        {
            var items = Store.GetAll();
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                items = items
                    .Where(x => x.TieuDe.Contains(searchString) || x.MoTaNgan.Contains(searchString) || (x.Nguon ?? "").Contains(searchString))
                    .ToList();
            }

            ViewBag.SearchString = searchString;
            return View(items.OrderByDescending(x => x.LaTinHot).ThenByDescending(x => x.NgayTao).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(TinTucLinkView model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Thông tin bài viết chưa hợp lệ. Kiểm tra tiêu đề, mô tả và link.";
                return RedirectToAction("Index");
            }

            Store.Add(model);
            TempData["Success"] = "Đã thêm tin tức.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Sua(int id)
        {
            var item = Store.Find(id);
            if (item == null) return HttpNotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(TinTucLinkView model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!Store.Update(model))
            {
                return HttpNotFound();
            }

            TempData["Success"] = "Đã cập nhật tin tức.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Xoa(int id)
        {
            if (!Store.Delete(id))
            {
                TempData["Error"] = "Không tìm thấy tin tức cần xóa.";
            }
            else
            {
                TempData["Success"] = "Đã xóa tin tức.";
            }

            return RedirectToAction("Index");
        }
    }
}
