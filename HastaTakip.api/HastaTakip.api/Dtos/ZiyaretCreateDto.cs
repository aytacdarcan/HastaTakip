namespace HastaTakip.Api.Dtos
{
    public class ZiyaretCreateDto
    {
        public int HastaID { get; set; }
        public DateTime Tarih { get; set; } = DateTime.Now;
        public string? Notlar { get; set; }
        public string? YakinmalarZiyaret { get; set; }
    }
}
