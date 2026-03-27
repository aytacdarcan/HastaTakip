namespace HastaTakip.Api.Dtos
{
    public class OykulerListDto
    {
        public int OykulerID { get; set; }
        public int HastaID { get; set; }
        public DateTime OlusturmaTarihi { get; set; }

        public string? Yakinmalar { get; set; }
    }
}