
namespace KpiVerimlilikTakip.Models
{
    public class Kisi
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } ="";
        public string Email { get; set; }="";
        public string SifreHash { get; set; }="";
        public string Gorev { get; set; }="";
        public DateTime DogumTarihi { get; set; }

        public DateTime KayitTarihi { get; set; }= DateTime.Now;
        public List<KisiYapacagi> KisiYapacaklari { get; set; } = new List<KisiYapacagi>();
         
        public string Yetki { get; set; } = "Calisan";
        public int? YoneticiId { get; set; }

public Kisi? Yonetici { get; set; }
    }
}