using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

// ==========================================
// 1. STRATEJİ VE VERİ MODELLERİ
// ==========================================
public enum YatirimciTuru { Garantici = 1, Dengeli = 2, Riskli = 3 }
public enum VadeTuru { KisaVade = 1, OrtaVade = 2, UzunVade = 3 }

public class StockData
{
    public DateTime Tarih { get; set; }
    public string Sembol { get; set; }
    public decimal Kapanis { get; set; }
    public decimal Hacim { get; set; }
}

public class MacroEvent
{
    public DateTime Tarih { get; set; }
    public int EtkiDerecesi { get; set; }
}

// ==========================================
// 2. KLASÖR TABANLI ÇOKLU VERİ YÜKLEYİCİ
// ==========================================
public class DataLoader
{
    public List<StockData> TumVeriler = new List<StockData>();
    public List<MacroEvent> MakroOlaylar = new List<MacroEvent>();

    public void KlasordekiTumVerileriOku(string klasorYolu, string makroYolu)
    {
        TumVeriler.Clear();

        if (!Directory.Exists(klasorYolu))
        {
            Directory.CreateDirectory(klasorYolu);
            Console.WriteLine($"\n📁 '{klasorYolu}' klasörü bulunamadı ve otomatik oluşturuldu.");
            Console.WriteLine($"⚠️ Lütfen CSV dosyalarınızı bu klasörün içine atıp programı tekrar çalıştırın.");
            return;
        }

        string[] csvDosyalari = Directory.GetFiles(klasorYolu, "*.csv");

        if (csvDosyalari.Length == 0)
        {
            Console.WriteLine($"\n⚠️ UYARI: '{klasorYolu}' klasöründe okunacak hiç CSV dosyası bulunamadı!");
            return;
        }

        string[] tarihFormatlari = new string[]
        {
            "yyyy-MM-dd", "MM/dd/yyyy", "dd.MM.yyyy", "dd/MM/yyyy",
            "M/d/yyyy", "d.M.yyyy", "yyyy/MM/dd", "dd-MM-yyyy", "yyyy.MM.dd"
        };

        foreach (var dosyaYolu in csvDosyalari)
        {
            string varsayilanSembol = Path.GetFileNameWithoutExtension(dosyaYolu).ToUpper();
            if (varsayilanSembol.Contains("_")) varsayilanSembol = varsayilanSembol.Split('_')[0];
            if (varsayilanSembol.Contains(" ")) varsayilanSembol = varsayilanSembol.Split(' ')[0];

            var tumSatirlar = File.ReadAllLines(dosyaYolu);
            if (tumSatirlar.Length <= 1) continue;

            char ayirici = tumSatirlar[0].Contains(";") ? ';' : ',';
            string baslik = tumSatirlar[0].ToLower();
            bool sentetikFormatMi = baslik.Contains("sembol");

            foreach (var satir in tumSatirlar.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(satir)) continue;

                var s = satir.Replace("\"", "").Split(ayirici);
                if (s.Length < 6) continue;

                string tarihStr = s[0].Trim();
                DateTime tarih;

                bool tarihOkundu = DateTime.TryParse(tarihStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out tarih) ||
                                   DateTime.TryParseExact(tarihStr, tarihFormatlari, CultureInfo.InvariantCulture, DateTimeStyles.None, out tarih) ||
                                   DateTime.TryParse(tarihStr, new CultureInfo("tr-TR"), DateTimeStyles.None, out tarih);

                if (!tarihOkundu) continue;

                string sembol = varsayilanSembol;
                string fiyatStr = "";
                string hacimStr = "";

                if (sentetikFormatMi)
                {
                    sembol = s[1].Trim().ToUpper();
                    fiyatStr = s[5].Trim();
                    if (s.Length >= 7) hacimStr = s[6].Trim();
                }
                else
                {
                    fiyatStr = s[1].Trim();
                    if (s.Length >= 6) hacimStr = s[5].Trim();
                }

                if (!decimal.TryParse(fiyatStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal kapanisFiyat))
                {
                    if (!decimal.TryParse(fiyatStr, NumberStyles.Any, new CultureInfo("tr-TR"), out kapanisFiyat))
                        continue;
                }

                decimal hacimSayi = 0m;
                if (!string.IsNullOrEmpty(hacimStr))
                {
                    decimal carpan = 1m;
                    if (hacimStr.EndsWith("M")) { carpan = 1000000m; hacimStr = hacimStr.TrimEnd('M'); }
                    else if (hacimStr.EndsWith("K")) { carpan = 1000m; hacimStr = hacimStr.TrimEnd('K'); }
                    else if (hacimStr.EndsWith("B")) { carpan = 1000000000m; hacimStr = hacimStr.TrimEnd('B'); }
                    else if (hacimStr == "-") { hacimStr = "0"; }

                    if (decimal.TryParse(hacimStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal h))
                        hacimSayi = h * carpan;
                    else if (decimal.TryParse(hacimStr, NumberStyles.Any, new CultureInfo("tr-TR"), out decimal hTr))
                        hacimSayi = hTr * carpan;
                }

                if (kapanisFiyat > 0m)
                {
                    TumVeriler.Add(new StockData
                    {
                        Tarih = tarih,
                        Sembol = sembol,
                        Kapanis = kapanisFiyat,
                        Hacim = hacimSayi
                    });
                }
            }
        }

