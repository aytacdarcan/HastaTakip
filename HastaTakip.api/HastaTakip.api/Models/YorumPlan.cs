using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaTakip.Api.Models
{
    [Table("YorumPlan", Schema = "klinik")]
    public class YorumPlan
    {
        [Key] public int YorumID { get; set; }

        [Required] public int ZiyaretID { get; set; }

        public string? TedaviBeslenmeSpor { get; set; }  
        public string? YorumNotlar { get; set; }         

        [Column(TypeName = "datetime2(7)")]
        public DateTime OlusturmaTarihi { get; set; }

        [ForeignKey(nameof(ZiyaretID))]
        public Ziyaret Ziyaret { get; set; } = null!;
    }
}
