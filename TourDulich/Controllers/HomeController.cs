using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.ModelView;

namespace TourDulich.Controllers
{
    public class HomeController : Controller
    {
        private ModelDB _contextDB = new ModelDB();
        public ActionResult Index()
        {
            var listTour = (from t in _contextDB.Tours
                            where t.DaXoa == false && t.TrangThaiHoatDong == true
                            join dg in _contextDB.DanhGias on t.ID_Tour equals dg.ID_Tour into danhGiaGroup
                            select new TourView()
                            {
                                ID_Tour = t.ID_Tour,
                                TenTour = t.TenTour,
                                HinhAnh = _contextDB.HinhAnhTours
                                            .Where(ha => ha.ID_Tour == t.ID_Tour && ha.HienThi == 1)
                                            .Select(ha => ha.HinhAnh)
                                            .FirstOrDefault(),
                                Gia = t.Gia,
                                SoNgay = t.SoNgay,
                                DiemKhoiHanh = t.DiemKhoiHanh,
                                SoSao = (int)Math.Round((danhGiaGroup.Average(dg => (double?)dg.SoSao) ?? 0)),
                                IsGiaTot = t.IsGiaTot,
                                IsUuDai = t.IsUuDai
                            }).ToList();

            var tourGiaCao = listTour.Where(t => t.IsGiaTot).ToList();
            var tourGiaTot = listTour.Where(t => t.IsUuDai).ToList();

            var listDiaDiem = _contextDB.DiaDiems
                        .Select(dd => new DiaDiemView
                        {
                            ID_DiaDiem = dd.ID_DiaDiem,
                            TenDiaDiem = dd.TenDiaDiem,
                            Hinh = dd.Hinh
                        })
                        .Distinct()
                        .Take(9)
                        .ToList();

            ViewBag.ListDiaDiem = listDiaDiem;
            ViewBag.TourGiaCao = tourGiaCao;
            ViewBag.TourGiaTot = tourGiaTot;

            return View(listTour); 
        }

        public ActionResult DetailsTour(int id)
        {
            var tourDetail = (from t in _contextDB.Tours
                              where t.ID_Tour == id && t.DaXoa == false && t.TrangThaiHoatDong == true
                              join dd in _contextDB.DiaDiems on t.ID_DiaDiem equals dd.ID_DiaDiem
                              join dg in _contextDB.DanhGias on t.ID_Tour equals dg.ID_Tour into danhGiaGroup
                              select new TourView()
                              {
                                  ID_Tour = t.ID_Tour,
                                  TenTour = t.TenTour,
                                  MoTa = t.MoTa,
                                  Gia = t.Gia,
                                  SoNgay = t.SoNgay,
                                  SoLuongToiDa = t.SoLuongToiDa,
                                  DiemKhoiHanh = t.DiemKhoiHanh,
                                  PhuongTien = t.PhuongTien,
                                  DiemDen = dd.TenDiaDiem,
                                  SoSao = (int)Math.Round((danhGiaGroup.Average(dg => (double?)dg.SoSao) ?? 0))
                              }).FirstOrDefault();

            if (tourDetail == null)
            {
                return HttpNotFound();
            }

            var bannerImage = _contextDB.HinhAnhTours
                                .Where(ha => ha.ID_Tour == id && ha.HienThi == 1)
                                .Select(ha => ha.HinhAnh)
                                .FirstOrDefault();

            var otherImages = _contextDB.HinhAnhTours
                                .Where(ha => ha.ID_Tour == id && ha.HienThi == 0)
                                .Select(ha => ha.HinhAnh)
                                .ToList();

            tourDetail.HinhAnh = bannerImage; 
            tourDetail.HinhAnhKhac = otherImages; 

            var lichTrinhList = _contextDB.LichTrinhTours
                                .Where(lt => lt.ID_Tour == id)
                                .OrderBy(lt => lt.NgayThu)
                                .Select(lt => new LichTrinhTourView
                                {
                                    ID_LichTrinhTour = lt.ID_LichTrinhTour,
                                    ID_Tour = lt.ID_Tour,
                                    NgayThu = lt.NgayThu,
                                    TieuDe = lt.TieuDe,
                                    NoiDung = lt.NoiDung
                                })
                                .ToList();

            tourDetail.LichTrinhTours = lichTrinhList;

            var danhGiaList = _contextDB.DanhGias
                            .Where(dg => dg.ID_Tour == id)
                            .OrderByDescending(dg => dg.NgayDanhGia)
                            .Select(dg => new DanhGiaView
                            {
                                HoTen = dg.HoTen,
                                NoiDung = dg.NoiDung,
                                SoSao = dg.SoSao,
                                NgayDanhGia = dg.NgayDanhGia
                            }).ToList();


            ViewBag.DanhGiaList = danhGiaList;

            int? userId = Session["ID_NguoiDung"] as int?;

            if (userId.HasValue)
            {
                bool daDanhGia = _contextDB.DanhGias.Any(d => d.ID_Tour == id && d.ID_NguoiDung == userId.Value);
                ViewBag.DaDanhGia = daDanhGia;
            }
            else
            {
                ViewBag.DaDanhGia = false;
            }

            return View(tourDetail);
        }