        TumVeriler = TumVeriler.OrderBy(x => x.Tarih).ToList();

        if (File.Exists(makroYolu))
        {
            foreach (var s in File.ReadAllLines(makroYolu).Skip(1).Select(l => l.Split(',')))
            {
                if (s.Length < 3) continue;
                if (DateTime.TryParse(s[0].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime makroTarih))
                {
                    int etki = 0;
                    if (s.Length > 3 && int.TryParse(s[3].Trim(), out etki))
                    {
                        MakroOlaylar.Add(new MacroEvent { Tarih = makroTarih, EtkiDerecesi = etki });
                    }
                }
            }
        }
    }
}

// ==========================================
// 3. CÜZDAN VE MULTI-ASSET BOT
// ==========================================
public class PortfolioManager
{
    public decimal Bakiye { get; private set; }
    public Dictionary<string, int> Lotlar = new Dictionary<string, int>();
    public Dictionary<string, decimal> Maliyetler = new Dictionary<string, decimal>();

    public PortfolioManager(decimal b) { Bakiye = b; }

    public void Al(string sembol, decimal fiyat, int lot, DateTime tarih)
    {
        if (fiyat <= 0m || lot <= 0) return;

        decimal maliyet = (fiyat * lot) * 1.001m;
        if (Bakiye >= maliyet)
        {
            Bakiye -= maliyet;
            if (!Lotlar.ContainsKey(sembol)) Lotlar[sembol] = 0;

            if (Lotlar[sembol] > 0 && Maliyetler.ContainsKey(sembol))
            {
                decimal eskiMaliyet = Maliyetler[sembol] * Lotlar[sembol];
                decimal yeniMaliyet = fiyat * lot;
                Maliyetler[sembol] = (eskiMaliyet + yeniMaliyet) / (Lotlar[sembol] + lot);
            }
            else
            {
                Maliyetler[sembol] = fiyat;
            }

            Lotlar[sembol] += lot;
            Console.WriteLine($"   [AL]  {tarih:yyyy-MM-dd} | {sembol,-10} | {lot,6} Lot | Fiyat: {fiyat,7:F2} | Maliyet: {Maliyetler[sembol],7:F2}");
        }
    }

