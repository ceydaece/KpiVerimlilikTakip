namespace KpiVerimlilikTakip.Models;

public class Bildirim
{
    public int Id { get; set; }

    public int KisiId { get; set; }

    public string Mesaj { get; set; } = "";

    public DateTime Tarih { get; set; } = DateTime.Now;

    public bool Okundu { get; set; }

    public Kisi? Kisi { get; set; }
}