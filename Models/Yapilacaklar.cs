namespace KpiVerimlilikTakip.Models;
public class Yapilacaklar
{
    public int Id { get; set; }

    public string IsTanimi { get; set; } = "";

    public string? Aciklama { get; set; } 

    public List<KisiYapacagi> KisiYapacaklari { get; set; } = new();
}