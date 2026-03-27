namespace HastaTakip.Api.Dtos
{
    public class AileUyesiListDto
    {
        public int AileUyesiID { get; set; }
        public int HastaID { get; set; }
        public string Iliski { get; set; } = string.Empty;
        public string? Ad { get; set; }
        public DateTime? DogumTarihi { get; set; }
        public decimal? BoyCm { get; set; }
        public decimal? AgirlikKg { get; set; }
        public decimal? PuberteYasiYil { get; set; }
        public string? SaglikDurumu { get; set; }
        public string? Meslek { get; set; }
    }
}
