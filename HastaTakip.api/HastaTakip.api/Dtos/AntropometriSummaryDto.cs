namespace HastaTakip.Api.Dtos
{
    public class AntropometriSummaryDto
    {
        public int AntropometriID { get; set; }
        public int ZiyaretID { get; set; }
        public DateTime ZiyaretTarihi { get; set; }

        public decimal? BoyCm { get; set; }
        public decimal? KiloKg { get; set; }
        public decimal? BasCevresiCm { get; set; }
        public decimal? BKI { get; set; }   
    }
}
