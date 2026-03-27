namespace HastaTakip.Api.Models;

public class LabSonuc
{
    public int LabSonucID { get; set; }

    public int ZiyaretID { get; set; }
    public Ziyaret Ziyaret { get; set; } = null!;

    public int LabParametreID { get; set; }
    public LabParametre Parametre { get; set; } = null!;

    public DateTime Tarih { get; set; }

    public string? Deger { get; set; }
    public decimal? DegerSayisal { get; set; }
    public decimal? RefAlt { get; set; }
    public decimal? RefUst { get; set; }
}
