using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace HastaTakip.Api.Models 
{
    [Table("Antropometri", Schema = "klinik")]
    public class Antropometri
    {
        public int AntropometriID { get; set; }
        public int ZiyaretID { get; set; }

        public int? YasAy { get; set; }                 

        public decimal? BoyCm { get; set; }
        public decimal? KiloKg { get; set; }
        public decimal? BasCevresiCm { get; set; }
        public decimal? OturmaBoyuCm { get; set; }

        [Column("OB_TB")]
        public decimal? ObTb { get; set; }             

        public decimal? GogusCevresiCm { get; set; }
        public decimal? BasPubisCm { get; set; }
        public decimal? PubisTopukCm { get; set; }
        public decimal? BoySDS { get; set; }
        public decimal? KiloSDS { get; set; }
        public decimal? BKISDS { get; set; }

        [Column("BasCevSDS")]
        public decimal? BasCevresiSDS { get; set; }    

        public decimal? YBHSDS { get; set; }
        public decimal? BKI { get; set; }              
       
        [ForeignKey(nameof(ZiyaretID))]

        public Ziyaret Ziyaret { get; set; } = null!;

    }

}