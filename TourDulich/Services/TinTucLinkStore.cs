using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TourDulich.ModelView;

namespace TourDulich.Services
{
    public class TinTucLinkStore
    {
        private readonly string _filePath;

        public TinTucLinkStore(string filePath)
        {
            _filePath = filePath;
        }

        public List<TinTucLinkView> GetAll()
        {
            EnsureSeedData();

            var json = File.ReadAllText(_filePath);
            var items = JsonConvert.DeserializeObject<List<TinTucLinkView>>(json);
            return items ?? new List<TinTucLinkView>();
        }

        public TinTucLinkView Find(int id)
        {
            return GetAll().FirstOrDefault(x => x.Id == id);
        }

        public void Add(TinTucLinkView item)
        {
            var items = GetAll();
            item.Id = items.Count == 0 ? 1 : items.Max(x => x.Id) + 1;
            item.NgayTao = DateTime.Now;
            items.Add(item);
            Save(items);
        }

        public bool Update(TinTucLinkView item)
        {
            var items = GetAll();
            var existing = items.FirstOrDefault(x => x.Id == item.Id);
            if (existing == null) return false;

            existing.TieuDe = item.TieuDe;
            existing.MoTaNgan = item.MoTaNgan;
            existing.LinkBaiViet = item.LinkBaiViet;
            existing.HinhAnh = item.HinhAnh;
            existing.Nguon = item.Nguon;
            existing.LaTinHot = item.LaTinHot;
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

        private void Save(List<TinTucLinkView> items)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            var json = JsonConvert.SerializeObject(items.OrderByDescending(x => x.LaTinHot).ThenByDescending(x => x.NgayTao), Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        private void EnsureSeedData()
        {
            if (File.Exists(_filePath)) return;

            var seed = new List<TinTucLinkView>
            {
                new TinTucLinkView
                {
                    Id = 1,
                    TieuDe = "Khám phá Hạ Long mùa hè",
                    MoTaNgan = "Tận hưởng kỳ nghỉ tại vịnh Hạ Long với các hoạt động chèo kayak, thăm hang động và nghỉ dưỡng.",
                    HinhAnh = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQYfPf08vD2dhaQAnpljaaOmoEGyfb8uJx_UQ&s",
                    LinkBaiViet = "https://www.google.com/search?q=kinh+nghiem+du+lich+ha+long+mua+he",
                    Nguon = "Google Search",
                    LaTinHot = true,
                    HienThi = true,
                    NgayTao = DateTime.Now
                },
                new TinTucLinkView
                {
                    Id = 2,
                    TieuDe = "Kinh nghiệm du lịch Cao Bằng",
                    MoTaNgan = "Khám phá thiên nhiên, văn hóa bản địa và ẩm thực đặc sắc tại vùng đất phía Bắc Việt Nam.",
                    HinhAnh = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSvAf_HVIF08tV1n2aWcZRfoRXoxsrGeDmI8A&s",
                    LinkBaiViet = "https://www.google.com/search?q=kinh+nghiem+du+lich+cao+bang",
                    Nguon = "Google Search",
                    LaTinHot = true,
                    HienThi = true,
                    NgayTao = DateTime.Now
                },
                new TinTucLinkView
                {
                    Id = 3,
                    TieuDe = "Đi Đà Lạt không lo thiếu ảnh đẹp",
                    MoTaNgan = "Top địa điểm chụp ảnh, quán cà phê đẹp và góc sống ảo ở Đà Lạt được nhiều người quan tâm.",
                    HinhAnh = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRjaeewfJe9lxGZhyoLnxBd7KLMn_o-bylg8A&s",
                    LinkBaiViet = "https://www.google.com/search?q=dia+diem+chup+anh+dep+da+lat",
                    Nguon = "Google Search",
                    LaTinHot = true,
                    HienThi = true,
                    NgayTao = DateTime.Now
                }
            };

            Save(seed);
        }
    }
}
