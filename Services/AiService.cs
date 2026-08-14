using System.Text;
using System.Text.Json;
using KpiVerimlilikTakip.Models.ViewModels;

namespace KpiVerimlilikTakip.Services;

public class AiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AiService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    // Sadece bağlantıyı test etmek için kullanılır.
    public async Task<string> TestMesajiAsync()
    {
        return await GeminiyeSorAsync(
            "Sadece 'Merhaba Ceyda' yaz.");
    }

    // Verimlilik raporu sayfasındaki ayrıntılı KPI verilerini analiz eder.
    public async Task<string> VerimlilikAnaliziYapAsync(
        List<VerimlilikRaporuVM> raporlar)
    {
        if (raporlar.Count == 0)
        {
            return "Analiz edilecek KPI verisi bulunmuyor.";
        }

        var prompt = new StringBuilder();

        prompt.AppendLine(
            "Sen deneyimli bir KPI ve verimlilik danışmanısın.");

        prompt.AppendLine(
            "Aşağıdaki KPI verilerini analiz et.");

        prompt.AppendLine(
            "En güçlü performansı, geliştirilmesi gereken alanı ve uygulanabilir bir öneriyi belirt.");

        prompt.AppendLine(
            "Türkçe, kısa, profesyonel ve motive edici bir cevap yaz.");

        prompt.AppendLine(
            "En fazla 5 cümle kullan.");

        prompt.AppendLine();

        foreach (var rapor in raporlar)
        {
            prompt.AppendLine($"İş: {rapor.IsAdi}");
            prompt.AppendLine($"Hedef: {rapor.HedefAdet}");
            prompt.AppendLine(
                $"Tamamlanan: {rapor.TamamlananAdet}");

            prompt.AppendLine(
                $"Başarı oranı: %{rapor.BasariYuzdesi:0.##}");

            prompt.AppendLine();
        }

        return await GeminiyeSorAsync(prompt.ToString());
    }

    // Dashboard üzerindeki kısa AI önerisini oluşturur.
    public async Task<string> DashboardOnerisiOlusturAsync(
        int toplamHedef,
        int tamamlanan,
        double basariYuzdesi,
        int yaklasanHedefSayisi)
    {
        if (toplamHedef == 0)
        {
            return "AI önerisi oluşturmak için önce bir hedef eklemelisiniz.";
        }

        var prompt = $"""
            Sen deneyimli bir KPI ve verimlilik danışmanısın.

            Kullanıcının güncel bilgileri:

            Toplam hedef sayısı: {toplamHedef}
            Toplam tamamlanan adet: {tamamlanan}
            Genel başarı oranı: %{basariYuzdesi:0.##}
            Önümüzdeki 3 gün içinde bitecek hedef sayısı: {yaklasanHedefSayisi}

            Kullanıcıya Türkçe, kısa, profesyonel,
            motive edici ve eyleme geçirilebilir bir öneri ver.

            Başarı oranını değerlendir.
            Yaklaşan hedef varsa zaman yönetimi önerisi ekle.
            En fazla 3 cümle yaz.
            """;

        return await GeminiyeSorAsync(prompt);
    }

    // Gemini API ile iletişim kuran ortak yardımcı metot.
    // Diğer metotlar yalnızca prompt hazırlar ve bu metodu çağırır.
    private async Task<string> GeminiyeSorAsync(string prompt)
    {
        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return "Gemini API anahtarı bulunamadı.";
        }

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key={apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        try
        {
            using var response =
                await _httpClient.PostAsync(url, content);

            var responseJson =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 429)
                {
                    return "Gemini kullanım sınırına ulaşıldı. "
                         + "Lütfen yaklaşık 30 saniye sonra tekrar deneyin.";
                }

                if ((int)response.StatusCode == 401 ||
                    (int)response.StatusCode == 403)
                {
                    return "Gemini API anahtarı geçersiz "
                         + "veya bu işlem için yetkili değil.";
                }

                if ((int)response.StatusCode == 404)
                {
                    return "Kullanılan Gemini modeli bulunamadı.";
                }

                return $"Gemini API isteği başarısız oldu: "
                     + $"{response.StatusCode}";
            }

            using var jsonDocument =
                JsonDocument.Parse(responseJson);

            var root = jsonDocument.RootElement;

            if (!root.TryGetProperty(
                    "candidates",
                    out var candidates) ||
                candidates.GetArrayLength() == 0)
            {
                return "Gemini tarafından bir cevap oluşturulamadı.";
            }

            var firstCandidate = candidates[0];

            if (!firstCandidate.TryGetProperty(
                    "content",
                    out var candidateContent) ||
                !candidateContent.TryGetProperty(
                    "parts",
                    out var parts) ||
                parts.GetArrayLength() == 0 ||
                !parts[0].TryGetProperty(
                    "text",
                    out var textElement))
            {
                return "Gemini cevabındaki metin okunamadı.";
            }

            return textElement.GetString()
                ?? "AI cevabı alınamadı.";
        }
        catch (HttpRequestException)
        {
            return "Gemini servisine bağlanılamadı. "
                 + "İnternet bağlantınızı kontrol edin.";
        }
        catch (JsonException)
        {
            return "Gemini cevabı geçerli JSON formatında değildi.";
        }
        catch (TaskCanceledException)
        {
            return "Gemini isteği zaman aşımına uğradı.";
        }
    }
    public async Task<string> YoneticiEkipAnaliziAsync(
    List<AiCalisanVM> calisanlar)
{
    if (calisanlar.Count == 0)
    {
        return "Analiz edilecek çalışan verisi bulunmuyor.";
    }

    var prompt = new StringBuilder();

    prompt.AppendLine(
        "Sen deneyimli bir KPI ve ekip verimliliği danışmanısın.");

    prompt.AppendLine(
        "Aşağıdaki çalışanların KPI sonuçlarını analiz et.");

    prompt.AppendLine();

    foreach (var calisan in calisanlar)
    {
    prompt.AppendLine($"Çalışan: {calisan.AdSoyad}");
    prompt.AppendLine($"Toplam Hedef: {calisan.Hedef}");
    prompt.AppendLine($"Tamamlanan: {calisan.Tamamlanan}");
    prompt.AppendLine($"Başarı: %{calisan.Basari:0.##}");
    prompt.AppendLine($"Aktif Hedef: {calisan.AktifHedefSayisi}");
    prompt.AppendLine($"Geciken Hedef: {calisan.GecikenHedefSayisi}");
    prompt.AppendLine($"En Çok Yaptığı İş: {calisan.EnCokYaptigiIs}");
    prompt.AppendLine("--------------------------------");
    }

    prompt.AppendLine("Şunları analiz et:");
prompt.AppendLine("- En başarılı çalışan kim?");
prompt.AppendLine("- Performansı geliştirilmesi gereken çalışan kim?");
prompt.AppendLine("- Geciken hedefleri dikkat çeken çalışan var mı?");
prompt.AppendLine("- Ekip genelinde hangi iş türü yoğun?");
prompt.AppendLine("- Yöneticiye uygulanabilir öneriler ver.");
prompt.AppendLine("Türkçe, kısa ve profesyonel yaz.");
    return await GeminiyeSorAsync(prompt.ToString());
}
}