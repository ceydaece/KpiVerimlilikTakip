using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KpiVerimlilikTakip.Data;
using KpiVerimlilikTakip.Models;


namespace KpiVerimlilikTakip.Controllers
{
    public class TamamlananlarController : Controller
    {
        private readonly AppDbContext _context;

        public TamamlananlarController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GunlukGiris()
        {
            var kullaniciId =
                HttpContext.Session.GetInt32("KullaniciId");

            if (kullaniciId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var hedefler = _context.KisiYapacaklari
                .Include(h => h.Yapilacaklar)
                .Where(h => h.KisiId == kullaniciId.Value)
                .ToList();

            return View(hedefler);
        }
        [HttpPost]
public IActionResult GunlukGiris(
    List<int> PlanlananIsId,
    List<int> TamamlananAdet)
{
    var kullaniciId =
        HttpContext.Session.GetInt32("KullaniciId");

    if (kullaniciId == null)
    {
        return RedirectToAction("Login", "Account");
    }

    for (int i = 0; i < PlanlananIsId.Count; i++)
    {
        if (TamamlananAdet[i] <= 0)
        {
            continue;
        }

        var hedefKullaniciyaAitMi =
            _context.KisiYapacaklari.Any(h =>
                h.Id == PlanlananIsId[i] &&
                h.KisiId == kullaniciId.Value);

        if (!hedefKullaniciyaAitMi)
        {
            continue;
        }

        var tamamlanan = new Tamamlanan
        {
            PlanlananIsId = PlanlananIsId[i],
            TamamlananAdet = TamamlananAdet[i],
            TamamlanmaTarihi = DateTime.Now
        };

        _context.Tamamlanan.Add(tamamlanan);
    }

    _context.SaveChanges();

    return RedirectToAction("Index", "Dashboard");
        }
    }
}