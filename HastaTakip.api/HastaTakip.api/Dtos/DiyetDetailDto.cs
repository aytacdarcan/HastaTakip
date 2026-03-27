namespace HastaTakip.Api.Dtos
{
    public class DiyetDetailDto
    {
        public int DiyetID { get; set; }
        public int HastaID { get; set; }
        public DateTime Tarih { get; set; }

        
        public string? Ekmek { get; set; }
        public string? Tahil { get; set; }

        public string? Et { get; set; }
        public string? Peynir { get; set; }

        public string? Sut { get; set; }
        public string? Yogurt { get; set; }

        public string? Meyve { get; set; }
        public string? Sebze { get; set; }

        
        public string? SiviGida { get; set; }

        public string? AburCubur { get; set; }

        
        public string? EkranSuresi { get; set; }
    }
}