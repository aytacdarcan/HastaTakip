using HastaTakip.Api.Dtos;

namespace HastaTakip.Api.Dtos
{
    
    public class PatientRegisterDto
    {
        public PatientCreateDto Hasta { get; set; } = null!;

        
        public List<AileUyesiCreateDto>? AileUyeleri { get; set; }
        public OykulerCreateDto? Oykuler { get; set; }
        public List<DiyetCreateDto>? Diyetler { get; set; }

        
        public ZiyaretCreateDto? Ziyaret { get; set; }
        public AntropometriCreateDto? Antropometri { get; set; }
        public PuberteFizikCreateDto? PuberteFizik { get; set; }
        public YorumPlanCreateDto? YorumPlan { get; set; }

        
        public OzetHesapCreateDto? OzetHesap { get; set; }
    }
}