        [HttpPost]
        public ActionResult DatTourTam(int TourId, DateTime SelectedDate, int TicketQuantity)
        {
            var tour = (from t in _contextDB.Tours
                        join ha in _contextDB.HinhAnhTours on t.ID_Tour equals ha.ID_Tour
                        where t.ID_Tour == TourId && t.DaXoa == false && t.TrangThaiHoatDong == true
                        select new
                        {
                            t.ID_Tour,
                            t.TenTour,
                            t.Gia,
                            HinhAnh = ha.HinhAnh
                        }).FirstOrDefault();

            if (tour == null)
                return HttpNotFound();

            var tourTam = new TourDaDatTamThoi
            {
                TourId = tour.ID_Tour,
                TenTour = tour.TenTour,
                Gia = tour.Gia,
                HinhAnh = tour.HinhAnh,
                NgayDi = SelectedDate,
                SoLuong = TicketQuantity
            };

            var dsTourTam = Session["DanhSachTourTamThoi"] as List<TourDaDatTamThoi>;

            if (dsTourTam == null)
            {
                dsTourTam = new List<TourDaDatTamThoi>();
            }

            var existingTour = dsTourTam.FirstOrDefault(t => t.TourId == tourTam.TourId && t.NgayDi == tourTam.NgayDi);

            if (existingTour != null)
            {
                existingTour.SoLuong += tourTam.SoLuong;
            }
            else
            {
                dsTourTam.Add(tourTam);
            }

            Session["DanhSachTourTamThoi"] = dsTourTam;

            return RedirectToAction("XacNhanDatTour");
        }

        public ActionResult XacNhanDatTour()
        {
            var ds = Session["DanhSachTourTamThoi"] as List<TourDaDatTamThoi>;

            return View(ds ?? new List<TourDaDatTamThoi>());
        }

