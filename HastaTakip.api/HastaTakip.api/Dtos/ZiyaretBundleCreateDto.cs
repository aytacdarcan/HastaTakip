namespace HastaTakip.Api.Dtos
{
    public class ZiyaretBundleCreateDto
    {
        public ZiyaretCreateDto Ziyaret { get; set; } = null!;
        public AntropometriCreateDto? Antropometri { get; set; }
        public PuberteFizikCreateDto? Puberte { get; set; }
        public YorumPlanCreateDto? YorumPlan { get; set; }
        public OzetHesapCreateDto? Ozet { get; set; }
    }
}
