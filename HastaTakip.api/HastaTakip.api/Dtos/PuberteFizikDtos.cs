namespace HastaTakip.Api.Dtos
{
    public class PuberteFizikListDto
    {
        public int PuberteID { get; set; }
        public int ZiyaretID { get; set; }
        public string? PuberteNotu { get; set; }
        public string? PatolojikFizik { get; set; }
    }

    public class PuberteFizikDetailDto : PuberteFizikListDto { }

    public class PuberteFizikCreateDto
    {
        public int ZiyaretID { get; set; }
        public string? PuberteNotu { get; set; }
        public string? PatolojikFizik { get; set; }
    }

    public class PuberteFizikUpdateDto
    {
        public string? PuberteNotu { get; set; }
        public string? PatolojikFizik { get; set; }
    }
}
