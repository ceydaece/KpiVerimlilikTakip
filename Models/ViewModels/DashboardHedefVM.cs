namespace KpiVerimlilikTakip.Models.ViewModels;

public class DashboardHedefVM
{
    public int Id { get; set; }

    public string IsAdi { get; set; } = "";

    public int HedefAdet { get; set; }

    public int TamamlananAdet { get; set; }

    public double BasariYuzdesi { get; set; }

    public string DonemTipi { get; set; } = "";

    public DateTime BaslangicTarihi { get; set; }

    public DateTime BitisTarihi { get; set; }
    public string Durum { get; set; } = "";
    public string Oncelik { get; set; } = "";
    public string AtayanKisiAdi { get; set; } = "";
    public int? AtayanKisiId { get; set; }
   
}