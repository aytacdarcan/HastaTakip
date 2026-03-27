using System.ComponentModel.DataAnnotations;

namespace HastaTakip.Api.Dtos
{
    public class DiyetCreateDto
    {
        [Required]
        public int HastaID { get; set; }

        
        [Required]
        public DateTime Tarih { get; set; }

        
        [MaxLength(100)] public string? Ekmek { get; set; }
        [MaxLength(100)] public string? Tahil { get; set; }

        [MaxLength(100)] public string? Et { get; set; }
        [MaxLength(100)] public string? Peynir { get; set; }

        [MaxLength(100)] public string? Sut { get; set; }
        [MaxLength(100)] public string? Yogurt { get; set; }

        [MaxLength(100)] public string? Meyve { get; set; }
        [MaxLength(100)] public string? Sebze { get; set; }

        
        [MaxLength(100)] public string? SiviGida { get; set; }

        [MaxLength(100)] public string? AburCubur { get; set; }

        
        [MaxLength(100)] public string? EkranSuresi { get; set; }
    }
}