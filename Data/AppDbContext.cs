using Microsoft.EntityFrameworkCore;
using KpiVerimlilikTakip.Models;

namespace KpiVerimlilikTakip.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Kisi> Kisiler => Set<Kisi>();
    public DbSet<Yapilacaklar> Yapilacaklar => Set<Yapilacaklar>();
    public DbSet<KisiYapacagi> KisiYapacaklari => Set<KisiYapacagi>();
    public DbSet<Tamamlanan> Tamamlanan => Set<Tamamlanan>();
    public DbSet<Bildirim> Bildirimler { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<KisiYapacagi>()
        .HasOne(k => k.Yapilacaklar)
        .WithMany(y => y.KisiYapacaklari)
        .HasForeignKey(k => k.IsId);
    modelBuilder.Entity<Tamamlanan>()
        .HasOne(t => t.KisiYapacagi)
        .WithMany()
        .HasForeignKey(t => t.PlanlananIsId);

    modelBuilder.Entity<KisiYapacagi>()
    .HasOne(h => h.AtayanKisi)
    .WithMany()
    .HasForeignKey(h => h.AtayanKisiId)
    .OnDelete(DeleteBehavior.Restrict);
   
}

}
