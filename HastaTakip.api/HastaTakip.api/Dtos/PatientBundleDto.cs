namespace HastaTakip.Api.Dtos
{
    public class PatientBundleDto
    {
        public PatientDetailDto Hasta { get; set; } = default!;
        public List<AileUyesiListDto> AileUyeleri { get; set; } = new();
        public OykulerDetailDto? Oykuler { get; set; }            // son/tek öykü
        public List<DiyetListDto> Diyetler { get; set; } = new(); // son 10 diyet
       

    }
}
