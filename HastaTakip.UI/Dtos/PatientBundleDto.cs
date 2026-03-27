namespace HastaTakip.UI.Dtos
{
    public class PatientBundleDto
    {
        public PatientDetailDto Hasta { get; set; } = default!;
        public List<AileUyesiListDto> AileUyeleri { get; set; } = new();
        public OykulerDetailDto? Oykuler { get; set; }
        public List<DiyetListDto> Diyetler { get; set; } = new();
    }
}
