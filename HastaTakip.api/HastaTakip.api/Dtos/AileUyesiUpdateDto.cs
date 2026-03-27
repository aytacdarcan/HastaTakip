using System.ComponentModel.DataAnnotations;

namespace HastaTakip.Api.Dtos
{
    
    public class AileUyesiUpdateDto
    {
        [Required, MaxLength(20)]
        public string Iliski { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Ad { get; set; }

        public DateTime? DogumTarihi { get; set; }
        public decimal? BoyCm { get; set; }
        public decimal? AgirlikKg { get; set; }
        public decimal? PuberteYasiYil { get; set; }

        [MaxLength(100)]
        public string? SaglikDurumu { get; set; }

        [MaxLength(100)]
        public string? Meslek { get; set; }
    }
}
