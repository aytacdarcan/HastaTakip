namespace HastaTakip.UI.Dtos
{
    public class ZiyaretBundleDto
    {
        public ZiyaretDetayDto Ziyaret { get; set; } = null!;
        public AntropometriListDto? Antropometri { get; set; }
        public PuberteFizikViewDto? Puberte { get; set; }
        public YorumPlanViewDto? YorumPlan { get; set; }
        public OzetHesapDto? Ozet { get; set; }
    }
}