    public void Sat(string sembol, decimal fiyat, int lot, DateTime tarih, string t)
    {
        if (lot <= 0 || fiyat <= 0m) return;

        Bakiye += (fiyat * lot) * 0.999m;
        Lotlar[sembol] -= lot;
        if (Lotlar[sembol] <= 0) Maliyetler.Remove(sembol);
        Console.WriteLine($"   [SAT] {tarih:yyyy-MM-dd} | {sembol,-10} | {lot,6} Lot | Fiyat: {fiyat,7:F2} | {t}");
    }
}

public class TradingBot
{
    private PortfolioManager _cuzdan;
    private YatirimciTuru _tur;
    private VadeTuru _vade;
    private List<MacroEvent> _makro;
    private int _pencereGunSayisi;

    public TradingBot(PortfolioManager p, YatirimciTuru t, VadeTuru v, List<MacroEvent> m)
    {
        _cuzdan = p;
        _tur = t;
        _vade = v;
        _makro = m;

        _pencereGunSayisi = _vade == VadeTuru.KisaVade ? 20 : (_vade == VadeTuru.OrtaVade ? 60 : 200);
    }

    public string HesaplaAnlikPiyasaKarakteri(List<StockData> hisseGecmisi, int bakilacakGunSayisi = 0)
    {
        var gecerliGecmis = hisseGecmisi?.Where(x => x.Kapanis > 0m).ToList();
        if (gecerliGecmis == null || gecerliGecmis.Count < 10)
            return "Sakin / Yatay Piyasa";

        int gun = bakilacakGunSayisi > 0 ? bakilacakGunSayisi : _pencereGunSayisi;
        var pencere = gecerliGecmis.Skip(Math.Max(0, gecerliGecmis.Count - gun)).ToList();

        decimal baslangicFiyat = pencere.First().Kapanis;
        decimal bitisFiyat = pencere.Last().Kapanis;
        decimal fiyatDegisimi = baslangicFiyat > 0 ? ((bitisFiyat - baslangicFiyat) / baslangicFiyat) * 100m : 0m;

        decimal ortalamaFiyat = pencere.Average(x => x.Kapanis);
        if (ortalamaFiyat <= 0) return "Sakin / Yatay Piyasa";

        decimal varyans = pencere.Average(x => (x.Kapanis - ortalamaFiyat) * (x.Kapanis - ortalamaFiyat));
        decimal standartSapma = (decimal)Math.Sqrt((double)varyans);
        decimal oynaklikOrani = (standartSapma / ortalamaFiyat) * 100m;

        if (oynaklikOrani > 5m && fiyatDegisimi > 0)
            return "Boğa Piyasası";
        else if (oynaklikOrani > 5m && fiyatDegisimi <= 0)
            return "Testere Piyasası";
        else
            return "Sakin / Yatay Piyasa";
    }

    private decimal HesaplaRSI(List<StockData> hisseGecmisi, int periyot)
    {
        if (hisseGecmisi.Count <= periyot) return 50m;

        List<decimal> degisimler = new List<decimal>();
        for (int i = hisseGecmisi.Count - periyot; i < hisseGecmisi.Count; i++)
        {
            degisimler.Add(hisseGecmisi[i].Kapanis - hisseGecmisi[i - 1].Kapanis);
        }

        decimal toplamKazanc = degisimler.Where(d => d > 0).Sum();
        decimal toplamKayip = degisimler.Where(d => d < 0).Sum() * -1;

        if (toplamKayip == 0) return 100m;

        decimal rs = (toplamKazanc / periyot) / (toplamKayip / periyot);
        return 100m - (100m / (1m + rs));
    }

