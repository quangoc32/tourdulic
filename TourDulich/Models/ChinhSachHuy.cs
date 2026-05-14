namespace TourDulich.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ChinhSachHuy")]
    public partial class ChinhSachHuy
    {
        [Key]
        public int ID_ChinhSach { get; set; }

        /// <summary>Hủy trước bao nhiêu ngày so với ngày khởi hành</summary>
        public int SoNgayTuHuy { get; set; }

        /// <summary>Phần trăm hoàn tiền (0-100)</summary>
        public int PhanTramHoan { get; set; }

        [StringLength(200)]
        public string MoTa { get; set; }
    }
}
