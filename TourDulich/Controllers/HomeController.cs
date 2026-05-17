using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.ModelView;
using System.Data.Entity;
using System.Data;

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
        public ActionResult DatTourTam(int TourId, DateTime SelectedDate, int TicketQuantity,
                                       string LoaiDat = AppConstants.LoaiDat.KhachLe,
                                       string TruongDoan = "", string SdtTruongDoan = "", string GhiChuDoan = "")
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

            bool laDoan = LoaiDat == AppConstants.LoaiDat.Doan;

            // ── Validation riêng cho đoàn ────────────────────────────────────────
            if (laDoan && TicketQuantity < 10)
            {
                TempData["Error"] = "Đặt tour đoàn yêu cầu tối thiểu 10 người.";
                return RedirectToAction("DetailsTour", new { id = TourId });
            }
            if (laDoan && string.IsNullOrWhiteSpace(TruongDoan))
            {
                TempData["Error"] = "Vui lòng nhập họ tên trưởng đoàn.";
                return RedirectToAction("DetailsTour", new { id = TourId });
            }

            // ── Kiểm tra slot (chỉ khách lẻ) ────────────────────────────────────────
            LichKhoiHanh lich = null;
            int conLai = int.MaxValue;

            if (!laDoan)
            {
                lich = _contextDB.LichKhoiHanhs.FirstOrDefault(l =>
                    l.ID_Tour == TourId &&
                    l.NgayKhoiHanh == SelectedDate.Date &&
                    l.TrangThai == AppConstants.TrangThaiLichKhoiHanh.Mo);

                if (lich == null)
                {
                    TempData["Error"] = "Ngày khởi hành này không có lịch hoặc đã đóng. Vui lòng chọn ngày khác.";
                    return RedirectToAction("DetailsTour", new { id = TourId });
                }

                conLai = lich.SoLuongToiDa - lich.SoLuongDaDat;
                if (TicketQuantity > conLai)
                {
                    TempData["Error"] = $"Chỉ còn {conLai} chỗ cho ngày này. Vui lòng chọn số lượng phù hợp.";
                    return RedirectToAction("DetailsTour", new { id = TourId });
                }
            }
            else
            {
                // Đoàn: chỉ cần ngày tồn tại trong lịch
                lich = _contextDB.LichKhoiHanhs.FirstOrDefault(l =>
                    l.ID_Tour == TourId && l.NgayKhoiHanh == SelectedDate.Date);

                if (lich == null)
                {
                    TempData["Error"] = "Ngày khởi hành này không tồn tại trong lịch. Vui lòng chọn ngày khác.";
                    return RedirectToAction("DetailsTour", new { id = TourId });
                }
            }

            // ── Tính giá theo mùa ────────────────────────────────────────────
            decimal giaGoc = tour.Gia ?? 0;
            var muaApDung = _contextDB.MuaGias
                .Where(m => m.IsActive
                         && m.NgayBatDau <= SelectedDate
                         && m.NgayKetThuc >= SelectedDate)
                .OrderByDescending(m => m.HeSoGia)
                .FirstOrDefault();

            decimal giaThucTe = muaApDung != null
                ? Math.Round(giaGoc * muaApDung.HeSoGia, 0)
                : giaGoc;

            var tourTam = new TourDaDatTamThoi
            {
                TourId        = tour.ID_Tour,
                TenTour       = tour.TenTour,
                Gia           = giaGoc,
                GiaThucTe     = giaThucTe,
                TenMua        = muaApDung?.TenMua,
                HinhAnh       = tour.HinhAnh,
                NgayDi        = SelectedDate,
                SoLuong       = TicketQuantity,
                LoaiDat       = laDoan ? AppConstants.LoaiDat.Doan : AppConstants.LoaiDat.KhachLe,
                TruongDoan    = laDoan ? TruongDoan?.Trim() : null,
                SdtTruongDoan = laDoan ? SdtTruongDoan?.Trim() : null,
                GhiChuDoan    = laDoan ? GhiChuDoan?.Trim() : null
            };

            var dsTourTam = Session["DanhSachTourTamThoi"] as List<TourDaDatTamThoi>
                            ?? new List<TourDaDatTamThoi>();

            var existingTour = dsTourTam.FirstOrDefault(t => t.TourId == tourTam.TourId && t.NgayDi == tourTam.NgayDi);

            if (existingTour != null)
            {
                if (!laDoan)
                {
                    int tongMoi = existingTour.SoLuong + tourTam.SoLuong;
                    if (tongMoi > conLai)
                    {
                        TempData["Error"] = $"Chỉ còn {conLai} chỗ cho ngày này (bạn đã thêm {existingTour.SoLuong} trước đó).";
                        return RedirectToAction("DetailsTour", new { id = TourId });
                    }
                }
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
                PhuongThucThanhToan = AppConstants.ThanhToan.ChuyenKhoan,
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
                decimal tongTien = dsTourTam.Sum(t => (t.GiaThucTe ?? t.Gia ?? 0) * t.SoLuong);
                bool laDoan = dsTourTam.Any(t => t.LaDoan);
                var itemDoan = dsTourTam.FirstOrDefault(t => t.LaDoan);

                using (var transaction = _contextDB.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    if (!laDoan)
                    {
                        var tongTheoLich = dsTourTam
                            .GroupBy(t => new { t.TourId, NgayDi = t.NgayDi.Date })
                            .Select(g => new { g.Key.TourId, g.Key.NgayDi, SoLuong = g.Sum(x => x.SoLuong) })
                            .ToList();

                        foreach (var item in tongTheoLich)
                        {
                            var lichUpdate = _contextDB.LichKhoiHanhs.FirstOrDefault(l =>
                                l.ID_Tour == item.TourId &&
                                l.NgayKhoiHanh == item.NgayDi &&
                                l.TrangThai == AppConstants.TrangThaiLichKhoiHanh.Mo);

                            if (lichUpdate == null)
                                throw new InvalidOperationException("Ngày khởi hành không còn mở. Vui lòng chọn ngày khác.");

                            var conLai = lichUpdate.SoLuongToiDa - lichUpdate.SoLuongDaDat;
                            if (item.SoLuong > conLai)
                                throw new InvalidOperationException($"Chỉ còn {conLai} chỗ cho ngày {item.NgayDi:dd/MM/yyyy}.");

                            lichUpdate.SoLuongDaDat += item.SoLuong;
                            if (lichUpdate.SoLuongDaDat >= lichUpdate.SoLuongToiDa)
                                lichUpdate.TrangThai = AppConstants.TrangThaiLichKhoiHanh.HetCho;
                        }
                    }

                    var datTour = new DatTour
                    {
                        ID_NguoiDung  = userId.Value,
                        NgayDat       = DateTime.Now,
                        TongTien      = tongTien,
                        TrangThai     = laDoan ? AppConstants.TrangThaiDatTour.YeuCauDoanChoXacNhan : AppConstants.TrangThaiDatTour.ChoXuLy,
                        LoaiDat       = laDoan ? AppConstants.LoaiDat.Doan : AppConstants.LoaiDat.KhachLe,
                        TruongDoan    = itemDoan?.TruongDoan,
                        SdtTruongDoan = itemDoan?.SdtTruongDoan,
                        GhiChuDoan    = itemDoan?.GhiChuDoan,
                    };

                    _contextDB.DatTours.Add(datTour);
                    _contextDB.SaveChanges();

                    foreach (var item in dsTourTam)
                    {
                        var chiTiet = new ChiTietDatTour
                        {
                            ID_DatTour          = datTour.ID_DatTour,
                            ID_Tour             = item.TourId,
                            NgayKhoiHanh        = item.NgayDi,
                            SoLuongNguoi        = item.SoLuong,
                            Gia                 = item.GiaThucTe ?? item.Gia,
                            PhuongThucThanhToan = AppConstants.ThanhToan.ChuyenKhoan
                        };
                        _contextDB.ChiTietDatTours.Add(chiTiet);
                    }

                    _contextDB.SaveChanges();
                    transaction.Commit();
                }

                Session.Remove("DanhSachTourTamThoi");
                TempData["Success"] = laDoan
                    ? "Đặt tour đoàn thành công! Nhân viên sẽ liên hệ trưởng đoàn trong 24 giờ."
                    : "Đặt tour thành công! Chúng tôi sẽ liên hệ bạn sớm.";
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

            var datTours = _contextDB.DatTours
                .Include("ChiTietDatTours.Tour")
                .Include("YeuCauHuys")
                .Where(dt => dt.ID_NguoiDung == userId.Value)
                .OrderByDescending(dt => dt.NgayDat)
                .ToList();

            var lichSu = datTours.Select(dt => new LichSuDatTourView
            {
                ID_DatTour = dt.ID_DatTour,
                NgayDat = dt.NgayDat,
                TongTien = dt.TongTien,
                TrangThai = dt.TrangThai,
                CoYeuCauHuy = dt.CoYeuCauHuy,
                TienHoan = dt.YeuCauHuys.Where(y => y.TrangThai == AppConstants.TrangThaiYeuCauHuy.ChapThuan).Select(y => y.TienHoan).FirstOrDefault(),
                LoaiDat = dt.LoaiDat,
                TruongDoan = dt.TruongDoan,
                SdtTruongDoan = dt.SdtTruongDoan,
                GhiChuDoan = dt.GhiChuDoan,
                ChiTietTours = dt.ChiTietDatTours.Select(ct => new ChiTietLichSuTourView
                {
                    TenTour = ct.Tour.TenTour,
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

        // API: lấy danh sách ngày còn slot của một tour (dùng cho date-picker phía user)
        public JsonResult GetAvailableDates(int tourId)
        {
            var activeSeasons = _contextDB.MuaGias.Where(m => m.IsActive).ToList();

            var dates = _contextDB.LichKhoiHanhs
                .Where(l => l.ID_Tour == tourId
                         && l.TrangThai == AppConstants.TrangThaiLichKhoiHanh.Mo
                         && l.NgayKhoiHanh >= DateTime.Today
                         && l.SoLuongDaDat < l.SoLuongToiDa)
                .OrderBy(l => l.NgayKhoiHanh)
                .Select(l => new
                {
                    ngay = l.NgayKhoiHanh,
                    conLai = l.SoLuongToiDa - l.SoLuongDaDat
                })
                .ToList()
                .Select(l => 
                {
                    var muaApDung = activeSeasons
                        .Where(m => m.NgayBatDau <= l.ngay && m.NgayKetThuc >= l.ngay)
                        .OrderByDescending(m => m.HeSoGia)
                        .FirstOrDefault();

                    return new
                    {
                        ngay = l.ngay.ToString("yyyy-MM-dd"),
                        conLai = l.conLai,
                        saptHet = l.conLai <= 5,
                        heSoGia = muaApDung != null ? muaApDung.HeSoGia : 1.0m,
                        tenMua = muaApDung != null ? muaApDung.TenMua : ""
                    };
                });

            return Json(dates, JsonRequestBehavior.AllowGet);
        }

        // API: Lấy thông tin hủy tour cho modal
        [HttpGet]
        public JsonResult GetThongTinHuy(int id)
        {
            var userId = Session["ID_NguoiDung"] as int?;
            if (!userId.HasValue) return Json(new { success = false, message = "Vui lòng đăng nhập" }, JsonRequestBehavior.AllowGet);

            var datTour = _contextDB.DatTours.Include("ChiTietDatTours.Tour")
                                    .FirstOrDefault(d => d.ID_DatTour == id && d.ID_NguoiDung == userId.Value);
            
            if (datTour == null) return Json(new { success = false, message = "Không tìm thấy đơn đặt tour" }, JsonRequestBehavior.AllowGet);

            if (datTour.TrangThai == AppConstants.TrangThaiDatTour.DaHuy || datTour.CoYeuCauHuy)
            {
                return Json(new { success = false, message = "Đơn này đã được hủy hoặc đang chờ xử lý" }, JsonRequestBehavior.AllowGet);
            }

            var earliestKhoiHanh = datTour.ChiTietDatTours
                                          .Where(ct => ct.NgayKhoiHanh.HasValue)
                                          .Min(ct => ct.NgayKhoiHanh);

            if (!earliestKhoiHanh.HasValue)
                return Json(new { success = false, message = "Không xác định được ngày khởi hành" }, JsonRequestBehavior.AllowGet);

            int soNgayConLai = (earliestKhoiHanh.Value.Date - DateTime.Today).Days;

            // Kiểm tra điều kiện cơ bản: còn > 7 ngày trước khởi hành
            if (soNgayConLai <= 7)
            {
                return Json(new { success = false, message = "Không đủ điều kiện hủy tour (phải hủy trước 7 ngày)" }, JsonRequestBehavior.AllowGet);
            }

            // Lấy chính sách hủy áp dụng
            var policy = _contextDB.ChinhSachHuys
                                   .Where(c => soNgayConLai >= c.SoNgayTuHuy)
                                   .OrderByDescending(c => c.SoNgayTuHuy)
                                   .FirstOrDefault();

            int phanTram = policy != null ? policy.PhanTramHoan : 0;
            decimal tongTien = datTour.TongTien ?? 0;
            decimal tienHoan = (phanTram / 100m) * tongTien;

            return Json(new
            {
                success = true,
                soNgayConLai = soNgayConLai,
                phanTram = phanTram,
                tienHoan = tienHoan,
                tongTien = tongTien,
                chinhSachText = policy != null ? $"Hủy trước {policy.SoNgayTuHuy} ngày" : "Không có chính sách hoàn tiền"
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiYeuCauHuy(int ID_DatTour, string LyDo)
        {
            var userId = Session["ID_NguoiDung"] as int?;
            if (!userId.HasValue) return RedirectToAction("DangNhap", "NguoiDung");

            var datTour = _contextDB.DatTours.Include("ChiTietDatTours.Tour")
                                    .FirstOrDefault(d => d.ID_DatTour == ID_DatTour && d.ID_NguoiDung == userId.Value);

            if (datTour == null || datTour.TrangThai == AppConstants.TrangThaiDatTour.DaHuy || datTour.CoYeuCauHuy)
            {
                TempData["Error"] = "Đơn không hợp lệ hoặc đã được xử lý.";
                return RedirectToAction("LichSuDatTour");
            }

            var earliestKhoiHanh = datTour.ChiTietDatTours
                                          .Where(ct => ct.NgayKhoiHanh.HasValue)
                                          .Min(ct => ct.NgayKhoiHanh);

            if (!earliestKhoiHanh.HasValue)
            {
                TempData["Error"] = "Lỗi dữ liệu ngày khởi hành.";
                return RedirectToAction("LichSuDatTour");
            }

            int soNgayConLai = (earliestKhoiHanh.Value.Date - DateTime.Today).Days;
            if (soNgayConLai <= 7)
            {
                TempData["Error"] = "Bạn không đủ điều kiện hủy tour này (phải hủy trước 7 ngày).";
                return RedirectToAction("LichSuDatTour");
            }

            var policy = _contextDB.ChinhSachHuys
                                   .Where(c => soNgayConLai >= c.SoNgayTuHuy)
                                   .OrderByDescending(c => c.SoNgayTuHuy)
                                   .FirstOrDefault();

            int phanTram = policy != null ? policy.PhanTramHoan : 0;
            decimal tienHoan = (phanTram / 100m) * (datTour.TongTien ?? 0);

            // Gửi yêu cầu hủy chờ admin duyệt
            var yeuCau = new YeuCauHuy
            {
                ID_DatTour = datTour.ID_DatTour,
                LyDo = LyDo,
                NgayGui = DateTime.Now,
                TrangThai = AppConstants.TrangThaiYeuCauHuy.ChoXuLy,
                PhanTramHoan = phanTram,
                TienHoan = tienHoan,
                NgayXuLy = null,
                GhiChuAdmin = null
            };

            _contextDB.YeuCauHuys.Add(yeuCau);

            // Cập nhật đơn đặt tour (chỉ đánh dấu là có yêu cầu hủy, không đổi trạng thái)
            datTour.CoYeuCauHuy = true;

            _contextDB.SaveChanges();

            TempData["Success"] = $"Đã gửi yêu cầu hủy tour! Yêu cầu của bạn đang được chờ xử lý. Bạn sẽ được hoàn {phanTram}% ({tienHoan:N0} VNĐ) sau khi admin xác nhận.";
            return RedirectToAction("LichSuDatTour");
        }
    }
}
