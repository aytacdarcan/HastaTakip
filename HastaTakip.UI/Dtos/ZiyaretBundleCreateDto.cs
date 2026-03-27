namespace HastaTakip.UI.Dtos
{
    public class ZiyaretBundleCreateDto
    {
        public ZiyaretCreateDto Ziyaret { get; set; } = new();
        public AntropometriCreateDto? Antropometri { get; set; }
        public PuberteFizikCreateDto? Puberte { get; set; }
        public YorumPlanCreateDto? YorumPlan { get; set; }
        public OzetHesapCreateDto? Ozet { get; set; }
    }

}
