namespace HastaTakip.Api.Dtos
{
    public class YorumPlanListDto
    {
        public int YorumID { get; set; }
        public int ZiyaretID { get; set; }
        public string? TedaviBeslenmeSpor { get; set; }
        public string? YorumNotlar { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
    }

    public class YorumPlanDetailDto : YorumPlanListDto { }

    public class YorumPlanCreateDto
    {
        public int ZiyaretID { get; set; }
        public string? TedaviBeslenmeSpor { get; set; }
        public string? YorumNotlar { get; set; }
        public DateTime? OlusturmaTarihi { get; set; } 
    }

    public class YorumPlanUpdateDto
    {
        public string? TedaviBeslenmeSpor { get; set; }
        public string? YorumNotlar { get; set; }
        public DateTime? OlusturmaTarihi { get; set; }
    }
}
