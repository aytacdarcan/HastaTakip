namespace HastaTakip.Api.Models;

public class LabParametre
{
    public int LabParametreID { get; set; }
    public string Kod { get; set; } = "";
    public string Ad { get; set; } = "";
    public string? Birim { get; set; }
    public string? Kategori { get; set; }
}
