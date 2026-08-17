using Microsoft.AspNetCore.Mvc;
using KpiVerimlilikTakip.Data;
using KpiVerimlilikTakip.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace KpiVerimlilikTakip.Controllers


{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Kisi> _passwordHasher;

       public AccountController(AppDbContext context, IPasswordHasher<Kisi> passwordHasher)
            {
                _context = context;
                _passwordHasher = passwordHasher;
            }
 
        [HttpGet]
        public IActionResult Register()
        {
    ViewBag.Yoneticiler = _context.Kisiler
        .Where(k => k.Yetki == "Yonetici")
        .OrderBy(k => k.AdSoyad)
        .ToList();

    return View();
        }

        [HttpPost]
        public IActionResult Register(Kisi kisi)
        {
                    if (kisi == null)
            {
                return Content("Kisi nesnesi null geldi.");
            }
            var mevcutKullanici = _context.Kisiler.FirstOrDefault(k => k.Email == kisi.Email);

            if (mevcutKullanici != null)
            {
                TempData["Hata"] = "Bu email adresi zaten kayıtlı.";
                return RedirectToAction("Register");
            }
            kisi.KayitTarihi = DateTime.Now;
            kisi.Yetki = "Calisan";
            kisi.SifreHash = _passwordHasher.HashPassword(kisi, kisi.SifreHash);

var yoneticiVarMi = _context.Kisiler.Any(k =>
    k.Id == kisi.YoneticiId &&
    k.Yetki == "Yonetici"
);

if (!yoneticiVarMi)
{
    TempData["Hata"] = "Geçerli bir yönetici seçiniz.";
    return RedirectToAction("Register");
}

            _context.Kisiler.Add(kisi);
            _context.SaveChanges();

            
            return RedirectToAction("Login");
        }
        
               [HttpGet]
            public IActionResult Login()
                    {
                        return View();
                    }

        [HttpPost]
public IActionResult Login(Kisi kisi)
{
    var bulunanKisi = _context.Kisiler.FirstOrDefault(k => k.Email == kisi.Email);

    if (bulunanKisi == null ||
        _passwordHasher.VerifyHashedPassword(
            bulunanKisi,
            bulunanKisi.SifreHash,
            kisi.SifreHash) == PasswordVerificationResult.Failed)
    {
        TempData["Hata"] = "Email veya şifre hatalı.";
        return RedirectToAction("Login");
    }

    HttpContext.Session.SetInt32(
        "KullaniciId",
        bulunanKisi.Id
    );

    HttpContext.Session.SetString(
        "AdSoyad",
        bulunanKisi.AdSoyad
    );

    HttpContext.Session.SetString(
        "Yetki",
        bulunanKisi.Yetki
    );

    TempData["BasariliMesaj"] =
        $"Hoş geldin {bulunanKisi.AdSoyad}";

    return RedirectToAction("Index", "Dashboard");
}
    }
}