    public void KararVer(List<StockData> tumGecmis, StockData bugun)
    {
        if (bugun.Kapanis <= 0m) return;

        var hisseGecmisi = tumGecmis.Where(x => x.Sembol == bugun.Sembol && x.Kapanis > 0m).ToList();

        int periyot = _tur == YatirimciTuru.Garantici ? 21 : (_tur == YatirimciTuru.Dengeli ? 14 : 9);
        decimal alEsik = _tur == YatirimciTuru.Garantici ? 40m : (_tur == YatirimciTuru.Dengeli ? 30m : 20m);
        decimal satEsik = _tur == YatirimciTuru.Garantici ? 60m : (_tur == YatirimciTuru.Dengeli ? 70m : 80m);
        decimal stopYuzdesi = _tur == YatirimciTuru.Garantici ? 0.02m : (_tur == YatirimciTuru.Dengeli ? 0.04m : 0.07m);

        if (hisseGecmisi.Count <= periyot) return;

        string anlikKarakter = HesaplaAnlikPiyasaKarakteri(hisseGecmisi);

        if (anlikKarakter.Contains("Boğa"))
        {
            alEsik += 5m;
            stopYuzdesi += 0.01m;
        }
        else if (anlikKarakter.Contains("Testere"))
        {
            alEsik -= 5m;
            stopYuzdesi = Math.Max(0.015m, stopYuzdesi - 0.01m);
        }

        if (_cuzdan.Lotlar.ContainsKey(bugun.Sembol) && _cuzdan.Lotlar[bugun.Sembol] > 0 && _cuzdan.Maliyetler.ContainsKey(bugun.Sembol))
        {
            decimal alinmaFiyati = _cuzdan.Maliyetler[bugun.Sembol];
            if (bugun.Kapanis <= alinmaFiyati * (1 - stopYuzdesi))
            {
                _cuzdan.Sat(bugun.Sembol, bugun.Kapanis, _cuzdan.Lotlar[bugun.Sembol], bugun.Tarih, $"⚠️ DİNAMİK STOP-LOSS (%{stopYuzdesi * 100:F1})");
                return;
            }
        }

        decimal rsi = HesaplaRSI(hisseGecmisi, periyot);
        decimal ortalamaHacim = hisseGecmisi.Skip(Math.Max(0, hisseGecmisi.Count - 10)).Take(10).Average(x => x.Hacim);
        bool hacimOnayli = bugun.Hacim > ortalamaHacim || ortalamaHacim == 0;

        int tolerans = _tur == YatirimciTuru.Riskli ? 3 : 2;
        bool riskli = _makro.Any(m => m.Tarih.Date == bugun.Tarih.Date && m.EtkiDerecesi >= tolerans);

        if (rsi < alEsik && hacimOnayli && !riskli)
        {
            decimal ayrilacakButce = _cuzdan.Bakiye * 0.25m;
            if (ayrilacakButce < bugun.Kapanis) ayrilacakButce = _cuzdan.Bakiye;

            int alinacakLot = (int)Math.Floor(ayrilacakButce / (bugun.Kapanis * 1.001m));
            if (alinacakLot > 0)
            {
                _cuzdan.Al(bugun.Sembol, bugun.Kapanis, alinacakLot, bugun.Tarih);
            }
        }
        else if (rsi > satEsik && _cuzdan.Lotlar.ContainsKey(bugun.Sembol) && _cuzdan.Lotlar[bugun.Sembol] > 0)
        {
            _cuzdan.Sat(bugun.Sembol, bugun.Kapanis, _cuzdan.Lotlar[bugun.Sembol], bugun.Tarih, $"{_tur} Kâr Satışı ({anlikKarakter})");
        }
    }
}