        [HttpPost]
        public ActionResult XoaTourTamThoi(int tourId)
        {
            var ds = Session["DanhSachTourTamThoi"] as List<TourDaDatTamThoi>;
            if (ds != null)
            {
                var tourXoa = ds.FirstOrDefault(t => t.TourId == tourId);
                if (tourXoa != null)
                {
                    ds.Remove(tourXoa);
                    Session["DanhSachTourTamThoi"] = ds; 
                }
            }

            return RedirectToAction("XacNhanDatTour");
        }
        public ActionResult ThanhToan()
        {
            
            var dsTourTam = Session["DanhSachTourTamThoi"] as List<TourDaDatTamThoi>;
            if (dsTourTam == null || dsTourTam.Count == 0)
            {
                TempData["Error"] = "Bạn chưa chọn tour nào để thanh toán.";
                return RedirectToAction("Index");
            }

           
            int? userId = Session["ID_NguoiDung"] as int?;
            if (!userId.HasValue)
            {
                TempData["Error"] = "Bạn phải đăng nhập để đặt tour.";
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            var user = _contextDB.NguoiDungs.Find(userId.Value);
            if (user == null)
            {
                TempData["Error"] = "Thông tin người dùng không hợp lệ.";
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            
            var model = new ThanhToanView
            {
                DanhSachTour = dsTourTam,
                UserInfo = new NguoiDung
                {
                    ID_NguoiDung = user.ID_NguoiDung,
                    HoTen = user.HoTen,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    DiaChi = user.DiaChi
                }
            };

            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThanhToan(ThanhToanView model)
        {
            if (!ModelState.IsValid)
            {
                
                model.DanhSachTour = Session["DanhSachTourTamThoi"] as List<TourDaDatTamThoi> ?? new List<TourDaDatTamThoi>();
                return View(model);
            }

            int? userId = Session["ID_NguoiDung"] as int?;
            if (!userId.HasValue)
            {
                TempData["Error"] = "Bạn phải đăng nhập để đặt tour.";
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            var dsTourTam = Session["DanhSachTourTamThoi"] as List<TourDaDatTamThoi>;
            if (dsTourTam == null || dsTourTam.Count == 0)
            {
                TempData["Error"] = "Bạn chưa chọn tour nào để thanh toán.";
                return RedirectToAction("Index");
            }

            try
            {
                
                decimal tongTien = dsTourTam.Sum(t => (t.Gia ?? 0) * t.SoLuong);

               
                var datTour = new DatTour
                {
                    ID_NguoiDung = userId.Value,
                    NgayDat = DateTime.Now,
                    TongTien = tongTien,
                    TrangThai = "Chờ xử lý",
                    
                };

                _contextDB.DatTours.Add(datTour);
                _contextDB.SaveChanges();

               
                foreach (var item in dsTourTam)
                {
                    var chiTiet = new ChiTietDatTour
                    {
                        ID_DatTour = datTour.ID_DatTour,
                        ID_Tour = item.TourId,
                        NgayKhoiHanh = item.NgayDi,
                        SoLuongNguoi = item.SoLuong,
                        Gia = item.Gia,
                        PhuongThucThanhToan = model.PhuongThucThanhToan 
                    };

                    _contextDB.ChiTietDatTours.Add(chiTiet);
                }

                _contextDB.SaveChanges();

                Session.Remove("DanhSachTourTamThoi");

                TempData["Success"] = "Đặt tour thành công! Chúng tôi sẽ liên hệ bạn sớm.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu đơn đặt tour: " + ex.Message);
                model.DanhSachTour = dsTourTam;
                return View(model);
            }
        }
        public ActionResult LichSuDatTour()
        {
            int? userId = Session["ID_NguoiDung"] as int?;
            if (!userId.HasValue)
            {
                TempData["Error"] = "Bạn chưa đăng nhập.";
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            var lichSu = (from dt in _contextDB.DatTours
                          where dt.ID_NguoiDung == userId.Value
                          orderby dt.NgayDat descending
                          select new LichSuDatTourView
                          {
                              ID_DatTour = dt.ID_DatTour,
                              NgayDat = dt.NgayDat,
                              TongTien = dt.TongTien,
                              TrangThai = dt.TrangThai,
                              ChiTietTours = (from ct in _contextDB.ChiTietDatTours
                                              join t in _contextDB.Tours on ct.ID_Tour equals t.ID_Tour
                                              where ct.ID_DatTour == dt.ID_DatTour
                                              select new ChiTietLichSuTourView
                                              {
                                                  TenTour = t.TenTour,
                                                  NgayKhoiHanh = ct.NgayKhoiHanh,
                                                  SoLuongNguoi = ct.SoLuongNguoi,
                                                  Gia = ct.Gia,
                                                  PhuongThucThanhToan = ct.PhuongThucThanhToan
                                              }).ToList()
                          }).ToList();

            return View(lichSu);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiDanhGia(DanhGia model)
        {

            if (Session["ID_NguoiDung"] == null)
            {
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            int userId = (int)Session["ID_NguoiDung"];

            bool daDanhGia = _contextDB.DanhGias.Any(dg => dg.ID_Tour == model.ID_Tour && dg.ID_NguoiDung == userId);

            if (daDanhGia)
            {
                ModelState.AddModelError("", "Bạn đã đánh giá tour này rồi.");
            }
            else if (ModelState.IsValid)
            {
                model.ID_NguoiDung = userId;

                string hoTen = Session["HoTen"] as string;

                if (string.IsNullOrEmpty(hoTen))
                {
                    var nguoiDung = _contextDB.NguoiDungs.Find(userId);
                    if (nguoiDung != null)
                    {
                        hoTen = nguoiDung.HoTen;
                    }
                }

                model.HoTen = hoTen ?? "Khách hàng"; 
                model.NgayDanhGia = DateTime.Now;

                _contextDB.DanhGias.Add(model);
                _contextDB.SaveChanges();

                return RedirectToAction("DetailsTour", new { id = model.ID_Tour });
            }


            return RedirectToAction("DetailsTour", new { id = model.ID_Tour });
        }
        

    }
}