using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace TourDulich.Models
{
    public partial class ModelDB : DbContext
    {
        public ModelDB()
            : base("name=ModelDB")
        {
        }

        public virtual DbSet<ChiTietDatTour> ChiTietDatTours { get; set; }
        public virtual DbSet<DanhGia> DanhGias { get; set; }
        public virtual DbSet<DanhMuc> DanhMucs { get; set; }
        public virtual DbSet<DatTour> DatTours { get; set; }
        public virtual DbSet<DiaDiem> DiaDiems { get; set; }
        public virtual DbSet<HinhAnhTour> HinhAnhTours { get; set; }
        public virtual DbSet<LichTrinhTour> LichTrinhTours { get; set; }
        public virtual DbSet<NguoiDung> NguoiDungs { get; set; }
        public virtual DbSet<Tour> Tours { get; set; }
        public virtual DbSet<LienHe> LienHes { get; set; }
        public virtual DbSet<LichKhoiHanh> LichKhoiHanhs { get; set; }
        public virtual DbSet<MuaGia> MuaGias { get; set; }
        public virtual DbSet<ChinhSachHuy> ChinhSachHuys { get; set; }
        public virtual DbSet<YeuCauHuy> YeuCauHuys { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
