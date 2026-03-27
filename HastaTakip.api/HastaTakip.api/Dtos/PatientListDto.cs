using System;

namespace HastaTakip.Api.Dtos
{
    public class PatientListDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }

        public string? Cinsiyet { get; set; }      
        public DateTime KayitTarihi { get; set; }   
    }
}
