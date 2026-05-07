using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.ModelView;
using TourDulich.Areas.Admin.Filters;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyLichTrinhController : Controller
    {
        private ModelDB _contextDB = new ModelDB();

        public ActionResult Index(string searchString)
        {
            var toursQuery = _contextDB.Tours.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                toursQuery = toursQuery.Where(t => t.TenTour.Contains(searchString));
            }

            var tours = toursQuery
                .Select(t => new TourStatusView
                {
                    ID_Tour = t.ID_Tour,
                    TenTour = t.TenTour,
                    CoLichTrinh = _contextDB.LichTrinhTours.Any(lt => lt.ID_Tour == t.ID_Tour)
                }).ToList();

            ViewBag.SearchString = searchString;

            return View(tours);
        }

        public ActionResult GetLichTrinhPartial(int id)
        {
            var lichTrinhs = _contextDB.LichTrinhTours
                .Where(lt => lt.ID_Tour == id)
                .OrderBy(lt => lt.NgayThu)
                .ToList();

            return PartialView("_LichTrinhPartial", lichTrinhs);
        }

        [HttpGet]
        public ActionResult GetLichTrinhEditPartial(int id)
        {
            var tour = _contextDB.Tours.Find(id);
            if (tour == null) return HttpNotFound();

            int soNgay = 1;
            var match = Regex.Match(tour.SoNgay ?? "", @"\d+");
            if (match.Success)
            {
                soNgay = int.Parse(match.Value);
            }

            var lichTrinhList = _contextDB.LichTrinhTours
                                  .Where(x => x.ID_Tour == id)
                                  .ToList();

            var fullList = new List<LichTrinhTour>();

            for (int i = 1; i <= soNgay; i++)
            {
                var existing = lichTrinhList.FirstOrDefault(x => x.NgayThu == i);
                if (existing != null)
                {
                    fullList.Add(existing);
                }
                else
                {
                    fullList.Add(new LichTrinhTour
                    {
                        ID_Tour = id,
                        NgayThu = i,
                        TieuDe = "",
                        NoiDung = ""
                    });
                }
            }

            return PartialView("_LichTrinhEditPartial", fullList);
        }

        [HttpPost]
        public JsonResult SuaLichTrinhAjax(List<LichTrinhTour> model)
        {
            try
            {
                if (model == null || model.Count == 0)
                {
                    return Json(new { success = false, message = "Model null hoặc rỗng" });
                }

                foreach (var item in model)
                {
                    var existing = _contextDB.LichTrinhTours.Find(item.ID_LichTrinhTour);
                    if (existing != null)
                    {
                        existing.TieuDe = item.TieuDe;
                        existing.NoiDung = item.NoiDung;
                    }
                    else
                    {
                        _contextDB.LichTrinhTours.Add(new LichTrinhTour
                        {
                            ID_Tour = item.ID_Tour,
                            NgayThu = item.NgayThu,
                            TieuDe = item.TieuDe,
                            NoiDung = item.NoiDung
                        });
                    }
                }
                _contextDB.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }
        [HttpGet]
        public ActionResult ThemLichTrinh(int id)
        {
            var tour = _contextDB.Tours.Find(id);
            if (tour == null) return HttpNotFound();

            int soNgay = 1;
            var match = Regex.Match(tour.SoNgay ?? "", @"\d+");
            if (match.Success)
                soNgay = int.Parse(match.Value);

            var fullList = new List<LichTrinhTour>();
            for (int i = 1; i <= soNgay; i++)
            {
                fullList.Add(new LichTrinhTour
                {
                    ID_Tour = id,
                    NgayThu = i,
                    TieuDe = "",
                    NoiDung = ""
                });
            }
            return PartialView("_LichTrinhAddPartial", fullList);
        }
        [HttpPost]
        public JsonResult ThemLichTrinhAjax(List<LichTrinhTour> model)
        {
            try
            {
                if (model == null || model.Count == 0)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                foreach (var item in model)
                {
                    _contextDB.LichTrinhTours.Add(new LichTrinhTour
                    {
                        ID_Tour = item.ID_Tour,
                        NgayThu = item.NgayThu,
                        TieuDe = item.TieuDe,
                        NoiDung = item.NoiDung
                    });
                }
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

