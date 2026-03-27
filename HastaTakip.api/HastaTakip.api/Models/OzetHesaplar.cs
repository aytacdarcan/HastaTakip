// Models/OzetHesapKlinik.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaTakip.Api.Models;

[Table("OzetHesaplar")] 
public class OzetHesapKlinik
{
    [Key]
    public int OzetID { get; set; }                 

    [ForeignKey(nameof(Ziyaret))]
    public int ZiyaretID { get; set; }              

    [Column(TypeName = "decimal(5,2)")]
    public decimal? KulacCm { get; set; }           

    [Column(TypeName = "decimal(5,2)")]
    public decimal? HedefBoyCm { get; set; }        

    [MaxLength(50)]
    public string? BoyaUyanTarti { get; set; }      

    // Nav
    public Ziyaret Ziyaret { get; set; } = null!;
}
