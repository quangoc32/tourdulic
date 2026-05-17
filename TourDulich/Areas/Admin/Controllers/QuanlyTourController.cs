using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.ModelView;
using TourDulich.Areas.Admin.Filters;

namespace TourDulich.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class QuanlyTourController : Controller
    {

        private ModelDB _contextDB = new ModelDB();
        private static readonly HashSet<string> AllowedImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };
        private const int MaxImageBytes = 5 * 1024 * 1024;
        public ActionResult Index(string searchString)
        {
            var listTourQuery = (from t in _contextDB.Tours
                            where t.DaXoa == false
                            join dm in _contextDB.DanhMucs on t.ID_DanhMuc equals dm.ID_DanhMuc
                            join dd in _contextDB.DiaDiems on t.ID_DiaDiem equals dd.ID_DiaDiem
                            let ha = _contextDB.HinhAnhTours
                                        .Where(h => h.ID_Tour == t.ID_Tour && h.HienThi == 1)
                                        .Select(h => h.HinhAnh)
                                        .FirstOrDefault()
                            select new TourView()
                            {
                                ID_Tour = t.ID_Tour,
                                TenTour = t.TenTour,
                                MoTa = t.MoTa,
                                Gia = t.Gia,
                                HinhAnh = ha,
                                DiemKhoiHanh = t.DiemKhoiHanh,
                                SoNgay = t.SoNgay,
                                SoLuongToiDa = t.SoLuongToiDa,
                                PhuongTien = t.PhuongTien,
                                TenDanhMuc = dm.TenDanhMuc,
                                DiemDen = dd.TenDiaDiem,
                                TrangThaiHoatDong = t.TrangThaiHoatDong,
                                HienThi = 1
                            });

            if (!string.IsNullOrEmpty(searchString))
            {
                listTourQuery = listTourQuery.Where(t => t.TenTour.Contains(searchString));
            }

            ViewBag.SearchString = searchString;

            return View(listTourQuery.ToList());
        }
        [HttpGet]
        public ActionResult ThemTour()
        {
            ViewBag.ID_DanhMuc = new SelectList(_contextDB.DanhMucs.OrderBy(x => x.TenDanhMuc).ToList(), "ID_DanhMuc", "TenDanhMuc");
            ViewBag.ID_DiaDiem = new SelectList(_contextDB.DiaDiems.OrderBy(x => x.TenDiaDiem).ToList(), "ID_DiaDiem", "TenDiaDiem");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemTour(TourView formData, HttpPostedFileBase AnhDaiDien, IEnumerable<HttpPostedFileBase> AnhPhu)
        {
            if (ModelState.IsValid)
            {
                ValidateTourImage(AnhDaiDien, "AnhDaiDien");
                ValidateTourImages(AnhPhu, "AnhPhu");
                if (!ModelState.IsValid)
                {
                    ViewBag.ID_DanhMuc = new SelectList(_contextDB.DanhMucs.OrderBy(x => x.TenDanhMuc).ToList(), "ID_DanhMuc", "TenDanhMuc", formData.ID_DanhMuc);
                    ViewBag.ID_DiaDiem = new SelectList(_contextDB.DiaDiems.OrderBy(x => x.TenDiaDiem).ToList(), "ID_DiaDiem", "TenDiaDiem", formData.ID_DiaDiem);
                    return View(formData);
                }

                var tour = new Tour
                {
                    TenTour = formData.TenTour,
                    MoTa = formData.MoTa,
                    Gia = formData.Gia,
                    SoNgay = formData.SoNgay,
                    DiemKhoiHanh = formData.DiemKhoiHanh,
                    PhuongTien = formData.PhuongTien,
                    SoLuongToiDa = formData.SoLuongToiDa,
                    ID_DanhMuc = formData.ID_DanhMuc,
                    ID_DiaDiem = formData.ID_DiaDiem,
                    TrangThaiHoatDong = formData.TrangThaiHoatDong,
                    IsGiaTot = formData.IsGiaTot,
                    IsUuDai = formData.IsUuDai,
                    DaXoa = false,
                    NgayTao = DateTime.Now
                };

                _contextDB.Tours.Add(tour);
                _contextDB.SaveChanges();

                if (AnhDaiDien != null && AnhDaiDien.ContentLength > 0)
                {
                    var fileName = SaveTourImage(AnhDaiDien);

                    var hinhAnhTour = new HinhAnhTour
                    {
                        ID_Tour = tour.ID_Tour,
                        HinhAnh = fileName,
                        HienThi = 1  
                    };
                    _contextDB.HinhAnhTours.Add(hinhAnhTour);
                }

                if (AnhPhu != null && AnhPhu.Any())
                {
                    foreach (var file in AnhPhu)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            var fileName = SaveTourImage(file);

                            var hinhAnhTour = new HinhAnhTour
                            {
                                ID_Tour = tour.ID_Tour,
                                HinhAnh = fileName,
                                HienThi = 0 
                            };
                            _contextDB.HinhAnhTours.Add(hinhAnhTour);
                        }
                    }
                }

                _contextDB.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.ID_DanhMuc = new SelectList(_contextDB.DanhMucs.OrderBy(x => x.TenDanhMuc).ToList(), "ID_DanhMuc", "TenDanhMuc", formData.ID_DanhMuc);
            ViewBag.ID_DiaDiem = new SelectList(_contextDB.DiaDiems.OrderBy(x => x.TenDiaDiem).ToList(), "ID_DiaDiem", "TenDiaDiem", formData.ID_DiaDiem);
            return View(formData);
        }


        public ActionResult SuaTour(int id)
        {
            var tour = _contextDB.Tours.Find(id);
            if (tour == null)
                return HttpNotFound();

            var tourView = new TourView
            {
                ID_Tour = tour.ID_Tour,
                TenTour = tour.TenTour,
                MoTa = tour.MoTa,
                Gia = tour.Gia,
                SoNgay = tour.SoNgay,
                DiemKhoiHanh = tour.DiemKhoiHanh,
                PhuongTien = tour.PhuongTien,
                SoLuongToiDa = tour.SoLuongToiDa,
                ID_DanhMuc = tour.ID_DanhMuc,
                ID_DiaDiem = tour.ID_DiaDiem,
                TrangThaiHoatDong = tour.TrangThaiHoatDong,
                IsGiaTot = tour.IsGiaTot,
                IsUuDai = tour.IsUuDai,
                NgayTao = tour.NgayTao,
                HinhAnh = _contextDB.HinhAnhTours
                            .Where(x => x.ID_Tour == id && x.HienThi == 1)
                            .Select(x => x.HinhAnh)
                            .FirstOrDefault(),
                DanhSachHinhAnhTour = _contextDB.HinhAnhTours
                        .Where(x => x.ID_Tour == id && x.HienThi == 0)
                        .ToList(),

                DanhMucSelectList = new SelectList(_contextDB.DanhMucs.OrderBy(x => x.TenDanhMuc), "ID_DanhMuc", "TenDanhMuc", tour.ID_DanhMuc),
                DiaDiemSelectList = new SelectList(_contextDB.DiaDiems.OrderBy(x => x.TenDiaDiem), "ID_DiaDiem", "TenDiaDiem", tour.ID_DiaDiem),
            };

            return View(tourView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaTour(TourView formData, HttpPostedFileBase HinhAnhFile, IEnumerable<HttpPostedFileBase> HinhAnhFiles)
        {
            if (ModelState.IsValid)
            {
                ValidateTourImage(HinhAnhFile, "HinhAnhFile");
                ValidateTourImages(HinhAnhFiles, "HinhAnhFiles");
                if (!ModelState.IsValid)
                {
                    ViewBag.ID_DanhMuc = new SelectList(_contextDB.DanhMucs.OrderBy(x => x.TenDanhMuc), "ID_DanhMuc", "TenDanhMuc", formData.ID_DanhMuc);
                    ViewBag.ID_DiaDiem = new SelectList(_contextDB.DiaDiems.OrderBy(x => x.TenDiaDiem), "ID_DiaDiem", "TenDiaDiem", formData.ID_DiaDiem);
                    formData.DanhSachHinhAnhTour = _contextDB.HinhAnhTours.Where(x => x.ID_Tour == formData.ID_Tour).ToList();
                    return View(formData);
                }

                var tour = _contextDB.Tours.Find(formData.ID_Tour);
                if (tour == null)
                    return HttpNotFound();

                tour.TenTour = formData.TenTour;
                tour.MoTa = formData.MoTa;
                tour.Gia = formData.Gia;
                tour.SoNgay = formData.SoNgay;
                tour.DiemKhoiHanh = formData.DiemKhoiHanh;
                tour.PhuongTien = formData.PhuongTien;
                tour.SoLuongToiDa = formData.SoLuongToiDa;
                tour.ID_DanhMuc = formData.ID_DanhMuc;
                tour.ID_DiaDiem = formData.ID_DiaDiem;
                tour.TrangThaiHoatDong = formData.TrangThaiHoatDong;
                tour.IsGiaTot = formData.IsGiaTot;
                tour.IsUuDai = formData.IsUuDai;

                if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
                {
                    var oldMainImage = _contextDB.HinhAnhTours.FirstOrDefault(x => x.ID_Tour == tour.ID_Tour && x.HienThi == 1);
                    if (oldMainImage != null)
                    {
                        string oldPath = Server.MapPath("~/Images/ImagesTour/" + oldMainImage.HinhAnh);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);

                        _contextDB.HinhAnhTours.Remove(oldMainImage);
                        _contextDB.SaveChanges();
                    }

                    var fileName = SaveTourImage(HinhAnhFile);

                    var newMainImage = new HinhAnhTour
                    {
                        ID_Tour = tour.ID_Tour,
                        HinhAnh = fileName,
                        HienThi = 1
                    };
                    _contextDB.HinhAnhTours.Add(newMainImage);
                    _contextDB.SaveChanges();
                }

                if (HinhAnhFiles != null && HinhAnhFiles.Any(f => f != null && f.ContentLength > 0))
                {
                    var oldSubImages = _contextDB.HinhAnhTours.Where(x => x.ID_Tour == tour.ID_Tour && x.HienThi == 0).ToList();

                    foreach (var img in oldSubImages)
                    {
                        string fullPath = Server.MapPath("~/Images/ImagesTour/" + img.HinhAnh);
                        if (System.IO.File.Exists(fullPath))
                            System.IO.File.Delete(fullPath);
                    }
                    _contextDB.HinhAnhTours.RemoveRange(oldSubImages);
                    _contextDB.SaveChanges();

                    foreach (var file in HinhAnhFiles)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            var fileName = SaveTourImage(file);

                            var hinhAnhTour = new HinhAnhTour
                            {
                                ID_Tour = tour.ID_Tour,
                                HinhAnh = fileName,
                                HienThi = 0
                            };
                            _contextDB.HinhAnhTours.Add(hinhAnhTour);
                        }
                    }
                    _contextDB.SaveChanges();
                }

                _contextDB.Entry(tour).State = System.Data.Entity.EntityState.Modified;
                _contextDB.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.ID_DanhMuc = new SelectList(_contextDB.DanhMucs.OrderBy(x => x.TenDanhMuc), "ID_DanhMuc", "TenDanhMuc", formData.ID_DanhMuc);
            ViewBag.ID_DiaDiem = new SelectList(_contextDB.DiaDiems.OrderBy(x => x.TenDiaDiem), "ID_DiaDiem", "TenDiaDiem", formData.ID_DiaDiem);
            formData.DanhSachHinhAnhTour = _contextDB.HinhAnhTours.Where(x => x.ID_Tour == formData.ID_Tour).ToList();
            return View(formData);
        }
        [HttpPost]
        public ActionResult XoaTour(int id)
        {
            var tour = _contextDB.Tours.Find(id);
            if (tour == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Soft Delete: Chuyển cờ Đã Xóa thay vì xóa hẳn khỏi CSDL
                tour.DaXoa = true;
                _contextDB.Entry(tour).State = System.Data.Entity.EntityState.Modified;
                _contextDB.SaveChanges();

                TempData["Success"] = "Xóa tour thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa tour: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private string SaveTourImage(HttpPostedFileBase file)
        {
            var extension = Path.GetExtension(file.FileName);
            var fileName = Guid.NewGuid().ToString("N") + extension.ToLowerInvariant();
            var uploadDir = Server.MapPath("~/Images/ImagesTour/");
            Directory.CreateDirectory(uploadDir);
            file.SaveAs(Path.Combine(uploadDir, fileName));

            return fileName;
        }

        private void ValidateTourImages(IEnumerable<HttpPostedFileBase> files, string key)
        {
            if (files == null) return;

            foreach (var file in files)
            {
                ValidateTourImage(file, key);
            }
        }

        private void ValidateTourImage(HttpPostedFileBase file, string key)
        {
            if (file == null || file.ContentLength <= 0) return;

            if (file.ContentLength > MaxImageBytes)
            {
                ModelState.AddModelError(key, "Ảnh vượt quá dung lượng cho phép 5MB.");
                return;
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError(key, "Chỉ cho phép upload ảnh .jpg, .jpeg, .png hoặc .webp.");
            }
        }

    }
}
