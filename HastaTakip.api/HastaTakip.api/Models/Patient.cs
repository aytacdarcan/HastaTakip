using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaTakip.Api.Models
{
    [Table("Hasta", Schema = "klinik")]
    public class Patient
    {
        [Key]
        [Column("HastaID")]
        public int Id { get; set; }

        [Column("TcKimlikNo"), Required, MaxLength(11)]
        public string TcKimlikNo { get; set; } = string.Empty;


        [Column("Ad"), Required, MaxLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Column("Soyad"), Required, MaxLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Column("DogumTarihi", TypeName = "date"), Required]
        public DateTime BirthDate { get; set; }

        [Column("Cinsiyet"), Required, StringLength(1)]
        public string Cinsiyet { get; set; } = "E"; // 'E'/'K'

        [Column("Telefon"), MaxLength(20)]
        public string? Telefon { get; set; }

        [Column("Email"), MaxLength(100)]
        public string? Email { get; set; }

        [Column("Adres"), MaxLength(250)]
        public string? Adres { get; set; }

        [Column("KayitTarihi", TypeName = "datetime2(7)"), Required]
        public DateTime KayitTarihi { get; set; }

        [NotMapped]
        public string FullName => $"{Ad} {Soyad}".Trim();
    }
}
