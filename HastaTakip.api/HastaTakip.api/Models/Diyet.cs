using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaTakip.Api.Models
{
    [Table("Diyet", Schema = "klinik")]
    public class Diyet
    {
        [Key]
        public int DiyetID { get; set; }

        [Required]
        public int HastaID { get; set; }

        
        [Required]
        [Column(TypeName = "date")]
        public DateTime Tarih { get; set; }

        
        [MaxLength(50)] public string? Ekmek { get; set; }
        [MaxLength(50)] public string? Tahil { get; set; }

        [MaxLength(50)] public string? Et { get; set; }
        [MaxLength(50)] public string? Peynir { get; set; }

        [MaxLength(50)] public string? Sut { get; set; }
        [MaxLength(50)] public string? Yogurt { get; set; }

        [MaxLength(50)] public string? Meyve { get; set; }
        [MaxLength(50)] public string? Sebze { get; set; }

        
        [MaxLength(50)] public string? SiviGida { get; set; }

        [MaxLength(50)] public string? AburCubur { get; set; }

        // 🔹 Alışkanlık
        [MaxLength(50)] public string? EkranSuresi { get; set; }

        // 🔗 Navigation
        [ForeignKey(nameof(HastaID))]
        public Patient Hasta { get; set; } = null!;
    }
}