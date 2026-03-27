using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaTakip.Api.Models
{
    [Table("AileUyesi", Schema = "klinik")] 
    public class AileUyesi
    {
        [Key] public int AileUyesiID { get; set; }

        [Required] public int HastaID { get; set; }

        [Required, MaxLength(20)]
        public string Iliski { get; set; } = string.Empty; 

        [MaxLength(50)]
        public string? Ad { get; set; }

        public DateTime? DogumTarihi { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? BoyCm { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AgirlikKg { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? PuberteYasiYil { get; set; }

        [MaxLength(100)]
        public string? SaglikDurumu { get; set; }

        [MaxLength(100)]
        public string? Meslek { get; set; }

        // Navigation
        [ForeignKey(nameof(HastaID))]
        public Patient Hasta { get; set; } = null!;
    }

}
