using KpiVerimlilikTakip.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KpiVerimlilikTakip.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Kisi>>();

        await context.Database.EnsureCreatedAsync();

        if (await context.Kisiler.AnyAsync())
        {
            return;
        }

        var manager = new Kisi
        {
            AdSoyad = "Demo Manager",
            Email = "manager@example.com",
            Gorev = "Software Engineering Manager",
            DogumTarihi = new DateTime(1990, 1, 1),
            KayitTarihi = DateTime.UtcNow,
            Yetki = "Yonetici"
        };
        manager.SifreHash = passwordHasher.HashPassword(manager, "Demo123!");

        context.Kisiler.Add(manager);
        await context.SaveChangesAsync();

        var employee = new Kisi
        {
            AdSoyad = "Demo Employee",
            Email = "employee@example.com",
            Gorev = "Software Developer",
            DogumTarihi = new DateTime(2000, 1, 1),
            KayitTarihi = DateTime.UtcNow,
            Yetki = "Calisan",
            YoneticiId = manager.Id
        };
        employee.SifreHash = passwordHasher.HashPassword(employee, "Demo123!");

        context.Kisiler.Add(employee);
        await context.SaveChangesAsync();
    }
}
