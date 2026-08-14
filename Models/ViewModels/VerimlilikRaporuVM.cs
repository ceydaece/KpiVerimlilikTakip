namespace KpiVerimlilikTakip.Models.ViewModels;

public class VerimlilikRaporuVM
{
    public string IsAdi { get; set; } = "";

    public int HedefAdet { get; set; }

    public int TamamlananAdet { get; set; }

    public double BasariYuzdesi { get; set; }
}