using System;

namespace HastaTakip.Api.Dtos
{
    public class PatientDetailDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }

        public string Cinsiyet { get; set; } = string.Empty; 

        
        public string AdSoyad => $"{Ad} {Soyad}".Trim();
        public string TcKimlikNo { get; set; } = default!;   
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Adres { get; set; }
    }
}
