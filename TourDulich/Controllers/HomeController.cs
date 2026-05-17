using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TourDulich.Models;
using TourDulich.ModelView;
using System.Data.Entity;
using System.Data;
using TourDulich.Services;

namespace TourDulich.Controllers
{
    public class HomeController : Controller
    {
        private ModelDB _contextDB = new ModelDB();

        private LienHeEmailService AdminEmailService
        {
            get { return new LienHeEmailService(new AdminEmailSettingStore(Server.MapPath("~/App_Data/admin-email-settings.json"))); }
        }
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
            var diemDonStore = new DiemDonTourStore(Server.MapPath("~/App_Data/diemdon-tour.json"));
            ViewBag.DiemDonTours = diemDonStore.GetByTour(id).Where(x => x.HienThi).ToList();

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
                                       string TruongDoan = "", string SdtTruongDoan = "", string GhiChuDoan = "",
                                       string LoaiDiemDon = "CoDinh", int? DiemDonId = null,
                                       string TinhThanhDon = "", string DiaChiDon = "", string GhiChuDiemDon = "")
        {
            var tour = (from t in _contextDB.Tours
                        join ha in _contextDB.HinhAnhTours on t.ID_Tour equals ha.ID_Tour
                        where t.ID_Tour == TourId && t.DaXoa == false && t.TrangThaiHoatDong == true
                        select new
                        {
                            t.ID_Tour,
                            t.TenTour,
                            t.Gia,
                            t.DiemKhoiHanh,
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

            string diemDon = "Điểm khởi hành chính: " + tour.DiemKhoiHanh;
            string diaChiDon = null;
            string tinhThanhDon = null;
            decimal phuThuDiemDon = 0;
            bool canXacNhanDiemDon = false;

            var diemDonStore = new DiemDonTourStore(Server.MapPath("~/App_Data/diemdon-tour.json"));
            if (LoaiDiemDon == "YeuCauKhac")
            {
                if (string.IsNullOrWhiteSpace(TinhThanhDon) || string.IsNullOrWhiteSpace(DiaChiDon))
                {
                    TempData["Error"] = "Vui lòng nhập tỉnh/thành và địa chỉ điểm đón mong muốn.";
                    return RedirectToAction("DetailsTour", new { id = TourId });
                }

                diemDon = "Yêu cầu điểm đón khác";
                diaChiDon = DiaChiDon.Trim();
                tinhThanhDon = TinhThanhDon.Trim();
                canXacNhanDiemDon = true;
            }
            else if (DiemDonId.HasValue)
            {
                var selectedPickup = diemDonStore.Find(DiemDonId.Value);
                if (selectedPickup == null || selectedPickup.ID_Tour != TourId || !selectedPickup.HienThi)
                {
                    TempData["Error"] = "Điểm đón không hợp lệ. Vui lòng chọn lại.";
                    return RedirectToAction("DetailsTour", new { id = TourId });
                }

                LoaiDiemDon = selectedPickup.LaTuTuc ? "TuTuc" : "CoDinh";
                diemDon = selectedPickup.TenDiemDon;
                diaChiDon = selectedPickup.DiaChi;
                tinhThanhDon = selectedPickup.TinhThanh;
                phuThuDiemDon = selectedPickup.PhuThu;
            }
            else
            {
                diemDon = "Điểm khởi hành chính: " + tour.DiemKhoiHanh;
            }

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
                GhiChuDoan    = laDoan ? GhiChuDoan?.Trim() : null,
                LoaiDiemDon = LoaiDiemDon,
                DiemDon = diemDon,
                DiaChiDon = diaChiDon,
                TinhThanhDon = tinhThanhDon,
                PhuThuDiemDon = phuThuDiemDon,
                GhiChuDiemDon = GhiChuDiemDon?.Trim(),
                CanXacNhanDiemDon = canXacNhanDiemDon
            };

            var dsTourTam = Session["DanhSachTourTamThoi"] as List<TourDaDatTamThoi>
                            ?? new List<TourDaDatTamThoi>();

            var existingTour = dsTourTam.FirstOrDefault(t =>
                t.TourId == tourTam.TourId &&
                t.NgayDi == tourTam.NgayDi &&
                t.LoaiDiemDon == tourTam.LoaiDiemDon &&
                t.DiemDon == tourTam.DiemDon &&
                t.DiaChiDon == tourTam.DiaChiDon);

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
                existingTour.LoaiDiemDon = tourTam.LoaiDiemDon;
                existingTour.DiemDon = tourTam.DiemDon;
                existingTour.DiaChiDon = tourTam.DiaChiDon;
                existingTour.TinhThanhDon = tourTam.TinhThanhDon;
                existingTour.PhuThuDiemDon = tourTam.PhuThuDiemDon;
                existingTour.GhiChuDiemDon = tourTam.GhiChuDiemDon;
                existingTour.CanXacNhanDiemDon = tourTam.CanXacNhanDiemDon;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiYeuCauXacNhanDiemDon()
        {
            var dsTourTam = Session["DanhSachTourTamThoi"] as List<TourDaDatTamThoi>;
            if (dsTourTam == null || dsTourTam.Count == 0)
            {
                TempData["Error"] = "Bạn chưa chọn tour nào.";
                return RedirectToAction("Index");
            }

            if (!dsTourTam.Any(t => t.CanXacNhanDiemDon))
            {
                return RedirectToAction("ThanhToan");
            }

            int? userId = Session["ID_NguoiDung"] as int?;
            if (!userId.HasValue)
            {
                TempData["Error"] = "Bạn phải đăng nhập để gửi yêu cầu xác nhận điểm đón.";
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            try
            {
                decimal tongTienTamTinh = 0;
                bool laDoan = dsTourTam.Any(t => t.LaDoan);
                var itemDoan = dsTourTam.FirstOrDefault(t => t.LaDoan);

                var datTour = new DatTour
                {
                    ID_NguoiDung = userId.Value,
                    NgayDat = DateTime.Now,
                    TongTien = tongTienTamTinh,
                    TrangThai = AppConstants.TrangThaiDatTour.ChoXacNhanDiemDon,
                    LoaiDat = laDoan ? AppConstants.LoaiDat.Doan : AppConstants.LoaiDat.KhachLe,
                    TruongDoan = itemDoan?.TruongDoan,
                    SdtTruongDoan = itemDoan?.SdtTruongDoan,
                    GhiChuDoan = itemDoan?.GhiChuDoan,
                    GhiChu = "Khách yêu cầu điểm đón khác. Admin cần xác nhận điểm đón và phụ thu trước khi thanh toán."
                };

                _contextDB.DatTours.Add(datTour);
                _contextDB.SaveChanges();

                foreach (var item in dsTourTam)
                {
                    _contextDB.ChiTietDatTours.Add(new ChiTietDatTour
                    {
                        ID_DatTour = datTour.ID_DatTour,
                        ID_Tour = item.TourId,
                        NgayKhoiHanh = item.NgayDi,
                        SoLuongNguoi = item.SoLuong,
                        Gia = item.GiaThucTe ?? item.Gia,
                        PhuongThucThanhToan = "Chưa thanh toán",
                        LoaiDiemDon = item.LoaiDiemDon,
                        DiemDon = item.DiemDon,
                        DiaChiDon = item.DiaChiDon,
                        TinhThanhDon = item.TinhThanhDon,
                        PhuThuDiemDon = item.PhuThuDiemDon,
                        GhiChuDiemDon = item.GhiChuDiemDon,
                        CanXacNhanDiemDon = item.CanXacNhanDiemDon
                    });
                }

                _contextDB.SaveChanges();
                SendAdminBookingEmail(datTour.ID_DatTour);
                Session.Remove("DanhSachTourTamThoi");

                TempData["Success"] = "Đã gửi yêu cầu xác nhận điểm đón. Nhân viên sẽ liên hệ bạn trước khi thanh toán.";
                return RedirectToAction("LichSuDatTour");
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;
                TempData["Error"] = "Lỗi khi gửi yêu cầu xác nhận điểm đón: " + errorMessage;
                return RedirectToAction("XacNhanDatTour");
            }
        }
        public ActionResult ThanhToan()
        {
            
            var dsTourTam = Session["DanhSachTourTamThoi"] as List<TourDaDatTamThoi>;
            if (dsTourTam == null || dsTourTam.Count == 0)
            {
                TempData["Error"] = "Bạn chưa chọn tour nào để thanh toán.";
                return RedirectToAction("Index");
            }

            if (dsTourTam.Any(t => t.CanXacNhanDiemDon))
            {
                TempData["Error"] = "Điểm đón của bạn cần admin xác nhận trước khi thanh toán.";
                return RedirectToAction("XacNhanDatTour");
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

        public ActionResult ThanhToanDon(int id)
        {
            int? userId = Session["ID_NguoiDung"] as int?;
            if (!userId.HasValue)
            {
                TempData["Error"] = "Bạn phải đăng nhập để thanh toán.";
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            var datTour = _contextDB.DatTours
                .Include("ChiTietDatTours.Tour")
                .FirstOrDefault(d => d.ID_DatTour == id && d.ID_NguoiDung == userId.Value);

            if (datTour == null) return HttpNotFound();
            if (datTour.TrangThai != AppConstants.TrangThaiDatTour.ChoThanhToan)
            {
                TempData["Error"] = "Đơn này chưa sẵn sàng để thanh toán.";
                return RedirectToAction("LichSuDatTour");
            }

            var user = _contextDB.NguoiDungs.Find(userId.Value);
            var model = new ThanhToanView
            {
                ID_DatTour = datTour.ID_DatTour,
                PhuongThucThanhToan = AppConstants.ThanhToan.ChuyenKhoan,
                UserInfo = user,
                DanhSachTour = datTour.ChiTietDatTours.Select(ct => new TourDaDatTamThoi
                {
                    TourId = ct.ID_Tour ?? 0,
                    TenTour = ct.Tour.TenTour,
                    NgayDi = ct.NgayKhoiHanh ?? DateTime.Today,
                    SoLuong = ct.SoLuongNguoi ?? 0,
                    Gia = ct.Gia,
                    GiaThucTe = ct.Gia,
                    LoaiDiemDon = ct.LoaiDiemDon,
                    DiemDon = ct.DiemDon,
                    DiaChiDon = ct.DiaChiDon,
                    TinhThanhDon = ct.TinhThanhDon,
                    PhuThuDiemDon = ct.PhuThuDiemDon,
                    GhiChuDiemDon = ct.GhiChuDiemDon,
                    CanXacNhanDiemDon = ct.CanXacNhanDiemDon
                }).ToList()
            };

            return View("ThanhToan", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanThanhToanDon(int ID_DatTour)
        {
            int? userId = Session["ID_NguoiDung"] as int?;
            if (!userId.HasValue)
            {
                TempData["Error"] = "Bạn phải đăng nhập để thanh toán.";
                return RedirectToAction("DangNhap", "NguoiDung");
            }

            var datTour = _contextDB.DatTours
                .Include("ChiTietDatTours")
                .FirstOrDefault(d => d.ID_DatTour == ID_DatTour && d.ID_NguoiDung == userId.Value);

            if (datTour == null) return HttpNotFound();
            if (datTour.TrangThai != AppConstants.TrangThaiDatTour.ChoThanhToan)
            {
                TempData["Error"] = "Đơn này chưa sẵn sàng để thanh toán.";
                return RedirectToAction("LichSuDatTour");
            }
            if (datTour.ChiTietDatTours.Any(c => c.CanXacNhanDiemDon))
            {
                TempData["Error"] = "Điểm đón của đơn này vẫn cần admin xác nhận.";
                return RedirectToAction("LichSuDatTour");
            }

            using (var transaction = _contextDB.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    foreach (var item in datTour.ChiTietDatTours)
                    {
                        var lichUpdate = _contextDB.LichKhoiHanhs.FirstOrDefault(l =>
                            l.ID_Tour == item.ID_Tour &&
                            l.NgayKhoiHanh == item.NgayKhoiHanh &&
                            l.TrangThai == AppConstants.TrangThaiLichKhoiHanh.Mo);

                        if (lichUpdate == null)
                            throw new InvalidOperationException("Ngày khởi hành không còn mở. Vui lòng liên hệ nhân viên.");

                        var soLuong = item.SoLuongNguoi ?? 0;
                        var conLai = lichUpdate.SoLuongToiDa - lichUpdate.SoLuongDaDat;
                        if (soLuong > conLai)
                            throw new InvalidOperationException($"Chỉ còn {conLai} chỗ cho ngày {item.NgayKhoiHanh:dd/MM/yyyy}.");

                        lichUpdate.SoLuongDaDat += soLuong;
                        if (lichUpdate.SoLuongDaDat >= lichUpdate.SoLuongToiDa)
                            lichUpdate.TrangThai = AppConstants.TrangThaiLichKhoiHanh.HetCho;

                        item.PhuongThucThanhToan = AppConstants.ThanhToan.ChuyenKhoan;
                    }

                    datTour.TongTien = datTour.ChiTietDatTours.Sum(c => ((c.Gia ?? 0) + (c.PhuThuDiemDon ?? 0)) * (c.SoLuongNguoi ?? 0));
                    datTour.TrangThai = AppConstants.TrangThaiDatTour.ChoXuLy;
                    datTour.GhiChu = "Khách đã xác nhận chuyển khoản sau khi admin xác nhận điểm đón.";
                    _contextDB.SaveChanges();
                    transaction.Commit();
                    SendAdminBookingEmail(datTour.ID_DatTour);

                    TempData["Success"] = "Thanh toán thành công! Đơn của bạn đang chờ xử lý.";
                    return RedirectToAction("LichSuDatTour");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi khi thanh toán: " + ex.Message;
                    return RedirectToAction("LichSuDatTour");
                }
            }
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

            if (dsTourTam.Any(t => t.CanXacNhanDiemDon))
            {
                TempData["Error"] = "Điểm đón của bạn cần admin xác nhận trước khi thanh toán.";
                return RedirectToAction("XacNhanDatTour");
            }

            try
            {
                decimal tongTien = dsTourTam.Sum(t => ((t.GiaThucTe ?? t.Gia ?? 0) + (t.PhuThuDiemDon ?? 0)) * t.SoLuong);
                bool laDoan = dsTourTam.Any(t => t.LaDoan);
                var itemDoan = dsTourTam.FirstOrDefault(t => t.LaDoan);
                int newDatTourId;

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
                            PhuongThucThanhToan = AppConstants.ThanhToan.ChuyenKhoan,
                            LoaiDiemDon = item.LoaiDiemDon,
                            DiemDon = item.DiemDon,
                            DiaChiDon = item.DiaChiDon,
                            TinhThanhDon = item.TinhThanhDon,
                            PhuThuDiemDon = item.PhuThuDiemDon,
                            GhiChuDiemDon = item.GhiChuDiemDon,
                            CanXacNhanDiemDon = item.CanXacNhanDiemDon
                        };
                        _contextDB.ChiTietDatTours.Add(chiTiet);
                    }

                    _contextDB.SaveChanges();
                    newDatTourId = datTour.ID_DatTour;
                    transaction.Commit();
                }

                SendAdminBookingEmail(newDatTourId);
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
                    ,
                    LoaiDiemDon = ct.LoaiDiemDon,
                    DiemDon = ct.DiemDon,
                    DiaChiDon = ct.DiaChiDon,
                    TinhThanhDon = ct.TinhThanhDon,
                    PhuThuDiemDon = ct.PhuThuDiemDon,
                    GhiChuDiemDon = ct.GhiChuDiemDon,
                    CanXacNhanDiemDon = ct.CanXacNhanDiemDon
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
        public ActionResult GuiYeuCauHuy(int ID_DatTour, string LyDo, string TenNganHang, string SoTaiKhoanHoanTien, string TenChuTaiKhoan)
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

            if (string.IsNullOrWhiteSpace(TenNganHang) || string.IsNullOrWhiteSpace(SoTaiKhoanHoanTien) || string.IsNullOrWhiteSpace(TenChuTaiKhoan))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin tài khoản ngân hàng nhận hoàn tiền.";
                return RedirectToAction("LichSuDatTour");
            }

            // Gửi yêu cầu hủy chờ admin duyệt
            var yeuCau = new YeuCauHuy
            {
                ID_DatTour = datTour.ID_DatTour,
                LyDo = LyDo,
                TenNganHang = TenNganHang.Trim(),
                SoTaiKhoanHoanTien = SoTaiKhoanHoanTien.Trim(),
                TenChuTaiKhoan = TenChuTaiKhoan.Trim(),
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
            SendAdminCancelRequestEmail(yeuCau.ID_YeuCauHuy);

            TempData["Success"] = $"Đã gửi yêu cầu hủy tour! Yêu cầu của bạn đang được chờ xử lý. Bạn sẽ được hoàn {phanTram}% ({tienHoan:N0} VNĐ) sau khi admin xác nhận.";
            return RedirectToAction("LichSuDatTour");
        }

        private void SendAdminBookingEmail(int idDatTour)
        {
            try
            {
                var datTour = _contextDB.DatTours
                    .Include("NguoiDung")
                    .Include("ChiTietDatTours.Tour")
                    .FirstOrDefault(d => d.ID_DatTour == idDatTour);

                if (datTour == null) return;

                string emailError;
                AdminEmailService.TrySendBookingNotification(datTour, out emailError);
            }
            catch
            {
                // Không để lỗi email làm hỏng thao tác đặt tour của khách.
            }
        }

        private void SendAdminCancelRequestEmail(int idYeuCauHuy)
        {
            try
            {
                var yeuCau = _contextDB.YeuCauHuys
                    .Include("DatTour.NguoiDung")
                    .FirstOrDefault(y => y.ID_YeuCauHuy == idYeuCauHuy);

                if (yeuCau == null) return;

                string emailError;
                AdminEmailService.TrySendCancelRequestNotification(yeuCau, out emailError);
            }
            catch
            {
                // Không để lỗi email làm hỏng thao tác gửi yêu cầu hủy của khách.
            }
        }
    }
}
