using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaTakip.Api.Models
{
    [Table("GrowthLMS", Schema = "klinik")]
    public class GrowthLMS
    {
        [Key]
        public int GrowthID { get; set; }

        [Required, StringLength(10)]
        public string Kaynak { get; set; } = "NEYZI";

        [Required, StringLength(200)]
        public string Olcum { get; set; } = string.Empty;  

        [Required, StringLength(1)]
        public string Cinsiyet { get; set; } = "E";        

        [Required]
        public int YasAy { get; set; }                     

        [Required]
        public double L { get; set; }

        [Required]
        public double M { get; set; }

        [Required]
        public double S { get; set; }
    }
}
