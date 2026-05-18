using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using TourDulich.Models;

namespace TourDulich.Services
{
    public class LienHeEmailService
    {
        private readonly AdminEmailSettingStore _store;

        public LienHeEmailService(AdminEmailSettingStore store)
        {
            _store = store;
        }

        public bool TrySendContactNotification(LienHe lienHe, out string errorMessage)
        {
            var subject = "[Du Lịch Việt] Liên hệ mới: " + (lienHe.TieuDe ?? "Không có tiêu đề");
            return TrySend(subject, BuildContactEmailBody(lienHe), lienHe.Email, lienHe.HoTen, out errorMessage);
        }

        public bool TrySendBookingNotification(DatTour datTour, out string errorMessage)
        {
            var customerName = datTour.NguoiDung != null ? datTour.NguoiDung.HoTen : "Khách hàng";
            var subject = "[Du Lịch Việt] Đơn đặt tour mới #" + datTour.ID_DatTour + " - " + customerName;
            return TrySend(subject, BuildBookingEmailBody(datTour), datTour.NguoiDung?.Email, customerName, out errorMessage);
        }

        public bool TrySendCancelRequestNotification(YeuCauHuy yeuCauHuy, out string errorMessage)
        {
            var customerName = yeuCauHuy.DatTour != null && yeuCauHuy.DatTour.NguoiDung != null
                ? yeuCauHuy.DatTour.NguoiDung.HoTen
                : "Khách hàng";
            var subject = "[Du Lịch Việt] Yêu cầu hủy tour mới #" + yeuCauHuy.ID_YeuCauHuy + " - " + customerName;
            return TrySend(subject, BuildCancelRequestEmailBody(yeuCauHuy), yeuCauHuy.DatTour?.NguoiDung?.Email, customerName, out errorMessage);
        }

        public bool TrySendBookingResultToCustomer(DatTour datTour, out string errorMessage)
        {
            var customer = datTour.NguoiDung;
            var subject = "[Du Lịch Việt] Cập nhật đơn đặt tour #" + datTour.ID_DatTour;
            return TrySendToCustomer(customer?.Email, customer?.HoTen, subject, BuildCustomerBookingResultEmailBody(datTour), out errorMessage);
        }

        public bool TrySendCancelResultToCustomer(YeuCauHuy yeuCauHuy, out string errorMessage)
        {
            var customer = yeuCauHuy.DatTour?.NguoiDung;
            var subject = "[Du Lịch Việt] Kết quả yêu cầu hủy tour #" + yeuCauHuy.ID_YeuCauHuy;
            return TrySendToCustomer(customer?.Email, customer?.HoTen, subject, BuildCustomerCancelResultEmailBody(yeuCauHuy), out errorMessage);
        }

        public bool TrySendTestEmail(out string errorMessage)
        {
            var subject = "[Du Lịch Việt] Kiểm tra gửi Gmail admin";
            var body = @"
                <div style='font-family: Arial, sans-serif; color: #111827; line-height: 1.5;'>
                    <h2 style='color: #0d6efd;'>Gửi thử Gmail thành công</h2>
                    <p>Đây là email kiểm tra từ hệ thống quản trị Du Lịch Việt.</p>
                    <p>Nếu bạn nhận được email này, cấu hình Gmail gửi và Gmail admin nhận thông báo đang hoạt động.</p>
                </div>";

            return TrySend(subject, body, null, null, out errorMessage);
        }

