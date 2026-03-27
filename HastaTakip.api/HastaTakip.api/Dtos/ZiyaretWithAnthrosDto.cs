namespace HastaTakip.Api.Dtos
{
    public class ZiyaretWithAnthrosDto
    {
        public int ZiyaretID { get; set; }
        public DateTime Tarih { get; set; }
        public string? Notlar { get; set; }

        
        public string? YakinmalarZiyaret { get; set; }

        public int AntropometriAdet { get; set; }
        public List<AntropometriMiniDto> Antropometriler { get; set; } = new();
    }
}
