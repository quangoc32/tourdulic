using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TourDulich.ModelView;

namespace TourDulich.Services
{
    public class DiemDonTourStore
    {
        private readonly string _filePath;

        public DiemDonTourStore(string filePath)
        {
            _filePath = filePath;
        }

        public List<DiemDonTourView> GetAll()
        {
            if (!File.Exists(_filePath)) return new List<DiemDonTourView>();

            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<DiemDonTourView>>(json) ?? new List<DiemDonTourView>();
        }

        public List<DiemDonTourView> GetByTour(int tourId)
        {
            return GetAll()
                .Where(x => x.ID_Tour == tourId)
                .OrderByDescending(x => x.LaTuTuc)
                .ThenBy(x => x.PhuThu)
                .ThenBy(x => x.TenDiemDon)
                .ToList();
        }

        public DiemDonTourView Find(int id)
        {
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        public void Add(DiemDonTourView item)
        {
            var items = GetAll();
            item.Id = items.Count == 0 ? 1 : items.Max(x => x.Id) + 1;
            item.NgayTao = DateTime.Now;
            items.Add(item);
            Save(items);
        }

        public bool Update(DiemDonTourView item)
        {
            var items = GetAll();
            var existing = items.FirstOrDefault(x => x.Id == item.Id);
            if (existing == null) return false;

            existing.TenDiemDon = item.TenDiemDon;
            existing.DiaChi = item.DiaChi;
            existing.TinhThanh = item.TinhThanh;
            existing.PhuThu = item.PhuThu;
            existing.LaTuTuc = item.LaTuTuc;
            existing.HienThi = item.HienThi;
            Save(items);
            return true;
        }

        public bool Delete(int id)
        {
            var items = GetAll();
            var item = items.FirstOrDefault(x => x.Id == id);
            if (item == null) return false;

            items.Remove(item);
            Save(items);
            return true;
        }

        private void Save(List<DiemDonTourView> items)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(items, Formatting.Indented));
        }
    }
}
