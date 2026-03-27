namespace HastaTakip.UI.Dtos;

public class ZiyaretCreateDto
{
    public int HastaID { get; set; }
    public DateTime Tarih { get; set; }
    public string? Notlar { get; set; }
    public string? YakinmalarZiyaret { get; set; }
}
