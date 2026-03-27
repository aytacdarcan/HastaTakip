namespace HastaTakip.Api.Dtos
{
    public class AntropometriListDto
    {
        public int AntropometriID { get; set; }
        public int ZiyaretID { get; set; }

        public int? YasAy { get; set; }              
        public decimal? BoyCm { get; set; }
        public decimal? KiloKg { get; set; }
        public decimal? BasCevresiCm { get; set; }
        public decimal? OturmaBoyuCm { get; set; }
        public decimal? ObTb { get; set; }            
        public decimal? GogusCevresiCm { get; set; }
        public decimal? BasPubisCm { get; set; }
        public decimal? PubisTopukCm { get; set; }
        public decimal? BoySDS { get; set; }
        public decimal? KiloSDS { get; set; }
        public decimal? BKISDS { get; set; }
        public decimal? BasCevresiSDS { get; set; }    // ✅ ad standardı
        public decimal? YBHSDS { get; set; }
        public decimal? BKI { get; set; }
    }

}
