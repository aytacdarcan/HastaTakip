namespace HastaTakip.UI.Dtos;

public class PatientCreateDto
{
    public string? TcKimlikNo { get; set; } 
    public string? Ad { get; set; }
    public string? Soyad { get; set; }
    public string? Cinsiyet { get; set; }   
    public DateTime? BirthDate { get; set; } 

    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adres { get; set; }
}
