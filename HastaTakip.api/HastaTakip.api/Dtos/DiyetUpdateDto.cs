using System.ComponentModel.DataAnnotations;

namespace HastaTakip.Api.Dtos
{
    
    public class DiyetUpdateDto
    {
        public DateTime? Tarih { get; set; }

        
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

        
        [MaxLength(50)] public string? EkranSuresi { get; set; }
    }
}