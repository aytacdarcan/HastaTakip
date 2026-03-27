namespace HastaTakip.UI.Dtos
{
    public class AileUyesiListDto
    {
        public int AileUyesiID { get; set; }
        public int HastaID { get; set; }
        public string? Iliski { get; set; }
        public string? Ad { get; set; }
        public DateTime? DogumTarihi { get; set; }
        public decimal? BoyCm { get; set; }
        public decimal? AgirlikKg { get; set; }
        public double? PuberteYasiYil { get; set; }
        public string? SaglikDurumu { get; set; }
        public string? Meslek { get; set; }
    }
}
