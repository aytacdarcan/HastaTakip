using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaTakip.Api.Models
{
    [Table("Oykuler", Schema = "klinik")]
    public class Oykuler
    {
        [Key] public int OykulerID { get; set; }

        [Required] public int HastaID { get; set; }

        
        public string? Yakinmalar { get; set; }
        public string? Oykusu { get; set; } 
        [MaxLength(20)] public string? DogumSekli { get; set; }
        public string? DogumAgirlik { get; set; }
        public string? DogumBoyu { get; set; }
        public string? NeonatalDonem { get; set; }
        public string? NorGelisim { get; set; }
        public string? SutcocukBeslenme { get; set; }
        public string? GecirilenHast { get; set; }
        public string? OperasyonKaza { get; set; }
        public string? GebelikOykusu { get; set; }
        public string? KanAkrabaligi { get; set; }
        public string? KardesSagligi { get; set; }
        public string? AileHastaliklar { get; set; }
       
        [Required]
        [Column(TypeName = "datetime2(7)")]
        public DateTime OlusturmaTarihi { get; set; }

        
        [ForeignKey(nameof(HastaID))]
        public Patient Hasta { get; set; } = null!;
    }
}
