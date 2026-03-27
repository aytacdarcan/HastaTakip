namespace HastaTakip.UI.Dtos;

public class PatientListDto
{
    public int Id { get; set; }
    public string? Ad { get; set; }
    public string? Soyad { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Cinsiyet { get; set; }
    public DateTime KayitTarihi { get; set; }
}
