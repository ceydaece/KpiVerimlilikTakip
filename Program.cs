using KpiVerimlilikTakip.Services;
using KpiVerimlilikTakip.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using KpiVerimlilikTakip.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<AiService>();
builder.Services.AddScoped<IPasswordHasher<Kisi>, PasswordHasher<Kisi>>();
builder.Services.AddSession();

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();


app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
