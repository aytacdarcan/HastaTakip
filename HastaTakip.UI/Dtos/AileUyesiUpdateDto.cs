namespace HastaTakip.UI.Dtos
{
    public class AileUyesiUpdateDto
    {
        public string? Iliski { get; set; }          
        public string? Ad { get; set; }
        public DateTime? DogumTarihi { get; set; }
        public decimal? BoyCm { get; set; }
        public decimal? AgirlikKg { get; set; }
        public int? PuberteYasiYil { get; set; }   

        public string? SaglikDurumu { get; set; }
        public string? Meslek { get; set; }
    }
}
