using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TourDulich.ModelView;

namespace TourDulich.Services
{
    public class AdminEmailSettingStore
    {
        private readonly string _filePath;

        public AdminEmailSettingStore(string filePath)
        {
            _filePath = filePath;
        }

        public AdminEmailSettingView Get()
        {
            EnsureFile();
            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<AdminEmailSettingView>(json) ?? new AdminEmailSettingView();
        }

        public void SaveSmtp(AdminEmailSettingView setting)
        {
            var current = Get();
            current.GmailGui = (setting.GmailGui ?? string.Empty).Trim();
            current.TenNguoiGui = string.IsNullOrWhiteSpace(setting.TenNguoiGui) ? "Du Lịch Việt" : setting.TenNguoiGui.Trim();
            current.BatThongBaoLienHe = setting.BatThongBaoLienHe;

            if (!string.IsNullOrWhiteSpace(setting.MatKhauUngDung))
            {
                current.MatKhauUngDung = setting.MatKhauUngDung.Trim();
            }

            Save(current);
        }

        public AdminEmailRecipientView AddRecipient(AdminEmailRecipientView recipient)
        {
            var setting = Get();
            recipient.Id = setting.NguoiNhans.Count == 0 ? 1 : setting.NguoiNhans.Max(x => x.Id) + 1;
            recipient.TenNguoiNhan = (recipient.TenNguoiNhan ?? string.Empty).Trim();
            recipient.Email = (recipient.Email ?? string.Empty).Trim();
            setting.NguoiNhans.Add(recipient);
            Save(setting);
            return recipient;
        }

        public bool UpdateRecipient(AdminEmailRecipientView recipient)
        {
            var setting = Get();
            var existing = setting.NguoiNhans.FirstOrDefault(x => x.Id == recipient.Id);
            if (existing == null) return false;

            existing.TenNguoiNhan = (recipient.TenNguoiNhan ?? string.Empty).Trim();
            existing.Email = (recipient.Email ?? string.Empty).Trim();
            existing.KichHoat = recipient.KichHoat;
            Save(setting);
            return true;
        }

        public bool DeleteRecipient(int id)
        {
            var setting = Get();
            var existing = setting.NguoiNhans.FirstOrDefault(x => x.Id == id);
            if (existing == null) return false;

            setting.NguoiNhans.Remove(existing);
            Save(setting);
            return true;
        }

        private void Save(AdminEmailSettingView setting)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            var json = JsonConvert.SerializeObject(setting, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        private void EnsureFile()
        {
            if (File.Exists(_filePath)) return;
            Save(new AdminEmailSettingView());
        }
    }
}
