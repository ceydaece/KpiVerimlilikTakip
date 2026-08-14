
namespace KpiVerimlilikTakip.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class KisiYapacagi
{
    public int Id { get; set; }

        public int KisiId { get; set; }

        public int IsId { get; set; }

        public int HedefAdet { get; set; }

        public string DonemTipi { get; set; } = "";

        public DateTime BaslangicTarihi { get; set; }

        public DateTime BitisTarihi { get; set; }
     
        [ForeignKey(nameof(IsId))]    
        public Yapilacaklar? Yapilacaklar { get; set; }
        public int? AtayanKisiId { get; set; }
        public Kisi? AtayanKisi { get; set; }
}