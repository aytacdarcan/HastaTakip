// Dtos/YorumPlanViewDto.cs
namespace HastaTakip.Api.Dtos
{
    public class YorumPlanViewDto
    {
        public int YorumID { get; set; }
        public int ZiyaretID { get; set; }
        public string? TedaviBeslenmeSpor { get; set; }
        public string? YorumNotlar { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
    }
}