        private bool TrySend(string subject, string body, string replyToEmail, string replyToName, out string errorMessage)
        {
            errorMessage = null;
            var setting = _store.Get();
            var recipients = setting.NguoiNhans.Where(x => x.KichHoat && !string.IsNullOrWhiteSpace(x.Email)).ToList();

            if (!setting.BatThongBaoLienHe)
            {
                errorMessage = "Chức năng gửi Gmail đang tắt.";
                return false;
            }

            if (!setting.DaCauHinhSmtp)
            {
                errorMessage = "Chưa cấu hình Gmail gửi hoặc App Password.";
                return false;
            }

            if (!recipients.Any())
            {
                errorMessage = "Chưa có Gmail admin nhận thông báo đang bật.";
                return false;
            }

            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(setting.GmailGui, setting.TenNguoiGui);
                    foreach (var recipient in recipients)
                    {
                        message.To.Add(new MailAddress(recipient.Email, recipient.TenNguoiNhan));
                    }

                    if (!string.IsNullOrWhiteSpace(replyToEmail))
                    {
                        message.ReplyToList.Add(new MailAddress(replyToEmail, replyToName));
                    }

                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    message.BodyEncoding = Encoding.UTF8;
                    message.SubjectEncoding = Encoding.UTF8;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.EnableSsl = true;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(setting.GmailGui, setting.MatKhauUngDung);
                        smtp.Send(message);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private bool TrySendToCustomer(string customerEmail, string customerName, string subject, string body, out string errorMessage)
        {
            errorMessage = null;
            var setting = _store.Get();

            if (!setting.BatThongBaoLienHe)
            {
                errorMessage = "Chức năng gửi Gmail đang tắt.";
                return false;
            }

            if (!setting.DaCauHinhSmtp)
            {
                errorMessage = "Chưa cấu hình Gmail gửi hoặc App Password.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(customerEmail))
            {
                errorMessage = "Khách hàng chưa có email.";
                return false;
            }

            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(setting.GmailGui, setting.TenNguoiGui);
                    message.To.Add(new MailAddress(customerEmail, customerName ?? "Khách hàng"));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    message.BodyEncoding = Encoding.UTF8;
                    message.SubjectEncoding = Encoding.UTF8;

                    using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.EnableSsl = true;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(setting.GmailGui, setting.MatKhauUngDung);
                        smtp.Send(message);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private static string BuildContactEmailBody(LienHe lienHe)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; color: #111827; line-height: 1.5;'>
                    <h2 style='color: #0d6efd;'>Khách hàng gửi liên hệ mới</h2>
                    <p><strong>Họ tên:</strong> {Encode(lienHe.HoTen)}</p>
                    <p><strong>Email khách:</strong> <a href='mailto:{Encode(lienHe.Email)}'>{Encode(lienHe.Email)}</a></p>
                    <p><strong>Tiêu đề:</strong> {Encode(lienHe.TieuDe)}</p>
                    <p><strong>Ngày gửi:</strong> {(lienHe.NgayGui.HasValue ? lienHe.NgayGui.Value.ToString("dd/MM/yyyy HH:mm") : "")}</p>
                    <div style='margin-top: 16px; padding: 14px; background: #f8fafc; border-left: 4px solid #0d6efd;'>
                        <strong>Nội dung:</strong>
                        <div style='white-space: pre-wrap; margin-top: 8px;'>{Encode(lienHe.NoiDung)}</div>
                    </div>
                    <p style='margin-top: 16px; color: #64748b;'>Bạn có thể bấm trả lời email này để phản hồi trực tiếp cho khách.</p>
                </div>";
        }

        private static string BuildBookingEmailBody(DatTour datTour)
        {
            var customer = datTour.NguoiDung;
            var tourRows = string.Join("", datTour.ChiTietDatTours.Select(ct => $@"
                <tr>
                    <td style='padding: 8px; border-bottom: 1px solid #e5e7eb;'>{Encode(ct.Tour?.TenTour)}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #e5e7eb;'>{(ct.NgayKhoiHanh.HasValue ? ct.NgayKhoiHanh.Value.ToString("dd/MM/yyyy") : "")}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #e5e7eb; text-align: center;'>{ct.SoLuongNguoi}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #e5e7eb;'>{Encode(ct.DiemDon)}</td>
                </tr>"));

            return $@"
                <div style='font-family: Arial, sans-serif; color: #111827; line-height: 1.5;'>
                    <h2 style='color: #0d6efd;'>Có đơn đặt tour mới</h2>
                    <p><strong>Mã đơn:</strong> #{datTour.ID_DatTour}</p>
                    <p><strong>Khách hàng:</strong> {Encode(customer?.HoTen)} - {Encode(customer?.SoDienThoai)}</p>
                    <p><strong>Email khách:</strong> <a href='mailto:{Encode(customer?.Email)}'>{Encode(customer?.Email)}</a></p>
                    <p><strong>Ngày đặt:</strong> {(datTour.NgayDat.HasValue ? datTour.NgayDat.Value.ToString("dd/MM/yyyy HH:mm") : "")}</p>
                    <p><strong>Trạng thái:</strong> {Encode(datTour.TrangThai)}</p>
                    <p><strong>Tổng tiền:</strong> {((datTour.TongTien ?? 0).ToString("N0"))} VNĐ</p>
                    <table style='border-collapse: collapse; width: 100%; margin-top: 14px;'>
                        <thead>
                            <tr style='background: #eff6ff;'>
                                <th style='padding: 8px; text-align: left;'>Tour</th>
                                <th style='padding: 8px; text-align: left;'>Ngày đi</th>
                                <th style='padding: 8px;'>Số lượng</th>
                                <th style='padding: 8px; text-align: left;'>Điểm đón</th>
                            </tr>
                        </thead>
                        <tbody>{tourRows}</tbody>
                    </table>
                    <p style='margin-top: 16px; color: #64748b;'>Vui lòng vào trang quản trị để kiểm tra và xử lý đơn.</p>
                </div>";
        }

        private static string BuildCancelRequestEmailBody(YeuCauHuy yeuCauHuy)
        {
            var datTour = yeuCauHuy.DatTour;
            var customer = datTour?.NguoiDung;

            return $@"
                <div style='font-family: Arial, sans-serif; color: #111827; line-height: 1.5;'>
                    <h2 style='color: #dc3545;'>Có yêu cầu hủy tour mới</h2>
                    <p><strong>Mã yêu cầu:</strong> #{yeuCauHuy.ID_YeuCauHuy}</p>
                    <p><strong>Mã đơn:</strong> #{yeuCauHuy.ID_DatTour}</p>
                    <p><strong>Khách hàng:</strong> {Encode(customer?.HoTen)} - {Encode(customer?.SoDienThoai)}</p>
                    <p><strong>Email khách:</strong> <a href='mailto:{Encode(customer?.Email)}'>{Encode(customer?.Email)}</a></p>
                    <p><strong>Ngày gửi:</strong> {yeuCauHuy.NgayGui:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Tổng tiền đơn:</strong> {((datTour?.TongTien ?? 0).ToString("N0"))} VNĐ</p>
                    <p><strong>Dự kiến hoàn:</strong> {yeuCauHuy.PhanTramHoan}% - {((yeuCauHuy.TienHoan ?? 0).ToString("N0"))} VNĐ</p>
                    <div style='margin-top: 16px; padding: 14px; background: #f8fafc; border-left: 4px solid #0d6efd;'>
                        <strong>Tài khoản hoàn tiền:</strong>
                        <p style='margin: 8px 0 0 0;'><strong>Ngân hàng:</strong> {Encode(yeuCauHuy.TenNganHang)}</p>
                        <p style='margin: 4px 0 0 0;'><strong>Số tài khoản:</strong> {Encode(yeuCauHuy.SoTaiKhoanHoanTien)}</p>
                        <p style='margin: 4px 0 0 0;'><strong>Chủ tài khoản:</strong> {Encode(yeuCauHuy.TenChuTaiKhoan)}</p>
                    </div>
                    <div style='margin-top: 16px; padding: 14px; background: #fff5f5; border-left: 4px solid #dc3545;'>
                        <strong>Lý do hủy:</strong>
                        <div style='white-space: pre-wrap; margin-top: 8px;'>{Encode(yeuCauHuy.LyDo)}</div>
                    </div>
                    <p style='margin-top: 16px; color: #64748b;'>Vui lòng vào trang quản trị để duyệt yêu cầu hủy.</p>
                </div>";
        }

        private static string BuildCustomerBookingResultEmailBody(DatTour datTour)
        {
            var customer = datTour.NguoiDung;
            var tourRows = string.Join("", datTour.ChiTietDatTours.Select(ct => $@"
                <tr>
                    <td style='padding: 8px; border-bottom: 1px solid #e5e7eb;'>{Encode(ct.Tour?.TenTour)}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #e5e7eb;'>{(ct.NgayKhoiHanh.HasValue ? ct.NgayKhoiHanh.Value.ToString("dd/MM/yyyy") : "")}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #e5e7eb; text-align: center;'>{ct.SoLuongNguoi}</td>
                </tr>"));

            return $@"
                <div style='font-family: Arial, sans-serif; color: #111827; line-height: 1.5;'>
                    <h2 style='color: #0d6efd;'>Cập nhật đơn đặt tour</h2>
                    <p>Xin chào <strong>{Encode(customer?.HoTen)}</strong>,</p>
                    <p>Đơn đặt tour <strong>#{datTour.ID_DatTour}</strong> của bạn đã được cập nhật.</p>
                    <p><strong>Trạng thái hiện tại:</strong> {Encode(datTour.TrangThai)}</p>
                    <p><strong>Tổng tiền:</strong> {((datTour.TongTien ?? 0).ToString("N0"))} VNĐ</p>
                    <p><strong>Ghi chú:</strong> {Encode(datTour.GhiChu)}</p>
                    <table style='border-collapse: collapse; width: 100%; margin-top: 14px;'>
                        <thead>
                            <tr style='background: #eff6ff;'>
                                <th style='padding: 8px; text-align: left;'>Tour</th>
                                <th style='padding: 8px; text-align: left;'>Ngày đi</th>
                                <th style='padding: 8px;'>Số lượng</th>
                            </tr>
                        </thead>
                        <tbody>{tourRows}</tbody>
                    </table>
                    <p style='margin-top: 16px; color: #64748b;'>Bạn có thể đăng nhập website để xem chi tiết đơn trong lịch sử đặt tour.</p>
                </div>";
        }

        private static string BuildCustomerCancelResultEmailBody(YeuCauHuy yeuCauHuy)
        {
            var datTour = yeuCauHuy.DatTour;
            var customer = datTour?.NguoiDung;

            return $@"
                <div style='font-family: Arial, sans-serif; color: #111827; line-height: 1.5;'>
                    <h2 style='color: #dc3545;'>Kết quả yêu cầu hủy tour</h2>
                    <p>Xin chào <strong>{Encode(customer?.HoTen)}</strong>,</p>
                    <p>Yêu cầu hủy tour của đơn <strong>#{yeuCauHuy.ID_DatTour}</strong> đã được xử lý.</p>
                    <p><strong>Kết quả:</strong> {Encode(yeuCauHuy.TrangThai)}</p>
                    <p><strong>Tiền hoàn:</strong> {yeuCauHuy.PhanTramHoan}% - {((yeuCauHuy.TienHoan ?? 0).ToString("N0"))} VNĐ</p>
                    <p><strong>Ghi chú từ admin:</strong> {Encode(yeuCauHuy.GhiChuAdmin)}</p>
                    <div style='margin-top: 16px; padding: 14px; background: #f8fafc; border-left: 4px solid #0d6efd;'>
                        <strong>Tài khoản hoàn tiền bạn đã cung cấp:</strong>
                        <p style='margin: 8px 0 0 0;'><strong>Ngân hàng:</strong> {Encode(yeuCauHuy.TenNganHang)}</p>
                        <p style='margin: 4px 0 0 0;'><strong>Số tài khoản:</strong> {Encode(yeuCauHuy.SoTaiKhoanHoanTien)}</p>
                        <p style='margin: 4px 0 0 0;'><strong>Chủ tài khoản:</strong> {Encode(yeuCauHuy.TenChuTaiKhoan)}</p>
                    </div>
                    <p style='margin-top: 16px; color: #64748b;'>Nếu cần hỗ trợ thêm, vui lòng liên hệ bộ phận chăm sóc khách hàng.</p>
                </div>";
        }

        private static string Encode(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
