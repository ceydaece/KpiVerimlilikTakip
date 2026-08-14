using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KpiVerimlilikTakip.Data;
using KpiVerimlilikTakip.Models.ViewModels;
using KpiVerimlilikTakip.Services;

namespace KpiVerimlilikTakip.Controllers;

public class DashboardController : Controller
{
    private readonly AppDbContext _context;
    private readonly AiService _aiService;

    public DashboardController(
        AppDbContext context,
        AiService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    [HttpGet]
    public IActionResult Index(string? arama)
    {
        var kullaniciId =
            HttpContext.Session.GetInt32("KullaniciId");

        if (kullaniciId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var kullanici = _context.Kisiler
        .AsNoTracking()
        .FirstOrDefault(k => k.Id == kullaniciId.Value);

    if (kullanici == null)
    {
        return RedirectToAction("Login", "Account");
    }
        
        
        var model = DashboardModeliHazirla(
            kullaniciId.Value,
            arama
        );
        var yetki = HttpContext.Session.GetString("Yetki");

if (yetki == "Yonetici")
{
    model.ToplamCalisan = _context.Kisiler
        .Count(k =>
            k.Yetki == "Calisan" &&
            k.YoneticiId == kullaniciId.Value);

    model.ToplamAtananHedef = _context.KisiYapacaklari
        .Count(h =>
            h.AtayanKisiId == kullaniciId.Value);

    model.GecikenHedefSayisi = _context.KisiYapacaklari
        .Count(h =>
            h.AtayanKisiId == kullaniciId.Value &&
            h.BitisTarihi < DateTime.Today);

    model.OrtalamaBasari = model.GenelBasariYuzdesi;
}

        // POST işleminden gelen AI önerisini gösterir.
        model.AIOnerisi =
            TempData["AIOnerisi"]?.ToString() ?? "";
        model.YoneticiAIOnerisi =
                TempData["YoneticiAIOnerisi"]?.ToString() ?? "";

        ViewBag.Arama = arama;


if (string.Equals(
        kullanici.Yetki?.Trim(),
        "Yonetici",
        StringComparison.OrdinalIgnoreCase))
    {
        return View("DashboardYonetici", model);
    }

    // Diğer kullanıcılar çalışan View'ını açar
    return View("DashboardCalisan", model);
}

    [HttpPost]
    public async Task<IActionResult> AiOnerisiOlustur()
    {
        var kullaniciId =
            HttpContext.Session.GetInt32("KullaniciId");

        if (kullaniciId == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var model = DashboardModeliHazirla(
        kullaniciId.Value,
        null
    );

      var aiOnerisi =
            await _aiService.DashboardOnerisiOlusturAsync(
                model.ToplamHedefSayisi,
                model.ToplamTamamlananAdet,
                model.GenelBasariYuzdesi,
                model.YaklasanHedefler.Count
            );

        TempData["AIOnerisi"] = aiOnerisi;

        return RedirectToAction("Index");
    }

    private DashboardVM DashboardModeliHazirla(
        int kullaniciId,
        string? arama)
    {
        // Kullanıcının bütün hedefleri
        var tumHedefler = _context.KisiYapacaklari
            .Include(h => h.Yapilacaklar)
            .Where(h => h.KisiId == kullaniciId)
            .OrderBy(h => h.BitisTarihi)
            .ToList();

        var hedefIdleri = tumHedefler
            .Select(h => h.Id)
            .ToList();

        // Özet kartları bütün hedeflere göre hesaplanır.
        var toplamHedefSayisi = tumHedefler.Count;

        var toplamHedefAdedi = tumHedefler
            .Sum(h => h.HedefAdet);

        var toplamTamamlananAdet = _context.Tamamlanan
            .Where(t =>
                hedefIdleri.Contains(t.PlanlananIsId))
            .Sum(t => (int?)t.TamamlananAdet) ?? 0;

        var genelBasariYuzdesi =
            toplamHedefAdedi > 0
                ? toplamTamamlananAdet * 100.0
                    / toplamHedefAdedi
                : 0;

        // Arama yalnızca tabloda görünen hedefleri filtreler.
        var gorunenHedefler = tumHedefler;

        if (!string.IsNullOrWhiteSpace(arama))
        {
            gorunenHedefler = gorunenHedefler
                .Where(h =>
                    (
                        h.Yapilacaklar != null &&
                        h.Yapilacaklar.IsTanimi.Contains(
                            arama,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    ||
                    h.DonemTipi.Contains(
                        arama,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .ToList();
        }

        var model = new DashboardVM
        {
            ToplamHedefSayisi = toplamHedefSayisi,
            ToplamTamamlananAdet =
                toplamTamamlananAdet,
            GenelBasariYuzdesi =
                genelBasariYuzdesi
        };

        foreach (var hedef in gorunenHedefler)
        {
            var tamamlanan = _context.Tamamlanan
                .Where(t =>
                    t.PlanlananIsId == hedef.Id)
                .Sum(t => (int?)t.TamamlananAdet) ?? 0;

            var basari = hedef.HedefAdet > 0
                ? tamamlanan * 100.0
                    / hedef.HedefAdet
                : 0;

            string durum;

            if (basari >= 100)
            {
                durum = "🟢 Tamamlandı";
            }
            else if (basari >= 50)
            {
                durum = "🟡 Devam Ediyor";
            }
            else
            {
                durum = "🔴 Geride";
            }

            var kalanGun =
                (hedef.BitisTarihi.Date -
                 DateTime.Today).Days;

            string oncelik;

            if (kalanGun < 0)
            {
                oncelik = "Gecikti";
            }
            else if (kalanGun <= 3)
            {
                oncelik = "Yüksek";
            }
            else if (kalanGun <= 7)
            {
                oncelik = "Orta";
            }
            else
            {
                oncelik = "Düşük";
            }

            model.Hedefler.Add(new DashboardHedefVM
            {
                Id = hedef.Id,
                IsAdi =
                    hedef.Yapilacaklar?.IsTanimi
                    ?? "İş bulunamadı",
                HedefAdet = hedef.HedefAdet,
                TamamlananAdet = tamamlanan,
                BasariYuzdesi = basari,
                DonemTipi = hedef.DonemTipi,
                BaslangicTarihi =
                    hedef.BaslangicTarihi,
                BitisTarihi =
                    hedef.BitisTarihi,
                Durum = durum,
                Oncelik = oncelik, 
                AtayanKisiId = hedef.AtayanKisiId
            });
        }

        // Yaklaşan hedefler aramadan bağımsız olarak
        // bütün hedeflerden oluşturulur.
        model.YaklasanHedefler = tumHedefler
            .Where(h =>
                h.BitisTarihi.Date >= DateTime.Today &&
                h.BitisTarihi.Date
                    <= DateTime.Today.AddDays(3))
            .Select(h =>
            {
                var tamamlanan = _context.Tamamlanan
                    .Where(t =>
                        t.PlanlananIsId == h.Id)
                    .Sum(t =>
                        (int?)t.TamamlananAdet) ?? 0;

                var basari = h.HedefAdet > 0
                    ? tamamlanan * 100.0
                        / h.HedefAdet
                    : 0;

                return new DashboardHedefVM
                {
                    Id = h.Id,
                    IsAdi =
                        h.Yapilacaklar?.IsTanimi
                        ?? "İş bulunamadı",
                    HedefAdet = h.HedefAdet,
                    TamamlananAdet = tamamlanan,
                    BasariYuzdesi = basari,
                    DonemTipi = h.DonemTipi,
                    BaslangicTarihi =h.BaslangicTarihi,
                    BitisTarihi =
                        h.BitisTarihi
                };
            })
            .OrderBy(h => h.BitisTarihi)
            .ToList();
          model.Calisanlar = _context.Kisiler
            .Where(k =>
                k.Yetki == "Calisan" &&
                k.YoneticiId == kullaniciId
    )
    .OrderBy(k => k.AdSoyad)
    .ToList(); 
    model.KendiHedefleri = model.Hedefler
    .Where(h => h.AtayanKisiId == null)
    .ToList();

model.YoneticiHedefleri = model.Hedefler
    .Where(h => h.AtayanKisiId != null)
    .ToList();
    model.Bildirimler = _context.Bildirimler
    .Where(b =>
        b.KisiId == kullaniciId &&
        !b.Okundu)
    .OrderByDescending(b => b.Tarih)
    .ToList();

        return model;
    }
    [HttpGet]
public IActionResult CalisanDetay(int id)
{
    var yoneticiId =
        HttpContext.Session.GetInt32("KullaniciId");

    var yetki =
        HttpContext.Session.GetString("Yetki");

    if (yoneticiId == null)
    {
        return RedirectToAction("Login", "Account");
    }

    if (yetki != "Yonetici")
    {
        return Forbid();
    }

    var calisan = _context.Kisiler
        .AsNoTracking()
        .FirstOrDefault(k =>
            k.Id == id &&
            k.Yetki == "Calisan" &&
            k.YoneticiId == yoneticiId.Value);

    if (calisan == null)
    {
        return NotFound();
    }

    var hedefler = _context.KisiYapacaklari
        .Include(h => h.Yapilacaklar)
        .Where(h => h.KisiId == calisan.Id)
        .OrderBy(h => h.BitisTarihi)
        .ToList();

    var hedefIdleri = hedefler
        .Select(h => h.Id)
        .ToList();

    var toplamHedefAdedi = hedefler
        .Sum(h => h.HedefAdet);

    var toplamTamamlananAdet = _context.Tamamlanan
        .Where(t => hedefIdleri.Contains(t.PlanlananIsId))
        .Sum(t => (int?)t.TamamlananAdet) ?? 0;

    var genelBasariYuzdesi = toplamHedefAdedi > 0
        ? toplamTamamlananAdet * 100.0 / toplamHedefAdedi
        : 0;

    var model = new CalisanDetayVM
    {
        CalisanId = calisan.Id,
        AdSoyad = calisan.AdSoyad,
        Gorev = calisan.Gorev,
        Email = calisan.Email,
        ToplamHedefSayisi = hedefler.Count,
        ToplamHedefAdedi = toplamHedefAdedi,
        ToplamTamamlananAdet = toplamTamamlananAdet,
        GenelBasariYuzdesi = genelBasariYuzdesi
    };

    foreach (var hedef in hedefler)
    {
        var tamamlanan = _context.Tamamlanan
            .Where(t => t.PlanlananIsId == hedef.Id)
            .Sum(t => (int?)t.TamamlananAdet) ?? 0;

        var basari = hedef.HedefAdet > 0
            ? tamamlanan * 100.0 / hedef.HedefAdet
            : 0;

        model.Hedefler.Add(new DashboardHedefVM
        {
            Id = hedef.Id,
            IsAdi = hedef.Yapilacaklar?.IsTanimi
                ?? "İş bulunamadı",
            HedefAdet = hedef.HedefAdet,
            TamamlananAdet = tamamlanan,
            BasariYuzdesi = basari,
            DonemTipi = hedef.DonemTipi,
            BaslangicTarihi = hedef.BaslangicTarihi,
            BitisTarihi = hedef.BitisTarihi
        });
    }

    return View(model);
}
[HttpPost]
public async Task<IActionResult> YoneticiAiAnalizi()
{
    var yoneticiId =
        HttpContext.Session.GetInt32("KullaniciId");

    var yetki =
        HttpContext.Session.GetString("Yetki");

    if (yoneticiId == null)
    {
        return RedirectToAction("Login", "Account");
    }

    if (yetki != "Yonetici")
    {
        return Forbid();
    }

    // Sadece bu yöneticiye bağlı çalışanları getiriyoruz.
    var calisanlar = _context.Kisiler
        .Where(k =>
            k.Yetki == "Calisan" &&
            k.YoneticiId == yoneticiId.Value)
        .ToList();

    var aiCalisanlar = new List<AiCalisanVM>();
    

    foreach (var calisan in calisanlar)
    {
        // Çalışanın bütün hedefleri
        
        var hedefler = _context.KisiYapacaklari
            .Include(h => h.Yapilacaklar)
            .Where(h => h.KisiId == calisan.Id)
            .ToList();
    
        var hedefIdleri = hedefler
            .Select(h => h.Id)
            .ToList();

        // Hedef adetlerinin toplamı
        var toplamHedef = hedefler
            .Sum(h => h.HedefAdet);

        // Tamamlanan adetlerinin toplamı
        var toplamTamamlanan = _context.Tamamlanan
            .Where(t =>
                hedefIdleri.Contains(t.PlanlananIsId))
            .Sum(t => (int?)t.TamamlananAdet) ?? 0;

        // Başarı yüzdesi
        var basari = toplamHedef > 0
            ? toplamTamamlanan * 100.0 / toplamHedef
            : 0;

        var gecikenHedef = hedefler.Count(h =>
        h.BitisTarihi.Date < DateTime.Today);

        var aktifHedef = hedefler.Count(h =>
            h.BitisTarihi.Date >= DateTime.Today);

        var enCokYaptigiIs = hedefler
            .Where(h => h.Yapilacaklar != null)
            .GroupBy(h => h.IsId)
            .OrderByDescending(g => g.Count())
            .Select(g =>
                g.First().Yapilacaklar!.IsTanimi)
            .FirstOrDefault()
            ?? "Yok";

            aiCalisanlar.Add(new AiCalisanVM
            {
                AdSoyad = calisan.AdSoyad,
                Hedef = toplamHedef,
                Tamamlanan = toplamTamamlanan,
                Basari = basari,
                GecikenHedefSayisi = gecikenHedef,
                AktifHedefSayisi = aktifHedef,
                EnCokYaptigiIs = enCokYaptigiIs
            });
        }

        var aiOnerisi =
            await _aiService.YoneticiEkipAnaliziAsync(aiCalisanlar);

        TempData["YoneticiAIOnerisi"] = aiOnerisi;

        return RedirectToAction("Index");
}
[HttpPost]
public IActionResult BildirimOkundu(int id)
{
    var kullaniciId =
        HttpContext.Session.GetInt32("KullaniciId");

    if (kullaniciId == null)
    {
        return RedirectToAction("Login", "Account");
    }

    var bildirim = _context.Bildirimler
        .FirstOrDefault(b =>
            b.Id == id &&
            b.KisiId == kullaniciId.Value);

    if (bildirim == null)
    {
        return NotFound();
    }

    bildirim.Okundu = true;

    _context.SaveChanges();

    return RedirectToAction("Index");
}
}