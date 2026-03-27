using HastaTakip.Api.Dtos;

public class ZiyaretBundleDto
{
    public ZiyaretDetailDto Ziyaret { get; set; } = null!;

   
    public AntropometriListDto? Antropometri { get; set; }   
    public PuberteFizikViewDto? Puberte { get; set; }
    public YorumPlanViewDto? YorumPlan { get; set; }

    
    public OzetHesapDto? Ozet { get; set; }                  
}
