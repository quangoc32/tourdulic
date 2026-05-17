using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TourDulich.ModelView
{
    public class AdminEmailRecipientView
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TenNguoiNhan { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        public bool KichHoat { get; set; } = true;
    }

    public class AdminEmailSettingView
    {
        [EmailAddress]
        [StringLength(150)]
        public string GmailGui { get; set; }

        [StringLength(200)]
        public string MatKhauUngDung { get; set; }

        [StringLength(150)]
        public string TenNguoiGui { get; set; } = "Du Lịch Việt";

        public bool BatThongBaoLienHe { get; set; } = true;

        public List<AdminEmailRecipientView> NguoiNhans { get; set; } = new List<AdminEmailRecipientView>();

        public bool DaCauHinhSmtp => !string.IsNullOrWhiteSpace(GmailGui) && !string.IsNullOrWhiteSpace(MatKhauUngDung);
    }

    public class AdminEmailSettingPageView
    {
        public AdminEmailSettingView EmailSetting { get; set; }
        public List<Models.LienHe> LienHes { get; set; }
    }
}
