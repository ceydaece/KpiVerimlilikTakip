using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KpiVerimlilikTakip.Data;
using KpiVerimlilikTakip.Models;

namespace KpiVerimlilikTakip.Controllers
{
    public class HedefController : Controller
    {
        private readonly AppDbContext _context;

        public HedefController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult YeniHedef(int? kisiId)
        {
            var kullaniciId = HttpContext.Session.GetInt32("KullaniciId");

            if (kullaniciId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var yetki = HttpContext.Session.GetString("Yetki");

        ViewBag.Yapilacaklar =_context.Yapilacaklar
                .OrderBy(y => y.IsTanimi)
                .ToList();

        ViewBag.YoneticiMi = yetki == "Yonetici";

        if (yetki == "Yonetici")
        {
            ViewBag.Calisanlar = _context.Kisiler
                .Where(k =>
                    k.Yetki == "Calisan" &&
                    k.YoneticiId == kullaniciId.Value)
                        .OrderBy(k => k.AdSoyad)
                            .ToList();

            ViewBag.SecilenKisiId = kisiId;
        }

    return View();
}

        [HttpPost]
public IActionResult YeniHedef(KisiYapacagi hedef)
{
    var kullaniciId =
        HttpContext.Session.GetInt32("KullaniciId");

    if (kullaniciId == null)
    {
        return RedirectToAction("Login", "Account");
    }

    var yetki =
        HttpContext.Session.GetString("Yetki");

    if (hedef.BitisTarihi < hedef.BaslangicTarihi)
    {
        TempData["HataMesaji"] =
            "Bitiş tarihi başlangıç tarihinden önce olamaz.";
         return RedirectToAction("YeniHedef",
            new
            {
                kisiId = yetki == "Yonetici"
                    ? (int?)hedef.KisiId
                    : null
            }
        );
    }
    if (hedef.HedefAdet <= 0)
    {
        TempData["HataMesaji"] =
            "Hedef adedi sıfırdan büyük olmalıdır.";
         return RedirectToAction("YeniHedef",
            new
            {
                kisiId = yetki == "Yonetici"
                    ? (int?)hedef.KisiId
                    : null
            }
        );
    }
    if (yetki == "Yonetici")
    {
        var secilenCalisanGecerliMi =
            _context.Kisiler.Any(k =>
                k.Id == hedef.KisiId &&
                k.Yetki == "Calisan" &&
                k.YoneticiId == kullaniciId.Value
            );

        if (!secilenCalisanGecerliMi)
        {
            TempData["HataMesaji"] =
                "Geçerli bir çalışan seçiniz.";

            return RedirectToAction("YeniHedef");
        }

        hedef.AtayanKisiId = kullaniciId.Value;
    }
    else
    {
        hedef.KisiId = kullaniciId.Value;
        hedef.AtayanKisiId = null;
    }

    var ayniHedefVarMi =
        _context.KisiYapacaklari.Any(h =>
            h.KisiId == hedef.KisiId &&
            h.IsId == hedef.IsId &&
            h.DonemTipi == hedef.DonemTipi &&
            h.BaslangicTarihi == hedef.BaslangicTarihi &&
            h.BitisTarihi == hedef.BitisTarihi
        );

    if (ayniHedefVarMi)
    {
        TempData["HataMesaji"] =
            "Bu çalışan için aynı dönem ve tarihlerde bu hedef zaten mevcut.";

        return RedirectToAction(
            "YeniHedef",
            new
            {
                kisiId = yetki == "Yonetici"
                    ? (int?)hedef.KisiId
                    : null
            }
        );
    }
    hedef.Yapilacaklar = null;

    _context.KisiYapacaklari.Add(hedef);
    _context.SaveChanges();
    if (yetki == "Yonetici")
{
    var isAdi = _context.Yapilacaklar
        .Where(i => i.Id == hedef.IsId)
        .Select(i => i.IsTanimi)
        .FirstOrDefault() ?? "Yeni hedef";

    var bildirim = new Bildirim
    {
        KisiId = hedef.KisiId,
        Mesaj = $"Yöneticiniz size yeni bir hedef atadı: {isAdi}",
        Tarih = DateTime.Now,
        Okundu = false
    };

    _context.Bildirimler.Add(bildirim);
    _context.SaveChanges();
}

    TempData["BasariliMesaj"] =
        yetki == "Yonetici"
            ? "Hedef çalışana başarıyla atandı."
            : "Hedef başarıyla oluşturuldu.";

    return RedirectToAction("Index", "Dashboard");
}

        public IActionResult Sil(int id)
        {
            var hedef = _context.KisiYapacaklari.FirstOrDefault(x => x.Id == id);

            if (hedef == null)
            {
                return NotFound();
            }

            _context.KisiYapacaklari.Remove(hedef);
            _context.SaveChanges();

            return RedirectToAction("Index", "Dashboard");
        }
        [HttpGet]
        public IActionResult Duzenle(int id)
        {
            var hedef = _context.KisiYapacaklari
                .FirstOrDefault(x => x.Id == id);

            if (hedef == null)
            {
                return NotFound();
            }

            ViewBag.Yapilacaklar = _context.Yapilacaklar.ToList();

            return View(hedef);
            }
            [HttpPost]
            public IActionResult Duzenle(KisiYapacagi hedef)
            {
                var mevcutHedef = _context.KisiYapacaklari
                    .FirstOrDefault(x => x.Id == hedef.Id);

                if (mevcutHedef == null)
                {
                    return NotFound();
                }

                mevcutHedef.IsId = hedef.IsId;
                mevcutHedef.HedefAdet = hedef.HedefAdet;
                mevcutHedef.DonemTipi = hedef.DonemTipi;
                mevcutHedef.BaslangicTarihi = hedef.BaslangicTarihi;
                mevcutHedef.BitisTarihi = hedef.BitisTarihi;

                _context.SaveChanges();

                return RedirectToAction("Index", "Dashboard");
            }
    }
}