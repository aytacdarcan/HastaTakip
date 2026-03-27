using System.ComponentModel.DataAnnotations;

namespace HastaTakip.Api.Dtos
{
    public class PatientCreateDto
    {
        [Required, StringLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required]                           
        public DateTime BirthDate { get; set; }

        [Required, StringLength(1)]        
        public string Cinsiyet { get; set; } = "E";

        [Required, StringLength(11)]
        public string TcKimlikNo { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telefon { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(250)]
        public string? Adres { get; set; }
    }
}
