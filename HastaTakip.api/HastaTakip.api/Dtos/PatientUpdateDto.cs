using System.ComponentModel.DataAnnotations;

namespace HastaTakip.Api.Dtos
{
    
    public class PatientUpdateDto
    {
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public DateTime? BirthDate { get; set; }  
        public string? Cinsiyet { get; set; }     
        public string? TcKimlikNo { get; set; }   
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Adres { get; set; }
    }
}
