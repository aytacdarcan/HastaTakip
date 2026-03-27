using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace HastaTakip.Api.Models
{
    [Table("Ziyaret", Schema = "klinik")]
    public class Ziyaret
    {
        [Key] public int ZiyaretID { get; set; }

        [Required] public int HastaID { get; set; }

        [Required] public DateTime Tarih { get; set; }


        public string? Notlar { get; set; }
        public string? YakinmalarZiyaret { get; set; }

        public Patient Hasta { get; set; } = null!;
        public ICollection<Antropometri> Antropometriler { get; set; } = new List<Antropometri>();
        public ICollection<LabSonuc> LabSonuclari { get; set; } = new List<LabSonuc>();
        public ICollection<PuberteFizik> PuberteFizikler { get; set; } = new List<PuberteFizik>();
        public ICollection<YorumPlan> YorumPlanlari { get; set; } = new List<YorumPlan>();
        public ICollection<OzetHesapKlinik> OzetHesaplar { get; set; } = new List<OzetHesapKlinik>();
        



    }
}
