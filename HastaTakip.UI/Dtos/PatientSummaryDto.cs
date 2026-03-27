namespace HastaTakip.UI.Dtos;

public class PatientSummaryDto
{
    public int Id { get; set; }
    public string? Ad { get; set; }
    public string? Soyad { get; set; }
    public DateTime? BirthDate { get; set; }

    public int? ZiyaretSayisi { get; set; }
    public DateTime? SonZiyaretTarihi { get; set; }
    public decimal? SonBoyCm { get; set; }
    public decimal? SonKiloKg { get; set; }
    public decimal? SonBKI { get; set; }
}
