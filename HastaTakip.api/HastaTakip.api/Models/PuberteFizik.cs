using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaTakip.Api.Models
{
    [Table("PuberteFizik", Schema = "klinik")]
    public class PuberteFizik
    {
        [Key] public int PuberteID { get; set; }

        [Required] public int ZiyaretID { get; set; }

        public string? PuberteNotu { get; set; }         
        public string? PatolojikFizik { get; set; }      

        [ForeignKey(nameof(ZiyaretID))]
        public Ziyaret Ziyaret { get; set; } = null!;
    }
}
