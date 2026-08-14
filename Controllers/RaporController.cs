using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KpiVerimlilikTakip.Data;
using KpiVerimlilikTakip.Models.ViewModels;
using KpiVerimlilikTakip.Services;//ai baglantısı

namespace KpiVerimlilikTakip.Controllers
{
    public class RaporController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AiService _aiService;//ai baglantısı

        public RaporController(AppDbContext context, AiService aiService)
        {
            _context = context;
             _aiService = aiService;
        }

        public async Task<IActionResult> Index()
            {
                var kullaniciId = HttpContext.Session.GetInt32("KullaniciId");

                if (kullaniciId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var hedefler = _context.KisiYapacaklari
                    .Include(h => h.Yapilacaklar)
                    .Where(h => h.KisiId == kullaniciId.Value)
                    .ToList();

                var raporListesi = new List<VerimlilikRaporuVM>();

                foreach (var hedef in hedefler)
                {
                    var toplamTamamlanan = _context.Tamamlanan
                        .Where(t => t.PlanlananIsId == hedef.Id)
                        .Sum(t => (int?)t.TamamlananAdet) ?? 0;

                    var basariYuzdesi = hedef.HedefAdet > 0
                        ? toplamTamamlanan * 100.0 / hedef.HedefAdet
                        : 0;

                    var raporSatiri = new VerimlilikRaporuVM
                    {
                        IsAdi = hedef.Yapilacaklar?.IsTanimi ?? "İş bulunamadı",
                        HedefAdet = hedef.HedefAdet,
                        TamamlananAdet = toplamTamamlanan,
                        BasariYuzdesi = basariYuzdesi
                    };

                    raporListesi.Add(raporSatiri);
                }
                
                return View(raporListesi);
            }
            [HttpPost]
public async Task<IActionResult> AiAnaliz()
{
    var kullaniciId = HttpContext.Session.GetInt32("KullaniciId");

    if (kullaniciId == null)
    {
        return RedirectToAction("Login", "Account");
    }

    var hedefler = _context.KisiYapacaklari
        .Include(h => h.Yapilacaklar)
        .Where(h => h.KisiId == kullaniciId.Value)
        .ToList();

    var raporListesi = new List<VerimlilikRaporuVM>();

    foreach (var hedef in hedefler)
    {
        var toplamTamamlanan = _context.Tamamlanan
            .Where(t => t.PlanlananIsId == hedef.Id)
            .Sum(t => (int?)t.TamamlananAdet) ?? 0;

        var basari = hedef.HedefAdet == 0
            ? 0
            : toplamTamamlanan * 100.0 / hedef.HedefAdet;

        raporListesi.Add(new VerimlilikRaporuVM
        {
            IsAdi = hedef.Yapilacaklar?.IsTanimi ?? "",
            HedefAdet = hedef.HedefAdet,
            TamamlananAdet = toplamTamamlanan,
            BasariYuzdesi = basari
        });
    }

    ViewBag.AiMesaj =
        await _aiService.VerimlilikAnaliziYapAsync(raporListesi);

    return View("Index", raporListesi);
}
    }
    
}