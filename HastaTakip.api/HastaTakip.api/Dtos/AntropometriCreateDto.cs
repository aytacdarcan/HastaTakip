namespace HastaTakip.Api.Dtos
{
    public class AntropometriCreateDto
    {
        public int ZiyaretID { get; set; }             
        
        public decimal? BoyCm { get; set; }
        public decimal? KiloKg { get; set; }
        public decimal? BasCevresiCm { get; set; }

        public decimal? OturmaBoyuCm { get; set; }
        public decimal? ObTb { get; set; }
        public decimal? GogusCevresiCm { get; set; }
        public decimal? BasPubisCm { get; set; }
        public decimal? PubisTopukCm { get; set; }

        
        public decimal? BKI { get; set; }

    }
}
