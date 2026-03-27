namespace HastaTakip.UI.Dtos;

public sealed class PatientDetailDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = default!;
    public string Soyad { get; set; } = default!;
    public DateTime? BirthDate { get; set; }
    public string? Cinsiyet { get; set; }

   
    public string TcKimlikNo { get; set; } = default!;
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adres { get; set; }
}
