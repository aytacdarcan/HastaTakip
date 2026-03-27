namespace HastaTakip.UI.Dtos;

public class RegisterRequestDto
{
    public PatientCreateDto Hasta { get; set; } = new();

    public List<AileUyesiCreateDto>? AileUyeleri { get; set; }
    public OykulerCreateDto? Oykuler { get; set; }
    public List<DiyetCreateDto>? Diyetler { get; set; }

    public ZiyaretCreateDto? Ziyaret { get; set; }
    public AntropometriCreateDto? Antropometri { get; set; }
    public PuberteFizikCreateDto? PuberteFizik { get; set; }
    public YorumPlanCreateDto? YorumPlan { get; set; }
    public OzetHesapCreateDto? OzetHesap { get; set; }
}
