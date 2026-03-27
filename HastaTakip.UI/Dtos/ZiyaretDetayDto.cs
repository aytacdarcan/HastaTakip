namespace HastaTakip.UI.Dtos;

public class ZiyaretDetayDto
{
    public int ZiyaretID { get; set; }
    public int HastaID { get; set; }
    public DateTime? Tarih { get; set; }
    public string? Notlar { get; set; }
    public string? YakinmalarZiyaret { get; set; }
    public string? HastaAd { get; set; }
    public string? HastaSoyad { get; set; }
   
    public int? AntropometriAdet { get; set; }
    public List<AntropometriMiniDto>? Antropometriler { get; set; }
    public DateTime? BirthDate { get; set; }
}
