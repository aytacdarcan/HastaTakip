namespace HastaTakip.UI.Dtos
{
    public class OykulerDetailDto
    {
        public int OykulerID { get; set; }
        public int HastaID { get; set; }

        public string? Yakinmalar { get; set; }
        public string? Oykusu { get; set; }
        public string? GebelikOykusu { get; set; }

        public string? DogumSekli { get; set; }

        public string? DogumAgirlik { get; set; }
        public string? DogumBoyu { get; set; }

        public string? NeonatalDonem { get; set; }
        public string? NorGelisim { get; set; }
        public string? SutcocukBeslenme { get; set; }
        public string? GecirilenHast { get; set; }
        public string? OperasyonKaza { get; set; }

        public string? KanAkrabaligi { get; set; }

        public string? KardesSagligi { get; set; }
        public string? AileHastaliklar { get; set; }

        public DateTime? OlusturmaTarihi { get; set; }
    }
}