// ==========================================
// 4. ANA MOTOR
// ==========================================
class Program
{
    static void Main()
    {
        Console.Title = "GravenAbyss - Multi-Asset Trading Motoru";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==========================================================================");
        Console.WriteLine("    GRAVENABYSS MULTI-ASSET PAPER-TRADING ALGORİTMİK YATIRIM SİSTEMİ ");
        Console.WriteLine("==========================================================================");
        Console.ResetColor();

        Console.Write("\n   Lütfen İsminizi Giriniz: ");
        string kullaniciAdi = Console.ReadLine();

        string klasorYolu = "Veriler";
        DataLoader loader = new DataLoader();

        if (!Directory.Exists(klasorYolu)) Directory.CreateDirectory(klasorYolu);

        loader.KlasordekiTumVerileriOku(klasorYolu, "makro_takvim_sentetik.csv");

        if (loader.TumVeriler.Count == 0)
        {
            Console.WriteLine("\n⚠️ HATA: 'Veriler' klasörüne en az bir CSV dosyası atıp tekrar başlatın.");
            Console.ReadLine();
            return;
        }

        var hisseGruplari = loader.TumVeriler.GroupBy(x => x.Sembol).ToList();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n==========================================================================");
        Console.WriteLine($"   PORTFÖYDEKİ HİSRELER VE 1 YILLIK (~250 İŞLEM GÜNÜ) PİYASA TÜRLERİ");
        Console.WriteLine("==========================================================================");

        TradingBot analizBotu = new TradingBot(new PortfolioManager(1000), YatirimciTuru.Dengeli, VadeTuru.OrtaVade, loader.MakroOlaylar);

        foreach (var grup in hisseGruplari)
        {
            var hisseVerileri = grup.OrderBy(x => x.Tarih).ToList();
            string birYillikTur = analizBotu.HesaplaAnlikPiyasaKarakteri(hisseVerileri, 250);

            Console.WriteLine($"   ► Hisse Sembolü : {grup.Key,-10} | Toplam Veri: {hisseVerileri.Count,4} Gün | 1 Yıllık Piyasa Türü: {birYillikTur}");
        }
        Console.WriteLine("==========================================================================");
        Console.ResetColor();

        bool devamEt = true;

        while (devamEt)
        {
            Console.Write("\n   Başlangıç Bakiyesi Giriniz (TL): ");
            decimal bakiye;
            while (!decimal.TryParse(Console.ReadLine(), out bakiye) || bakiye <= 0)
            {
                Console.Write("   Lütfen geçerli bir bakiye giriniz (TL): ");
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n   --- YATIRIM VADESİ SEÇİMİ ---");
            Console.WriteLine("   [1] Kısa Vade (20 Günlük Rejim Analizi  ~ 1 Ay)");
            Console.WriteLine("   [2] Orta Vade (60 Günlük Rejim Analizi  ~ 3 Ay - İdeal)");
            Console.WriteLine("   [3] Uzun Vade (200 Günlük Rejim Analizi ~ 1 Yıl)");
            Console.ResetColor();
            Console.Write("   Seçiminiz: ");
            int vadeSecim;
            while (!int.TryParse(Console.ReadLine(), out vadeSecim) || vadeSecim < 1 || vadeSecim > 3)
            {
                Console.Write("   Lütfen 1, 2 veya 3 seçiniz: ");
            }
            VadeTuru vade = (VadeTuru)vadeSecim;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n   --- STRATEJİ / RİSK PROFİLİ SEÇİMİ ---");
            Console.WriteLine("   [1] Garantici Profil (RSI-21 | %2 Stop-Loss)");
            Console.WriteLine("   [2] Dengeli Profil   (RSI-14 | %4 Stop-Loss)");
            Console.WriteLine("   [3] Riskli Profil    (RSI-9  | %7 Stop-Loss)");
            Console.ResetColor();
            Console.Write("   Seçiminiz: ");
            int turSecim;
            while (!int.TryParse(Console.ReadLine(), out turSecim) || turSecim < 1 || turSecim > 3)
            {
                Console.Write("   Lütfen 1, 2 veya 3 seçiniz: ");
            }
            YatirimciTuru tur = (YatirimciTuru)turSecim;

            PortfolioManager cuzdan = new PortfolioManager(bakiye);
            TradingBot bot = new TradingBot(cuzdan, tur, vade, loader.MakroOlaylar);

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n==========================================================================");
            Console.WriteLine($"   SEÇİLEN {vade.ToString().ToUpper()} VADEYE GÖRE HİSSELARİN ANLIK PİYASA TÜRLERİ");
            Console.WriteLine("==========================================================================");
            Console.ResetColor();

            foreach (var grup in hisseGruplari)
            {
                var hisseVerileri = grup.OrderBy(x => x.Tarih).ToList();
                string turMetni = bot.HesaplaAnlikPiyasaKarakteri(hisseVerileri);
                Console.WriteLine($"   ► Hisse: {grup.Key,-10} | Seçilen Vade ({vade}) Analiz Türü: {turMetni}");
            }
            Console.WriteLine("==========================================================================");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n   Sayın {kullaniciAdi}, {hisseGruplari.Count} Hisse Üzerinde Canlı Alım-Satım Simülasyonu Başlatılıyor...\n");
            Console.ResetColor();

            List<StockData> akanGecmis = new List<StockData>();
            var tumTarihler = loader.TumVeriler.Select(x => x.Tarih.Date).Distinct().OrderBy(t => t).ToList();

            foreach (var tarih in tumTarihler)
            {
                var oGununVerileri = loader.TumVeriler.Where(x => x.Tarih.Date == tarih).ToList();

                foreach (var gunVerisi in oGununVerileri)
                {
                    bot.KararVer(akanGecmis, gunVerisi);
                    akanGecmis.Add(gunVerisi);
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("                           SİMÜLASYON FİNAL BİLANÇOSU                     ");
            Console.WriteLine("==========================================================================");
            Console.ResetColor();

            decimal toplamHisseDegeri = 0m;
            foreach (var lot in cuzdan.Lotlar)
            {
                if (lot.Value > 0)
                {
                    var sonHisse = loader.TumVeriler.LastOrDefault(x => x.Sembol == lot.Key);
                    decimal sonFiyat = sonHisse != null ? sonHisse.Kapanis : 0m;
                    toplamHisseDegeri += lot.Value * sonFiyat;
                }
            }

            decimal toplamVarlik = cuzdan.Bakiye + toplamHisseDegeri;
            decimal netDegisim = toplamVarlik - bakiye;
            decimal yuzdeselDegisim = bakiye > 0 ? (netDegisim / bakiye) * 100m : 0m;

            Console.WriteLine($"   ► Operatör/Geliştirici : {kullaniciAdi}");
            Console.WriteLine($"   ► İşlenen Hisse Sayısı : {hisseGruplari.Count} Adet");
            Console.WriteLine($"   ► Seçilen Vade         : {vade} Analiz Modu");
            Console.WriteLine($"   ► Seçilen Strateji     : {tur} Modu");
            Console.WriteLine($"   ► Başlangıç Sermayesi  : {bakiye:N2} TL");
            Console.WriteLine("   -----------------------------------------------------------------------");
            Console.WriteLine($"   ► Kalan Nakit (Kasa)   : {cuzdan.Bakiye:N2} TL");
            Console.WriteLine($"   ► Portföy Hisse Değeri : {toplamHisseDegeri:N2} TL");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"   ► TOPLAM VARLIK        : {toplamVarlik:N2} TL");
            Console.ResetColor();
            Console.WriteLine("   -----------------------------------------------------------------------");

            if (netDegisim >= 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   ► NET PERFORMANS       : +{netDegisim:N2} TL (%{yuzdeselDegisim:F2} Kâr) 🚀");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   ► NET PERFORMANS       : {netDegisim:N2} TL (%{yuzdeselDegisim:F2} Zarar) 📉");
            }
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("   [0] Yeni Parametrelerle Yeniden Başlat (Seçenekler Ekranına Dön)");
            Console.WriteLine("   [ENTER] Programdan Çıkış Yap");
            Console.WriteLine("==========================================================================");
            Console.ResetColor();
            Console.Write("   Kararınız: ");

            string secim = Console.ReadLine();
            if (secim != "0")
            {
                devamEt = false;
            }
            else
            {
                Console.Clear();
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n   GravenAbyss Sistemden Güvenle Çıkış Yapıldı. İyi Günler!");
        Console.ResetColor();
    }
}