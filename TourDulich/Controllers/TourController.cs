using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.ModelView;

namespace TourDulich.Controllers
{
    public class TourController : Controller
    {
        private ModelDB _contextDB = new ModelDB();
        public ActionResult Index(string searchString, int? danhMucId, string diemKhoiHanh, string diemDen, int? giaLoai, string sortOrder, int page = 1)
        {
            int pageSize = 10;

            var listTourQuery = from t in _contextDB.Tours
                                where t.DaXoa == false && t.TrangThaiHoatDong == true
                                join dm in _contextDB.DanhMucs on t.ID_DanhMuc equals dm.ID_DanhMuc
                                join dg in _contextDB.DanhGias on t.ID_Tour equals dg.ID_Tour into danhGiaGroup
                                join dd in _contextDB.DiaDiems on t.ID_DiaDiem equals dd.ID_DiaDiem
                                select new
                                {
                                    Tour = t,
                                    TenDanhMuc = dm.TenDanhMuc,
                                    DiemDen = dd.TenDiaDiem,
                                    SoSao = (int)Math.Round((danhGiaGroup.Average(dg => (double?)dg.SoSao) ?? 0))
                                };

            if (!string.IsNullOrEmpty(searchString))
                listTourQuery = listTourQuery.Where(x => x.Tour.TenTour.Contains(searchString));

            if (danhMucId.HasValue)
                listTourQuery = listTourQuery.Where(x => x.Tour.ID_DanhMuc == danhMucId);

            if (!string.IsNullOrEmpty(diemKhoiHanh))
                listTourQuery = listTourQuery.Where(x => x.Tour.DiemKhoiHanh == diemKhoiHanh);

            if (!string.IsNullOrEmpty(diemDen))
                listTourQuery = listTourQuery.Where(x => x.DiemDen == diemDen);

            if (giaLoai.HasValue)
            {
                if (giaLoai == 1)
                    listTourQuery = listTourQuery.Where(x => x.Tour.Gia < 10000000);
                else if (giaLoai == 2)
                    listTourQuery = listTourQuery.Where(x => x.Tour.Gia >= 10000000);
            }

            var listTour = listTourQuery.Select(x => new TourView()
            {
                ID_Tour = x.Tour.ID_Tour,
                TenTour = x.Tour.TenTour,
                HinhAnh = _contextDB.HinhAnhTours
                            .Where(ha => ha.ID_Tour == x.Tour.ID_Tour && ha.HienThi == 1)
                            .Select(ha => ha.HinhAnh)
                            .FirstOrDefault(),
                Gia = x.Tour.Gia,
                SoNgay = x.Tour.SoNgay,
                DiemKhoiHanh = x.Tour.DiemKhoiHanh,
                DiemDen = x.DiemDen,
                TenDanhMuc = x.TenDanhMuc,
                SoSao = x.SoSao
            }).ToList();

            switch (sortOrder)
            {
                case "price_asc":
                    listTour = listTour.OrderBy(x => x.Gia).ToList();
                    break;
                case "price_desc":
                    listTour = listTour.OrderByDescending(x => x.Gia).ToList();
                    break;
                case "star_desc":
                    listTour = listTour.OrderByDescending(x => x.SoSao).ToList();
                    break;
            }

            int totalTour = listTour.Count();
            int totalPage = (int)Math.Ceiling((double)totalTour / pageSize);

            var toursToShow = listTour.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPage = totalPage;
            ViewBag.CurrentPage = page;
            ViewBag.SearchString = searchString;

            ViewBag.DanhMuc = new SelectList(_contextDB.DanhMucs, "ID_DanhMuc", "TenDanhMuc", danhMucId);
            ViewBag.DiemKhoiHanh = new SelectList(_contextDB.Tours.Where(t => t.DaXoa == false && t.TrangThaiHoatDong == true).Select(t => t.DiemKhoiHanh).Distinct(), diemKhoiHanh);
            ViewBag.DiemDen = new SelectList(_contextDB.DiaDiems.Select(d => d.TenDiaDiem).Distinct(), diemDen);

            return View(toursToShow);
        }
    }
}