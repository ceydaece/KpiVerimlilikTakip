namespace KpiVerimlilikTakip.Models.ViewModels;

public class CalisanDetayVM
{
    public int CalisanId { get; set; }

    public string AdSoyad { get; set; } = "";

    public string Gorev { get; set; } = "";

    public string Email { get; set; } = "";

    public int ToplamHedefSayisi { get; set; }

    public int ToplamHedefAdedi { get; set; }

    public int ToplamTamamlananAdet { get; set; }

    public double GenelBasariYuzdesi { get; set; }

    public List<DashboardHedefVM> Hedefler { get; set; } = new();
}