namespace KpiVerimlilikTakip.Models.ViewModels;
using KpiVerimlilikTakip.Models;


public class DashboardVM
{
    public int ToplamHedefSayisi { get; set; }

    public int ToplamTamamlananAdet { get; set; }

    public double GenelBasariYuzdesi { get; set; }
     public string AIOnerisi { get; set; } = "";

    public List<DashboardHedefVM> Hedefler { get; set; } = new();

    public List<DashboardHedefVM> YaklasanHedefler { get; set; } = new();
    public List<Kisi> Calisanlar { get; set; } = new();
    public List<DashboardHedefVM> KendiHedefleri { get; set; } = new();

    public List<DashboardHedefVM> YoneticiHedefleri { get; set; } = new();
    public int ToplamCalisan { get; set; }

public int ToplamAtananHedef { get; set; }

public int GecikenHedefSayisi { get; set; }

public double OrtalamaBasari { get; set; }
public string YoneticiAIOnerisi { get; set; } = "";
public List<Bildirim> Bildirimler { get; set; } = new();
    }

