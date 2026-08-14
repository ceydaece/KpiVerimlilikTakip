namespace KpiVerimlilikTakip.Models;

public class Tamamlanan
{
    public int Id { get; set; }

    public int PlanlananIsId { get; set; }

    public int TamamlananAdet { get; set; }

    public DateTime TamamlanmaTarihi { get; set; } = DateTime.Now;


    public KisiYapacagi? KisiYapacagi { get; set; }
}