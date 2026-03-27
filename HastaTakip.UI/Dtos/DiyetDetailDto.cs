namespace HastaTakip.UI.Dtos
{
    public class DiyetDetailDto
    {
        public int DiyetID { get; set; }
        public int HastaID { get; set; }
        public DateTime Tarih { get; set; }
        public string? Ekmek { get; set; }
        public string? EtPeynir { get; set; }
        public string? Sut { get; set; }
        public string? MeyveSebze { get; set; }
        public string? SiviGida { get; set; }
        public string? AburCubur { get; set; }
    }
}
