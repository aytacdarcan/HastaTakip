namespace HastaTakip.Api.Dtos
{
    public class ZiyaretDetailDto
    {
        public int ZiyaretID { get; set; }
        public int HastaID { get; set; }
        public DateTime Tarih { get; set; }
        public string? Notlar { get; set; }

        
        public string? YakinmalarZiyaret { get; set; }

       
        public string? HastaAd { get; set; }
        public string? HastaSoyad { get; set; }

       
        public DateTime? BirthDate { get; set; }

    }
}
