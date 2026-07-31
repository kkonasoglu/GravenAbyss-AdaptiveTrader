using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// ==========================================
// 1. STRATEJİ VE VERİ MODELLERİ
// ==========================================
public enum YatirimciTuru
{
    Garantici = 1,
    Dengeli = 2,
    AgresifRiskli = 3,
    BollingerBantlar = 4
}

public enum PiyasaTuru
{
    TurkiyeBIST = 1,
    AmerikaUS = 2,
    TumPiyasalar = 3
}
public enum VadeTuru { KisaVade = 1, OrtaVade = 2, UzunVade = 3 }

public class StockData
{
    public DateTime Tarih { get; set; }
    public string Sembol { get; set; }
    public decimal Kapanis { get; set; }
    public decimal Acilis { get; set; }
    public decimal Hacim { get; set; }
    public PiyasaTuru Piyasa { get; set; } = PiyasaTuru.TumPiyasalar;
}

public class MacroEvent
{
    public DateTime Tarih { get; set; }
    public int EtkiDerecesi { get; set; }
}

public class AlimAdayi
{
    public StockData GunVerisi { get; set; }
    public decimal RSI { get; set; }
    public decimal HacimOrani { get; set; }
    public decimal Skor { get; set; }
    public decimal ATR { get; set; }
    public decimal EMA200 { get; set; }
}

public class BekleyenEmir
{
    public AlimAdayi Aday { get; set; }
    public DateTime SinyalTarihi { get; set; }
    public decimal SinyalKapanisFiyati { get; set; }
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

        string turkKlasor = Path.Combine(klasorYolu, "Turk");
        string amerikanKlasor = Path.Combine(klasorYolu, "Amerikan");

        if (!Directory.Exists(klasorYolu)) Directory.CreateDirectory(klasorYolu);
        if (!Directory.Exists(turkKlasor)) Directory.CreateDirectory(turkKlasor);
        if (!Directory.Exists(amerikanKlasor)) Directory.CreateDirectory(amerikanKlasor);

        string[] csvDosyalari = Directory.GetFiles(klasorYolu, "*.csv", SearchOption.AllDirectories);

        if (csvDosyalari.Length == 0)
        {
            Console.WriteLine($"\n⚠️ UYARI: '{klasorYolu}' klasöründe veya alt klasörlerinde hiç CSV dosyası bulunamadı!");
            return;
        }

        string[] tarihFormatlari = new string[]
        {
            "yyyy-MM-dd", "MM/dd/yyyy", "dd.MM.yyyy", "dd/MM/yyyy",
            "M/d/yyyy", "d.M.yyyy", "yyyy/MM/dd", "dd-MM-yyyy", "yyyy.MM.dd"
        };

        var bistSemboller = new HashSet<string> { "THYAO", "THY", "GARAN", "GARANTİ", "TUPRAS", "TÜPRAŞ", "EREGL", "ERDEMİR", "KOC", "KOÇ", "SISE", "ŞİŞECAM", "SAHOL", "SABANCİ", "SOKM", "ŞOKM", "AKBNK", "ASELS", "BIMAS", "BİMAS", "CCOLA" };

        foreach (var dosyaYolu in csvDosyalari)
        {
            string varsayilanSembol = Path.GetFileNameWithoutExtension(dosyaYolu).ToUpper();
            if (varsayilanSembol.Contains("_")) varsayilanSembol = varsayilanSembol.Split('_')[0];
            if (varsayilanSembol.Contains(" ")) varsayilanSembol = varsayilanSembol.Split(' ')[0];

            PiyasaTuru dosyaPiyasa = PiyasaTuru.TumPiyasalar;
            if (dosyaYolu.IndexOf("Turk", StringComparison.OrdinalIgnoreCase) >= 0 || bistSemboller.Contains(varsayilanSembol))
            {
                dosyaPiyasa = PiyasaTuru.TurkiyeBIST;
            }
            else
            {
                dosyaPiyasa = PiyasaTuru.AmerikaUS;
            }

            var tumSatirlar = File.ReadAllLines(dosyaYolu);
            if (tumSatirlar.Length <= 1) continue;

            char ayirici = tumSatirlar[0].Contains(";") ? ';' : ',';
            var basliklar = SplitCsvLine(tumSatirlar[0], ayirici).Select(b => b.ToLower().Trim()).ToList();

            int tarihIdx = basliklar.FindIndex(b => b.Contains("date") || b.Contains("tarih"));
            int kapanisIdx = basliklar.FindIndex(b => b.Contains("price") || b.Contains("kapanis") || b.Contains("close") || b.Contains("şimdi") || b.Contains("son"));
            int acilisIdx = basliklar.FindIndex(b => b.Contains("open") || b.Contains("acilis") || b.Contains("açılış"));
            int hacimIdx = basliklar.FindIndex(b => b.Contains("vol") || b.Contains("hacim") || b.Contains("hac."));

            if (tarihIdx == -1) tarihIdx = 0;
            if (kapanisIdx == -1) kapanisIdx = 1;
            if (acilisIdx == -1) acilisIdx = kapanisIdx;

            foreach (var satir in tumSatirlar.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(satir)) continue;

                var s = SplitCsvLine(satir, ayirici);
                if (s.Length <= Math.Max(kapanisIdx, acilisIdx)) continue;

                string tarihStr = s[tarihIdx].Trim();
                DateTime tarih;

                bool tarihOkundu = DateTime.TryParse(tarihStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out tarih) ||
                                 DateTime.TryParseExact(tarihStr, tarihFormatlari, CultureInfo.InvariantCulture, DateTimeStyles.None, out tarih) ||
                                 DateTime.TryParse(tarihStr, new CultureInfo("tr-TR"), DateTimeStyles.None, out tarih);

                if (!tarihOkundu) continue;

                decimal kapanisFiyat = TemizleVeParseEt(s[kapanisIdx]);
                decimal acilisFiyat = acilisIdx != -1 ? TemizleVeParseEt(s[acilisIdx]) : kapanisFiyat;

                if (acilisFiyat <= 0m) acilisFiyat = kapanisFiyat;

                decimal hacimSayi = 0m;
                if (hacimIdx != -1 && s.Length > hacimIdx)
                {
                    string hacimStr = s[hacimIdx].Trim();
                    decimal carpan = 1m;
                    if (hacimStr.EndsWith("M", StringComparison.OrdinalIgnoreCase)) { carpan = 1000000m; hacimStr = hacimStr.Substring(0, hacimStr.Length - 1); }
                    else if (hacimStr.EndsWith("K", StringComparison.OrdinalIgnoreCase)) { carpan = 1000m; hacimStr = hacimStr.Substring(0, hacimStr.Length - 1); }
                    else if (hacimStr.EndsWith("B", StringComparison.OrdinalIgnoreCase)) { carpan = 1000000000m; hacimStr = hacimStr.Substring(0, hacimStr.Length - 1); }

                    hacimSayi = TemizleVeParseEt(hacimStr) * carpan;
                }

                if (kapanisFiyat > 0m)
                {
                    TumVeriler.Add(new StockData
                    {
                        Tarih = tarih,
                        Sembol = varsayilanSembol,
                        Kapanis = kapanisFiyat,
                        Acilis = acilisFiyat,
                        Hacim = hacimSayi,
                        Piyasa = dosyaPiyasa
                    });
                }
            }
        }

        TumVeriler = TumVeriler
            .GroupBy(x => new { x.Sembol, x.Tarih })
            .Select(g => g.First())
            .OrderBy(x => x.Tarih)
            .ToList();

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

    private string[] SplitCsvLine(string line, char delimiter)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == delimiter && !inQuotes)
            {
                result.Add(current.Trim());
                current = "";
            }
            else
            {
                current += c;
            }
        }
        result.Add(current.Trim());
        return result.ToArray();
    }

    private decimal TemizleVeParseEt(string veri)
    {
        if (string.IsNullOrWhiteSpace(veri) || veri == "-") return 0m;

        veri = veri.Trim().Replace("\"", "").Replace("TL", "").Replace("$", "").Replace("%", "");

        if (veri.Contains(",") && !veri.Contains("."))
        {
            if (decimal.TryParse(veri, NumberStyles.Any, new CultureInfo("tr-TR"), out decimal trSonuc))
                return trSonuc;
        }

        if (decimal.TryParse(veri, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal sonuc))
            return sonuc;

        if (decimal.TryParse(veri, NumberStyles.Any, new CultureInfo("tr-TR"), out sonuc))
            return sonuc;

        return 0m;
    }
}

// ==========================================
// 3. CÜZDAN VEYA SERMAYE YÖNETİCİSİ
public class PortfolioManager
{
    public decimal Bakiye { get; private set; }
    public decimal ToplamYatirilanSermaye { get; private set; }
    public decimal ToplamCekilenSermaye { get; private set; }
    public Dictionary<string, int> Lotlar = new Dictionary<string, int>();
    public Dictionary<string, decimal> Maliyetler = new Dictionary<string, decimal>();
    public Dictionary<string, decimal> EnYuksekFiyatlar = new Dictionary<string, decimal>();
    public List<decimal> IslemKarZararListesi = new List<decimal>();

    public PortfolioManager(decimal baslangic)
    {
        Bakiye = baslangic;
        ToplamYatirilanSermaye = baslangic;
        ToplamCekilenSermaye = 0m;
    }

    public void SermayeEkle(decimal miktar, DateTime tarih)
    {
        if (miktar <= 0) return;
        Bakiye += miktar;
        ToplamYatirilanSermaye += miktar;
        Console.WriteLine($"   💵 [SERMAYE EKLEMESİ] {tarih:yyyy-MM-dd} | Kasaya +{miktar:N2} TL Yatırıldı | Güncel Kasa: {Bakiye:N2} TL");
    }

    public void SermayeCekAkilli(decimal miktar, DateTime tarih, List<StockData> oGununVerileri)
    {
        if (miktar <= 0) return;

        // Kasada nakit yetersizse, kârda olan hisselerden satış yaparak nakit yarat!
        if (Bakiye < miktar && Lotlar.Count > 0)
        {
            decimal eksikNakit = miktar - Bakiye;
            Console.WriteLine($"   💡 [AKILLI NAKİT REBALANSI] {tarih:yyyy-MM-dd} | Çekim için {eksikNakit:N2} TL nakit yaratılıyor...");

            var eldekiKardakiHisseler = Lotlar.Where(x => x.Value > 0)
                .Select(x =>
                {
                    var gunVerisi = oGununVerileri?.FirstOrDefault(g => g.Sembol == x.Key);
                    decimal sonFiyat = gunVerisi != null ? gunVerisi.Kapanis : (Maliyetler.ContainsKey(x.Key) ? Maliyetler[x.Key] : 0m);
                    decimal maliyet = Maliyetler.ContainsKey(x.Key) ? Maliyetler[x.Key] : sonFiyat;
                    decimal karOrani = maliyet > 0 ? (sonFiyat - maliyet) / maliyet : 0m;
                    return new { Sembol = x.Key, Lot = x.Value, SonFiyat = sonFiyat, KarOrani = karOrani };
                })
                .OrderByDescending(x => x.KarOrani)
                .ToList();

            foreach (var h in eldekiKardakiHisseler)
            {
                if (Bakiye >= miktar) break;
                if (h.SonFiyat <= 0) continue;

                decimal gerekenKapanis = miktar - Bakiye;
                int satilacakLot = (int)Math.Ceiling(gerekenKapanis / (h.SonFiyat * 0.999m));
                satilacakLot = Math.Min(satilacakLot, h.Lot);

                if (satilacakLot > 0)
                {
                    Sat(h.Sembol, h.SonFiyat, satilacakLot, tarih, $"🏧 Nakit Çekim Rebalansı (%{h.KarOrani * 100:F1} Kârda)");
                }
            }
        }

        SermayeCek(miktar, tarih);
    }
    public void SermayeCek(decimal miktar, DateTime tarih)
    {
        if (miktar <= 0) return;

        if (Bakiye >= miktar)
        {
            Bakiye -= miktar;
            ToplamCekilenSermaye += miktar;
            Console.WriteLine($"   🏧 [AYLIK NAKİT ÇEKİMİ] {tarih:yyyy-MM-dd} | Kasadan -{miktar:N2} TL Çekildi | Kalan Kasa: {Bakiye:N2} TL | Toplam Çekilen: {ToplamCekilenSermaye:N2} TL");
        }
        else
        {
            decimal cekilen = Bakiye;
            Bakiye = 0m;
            ToplamCekilenSermaye += cekilen;
            Console.WriteLine($"   ⚠️ [YETERSİZ NAKİT ÇEKİMİ] {tarih:yyyy-MM-dd} | İstenen: {miktar:N2} TL | Mevcut Nakit Çekildi: -{cekilen:N2} TL | Kasa Boşaldı!");
        }
    }

    public void Al(string sembol, decimal fiyat, int lot, DateTime tarih)
    {
        if (fiyat <= 0m || lot <= 0) return;

        decimal maliyet = (fiyat * lot) * 1.001m;
        if (Bakiye < maliyet)
        {
            lot = (int)Math.Floor(Bakiye / (fiyat * 1.001m));
            maliyet = (fiyat * lot) * 1.001m;
        }

        if (lot <= 0 || Bakiye < maliyet) return;

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
        int eskiLot = Lotlar[sembol];
        Lotlar[sembol] += lot;

        if (eskiLot == 0 || !EnYuksekFiyatlar.ContainsKey(sembol) || fiyat > EnYuksekFiyatlar[sembol])
        {
            EnYuksekFiyatlar[sembol] = fiyat;
        }

        Console.WriteLine($"   [AL - T+1 Açılış] {tarih:yyyy-MM-dd} | {sembol,-10} | {lot,6} Lot | Fiyat: {fiyat,7:F2} | Maliyet: {Maliyetler[sembol],7:F2}");
    }

    public void Sat(string sembol, decimal fiyat, int lot, DateTime tarih, string t)
    {
        if (lot <= 0 || fiyat <= 0m || !Lotlar.ContainsKey(sembol) || Lotlar[sembol] <= 0) return;

        if (lot > Lotlar[sembol])
        {
            lot = Lotlar[sembol];
        }

        decimal birimMaliyet = Maliyetler.ContainsKey(sembol) ? Maliyetler[sembol] : fiyat;
        decimal netGelir = (fiyat * lot) * 0.999m;
        decimal netMaliyet = (birimMaliyet * lot) * 1.001m;
        decimal karZarar = netGelir - netMaliyet;

        IslemKarZararListesi.Add(karZarar);

        Bakiye += netGelir;
        Lotlar[sembol] -= lot;

        if (Lotlar[sembol] <= 0)
        {
            Maliyetler.Remove(sembol);
            EnYuksekFiyatlar.Remove(sembol);
        }

        Console.WriteLine($"   [SAT]             {tarih:yyyy-MM-dd} | {sembol,-10} | {lot,6} Lot | Fiyat: {fiyat,7:F2} | PnL: {(karZarar >= 0 ? "+" : "")}{karZarar:N2} TL | {t}");
    }
}

public class TradingBot
{
    private PortfolioManager _cuzdan;
    private YatirimciTuru _tur;
    private VadeTuru _vade;
    private PiyasaTuru _piyasa;
    private List<MacroEvent> _makro;
    private int _pencereGunSayisi;
    private HashSet<string> _islenenSplitler = new HashSet<string>();
    private Dictionary<string, DateTime> _sonStopTarihleri = new Dictionary<string, DateTime>();
    private Dictionary<string, DateTime> _sonSplitTarihleri = new Dictionary<string, DateTime>();

    public TradingBot(PortfolioManager p, YatirimciTuru t, VadeTuru v, List<MacroEvent> m, PiyasaTuru piyasa = PiyasaTuru.TumPiyasalar)
    {

        _cuzdan = p;
        _tur = t;
        _vade = v;
        _makro = m;
        _piyasa = piyasa;

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
        if (hisseGecmisi == null || hisseGecmisi.Count < periyot + 1) return 50m;

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

    public decimal HesaplaEMA(List<StockData> hisseGecmisi, int periyot)
    {
        if (hisseGecmisi == null || hisseGecmisi.Count == 0) return 0m;
        if (hisseGecmisi.Count < periyot) periyot = hisseGecmisi.Count;

        decimal k = 2m / (periyot + 1);
        decimal ema = hisseGecmisi.Take(periyot).Average(x => x.Kapanis);
        foreach (var d in hisseGecmisi.Skip(periyot))
        {
            ema = (d.Kapanis * k) + (ema * (1m - k));
        }
        return ema;
    }

    public decimal HesaplaATR(List<StockData> hisseGecmisi, int periyot = 14)
    {
        if (hisseGecmisi == null || hisseGecmisi.Count <= 1) return 0m;
        List<decimal> trList = new List<decimal>();
        for (int i = 1; i < hisseGecmisi.Count; i++)
        {
            decimal tr = Math.Max(
                Math.Abs(hisseGecmisi[i].Kapanis - hisseGecmisi[i].Acilis),
                Math.Abs(hisseGecmisi[i].Kapanis - hisseGecmisi[i - 1].Kapanis)
            );
            trList.Add(tr);
        }
        if (trList.Count == 0) return 0m;
        int p = Math.Min(periyot, trList.Count);
        return trList.Skip(trList.Count - p).Average();
    }

    public void SatisKontroluVeZirveGuncelle(List<StockData> tumGecmis, StockData bugun)
    {
        if (bugun.Kapanis <= 0m) return;

        var hisseGecmisi = tumGecmis.Where(x => x.Sembol == bugun.Sembol && x.Kapanis > 0m).ToList();
        int periyot = _tur == YatirimciTuru.Garantici ? 21 : (_tur == YatirimciTuru.Dengeli ? 14 : (_tur == YatirimciTuru.AgresifRiskli ? 9 : 20));

        decimal atr = HesaplaATR(hisseGecmisi, 14);
        decimal stopYuzdesi;
        if (_piyasa == PiyasaTuru.AmerikaUS)
        {
            // ABD Piyasası (Yüksek volatilite ve yüksek nominal fiyatlar için esnetilmiş stoplar)
            stopYuzdesi = _tur == YatirimciTuru.Garantici ? 0.06m : (_tur == YatirimciTuru.Dengeli ? 0.12m : (_tur == YatirimciTuru.AgresifRiskli ? 0.15m : 0.08m));
        }
        else if (_piyasa == PiyasaTuru.TurkiyeBIST)
        {
            // BİST Piyasası (%10 günlük tavan/taban kısıtı gereği tüm stoplar kesinlikle %9.0 altındadır!)
            stopYuzdesi = _tur == YatirimciTuru.Garantici ? 0.04m : (_tur == YatirimciTuru.Dengeli ? 0.07m : (_tur == YatirimciTuru.AgresifRiskli ? 0.085m : 0.05m));
        }
        else
        {
            // Genel Hibrit Portföy
            stopYuzdesi = _tur == YatirimciTuru.Garantici ? 0.05m : (_tur == YatirimciTuru.Dengeli ? 0.10m : (_tur == YatirimciTuru.AgresifRiskli ? 0.15m : 0.06m));
        }

        if (hisseGecmisi.Count <= periyot) return;
        string anlikKarakter = HesaplaAnlikPiyasaKarakteri(hisseGecmisi);

        if (_cuzdan.Lotlar.ContainsKey(bugun.Sembol) && _cuzdan.Lotlar[bugun.Sembol] > 0)
        {
            decimal alisFiyati = _cuzdan.Maliyetler.ContainsKey(bugun.Sembol) ? _cuzdan.Maliyetler[bugun.Sembol] : bugun.Kapanis;

            // ✂️ AKILLI MALİYET BAZLI STOK SPLİT DÜZELTMESİ (%25+ Gerçek Bölünme Boşlukları Otomatik Revize Edilir)
            if (alisFiyati > 0m && bugun.Kapanis < alisFiyati * 0.75m)
            {
                decimal splitOrani = alisFiyati / bugun.Kapanis;
                int yeniLot = (int)Math.Round(_cuzdan.Lotlar[bugun.Sembol] * splitOrani);

                _cuzdan.Maliyetler[bugun.Sembol] = bugun.Kapanis; // Maliyeti güncel fiyata uyarla
                _cuzdan.EnYuksekFiyatlar[bugun.Sembol] = bugun.Kapanis; // Zirveyi resetle
                _cuzdan.Lotlar[bugun.Sembol] = yeniLot;
                _sonSplitTarihleri[bugun.Sembol] = bugun.Tarih;

                Console.WriteLine($"   ✂️ [STOK SPLİT DÜZELTMESİ] {bugun.Tarih:yyyy-MM-dd} | {bugun.Sembol,-10} 1:{splitOrani:F2} Bölündü! Yeni Lot: {yeniLot}, Revize Maliyet: {bugun.Kapanis:F2} TL");

                alisFiyati = bugun.Kapanis;
            }

            if (_sonSplitTarihleri.ContainsKey(bugun.Sembol) && (bugun.Tarih - _sonSplitTarihleri[bugun.Sembol]).TotalDays <= 3)
            {
                return; // ✂️ Bölünme sonrası veri oturma süreci (3 Gün Grace Period)
            }

            if (!_cuzdan.EnYuksekFiyatlar.ContainsKey(bugun.Sembol) || bugun.Kapanis > _cuzdan.EnYuksekFiyatlar[bugun.Sembol])
            {
                _cuzdan.EnYuksekFiyatlar[bugun.Sembol] = bugun.Kapanis;
            }

            decimal zirveFiyat = _cuzdan.EnYuksekFiyatlar[bugun.Sembol];
            decimal ema20 = HesaplaEMA(hisseGecmisi, 20);
            decimal ema50 = HesaplaEMA(hisseGecmisi, 50);

            // 🎯 RİSK PROFİLİ TABANLI DİNAMİK TREND VE ATR STOPU
            decimal ilkHardStop = alisFiyati * (1m - stopYuzdesi);
            decimal trailingStop = zirveFiyat * (1m - stopYuzdesi);
            decimal chandelierStop = zirveFiyat - (2.5m * (atr > 0m ? atr : zirveFiyat * stopYuzdesi));
            decimal nihaiStop = (zirveFiyat >= alisFiyati * (1m + stopYuzdesi)) ? Math.Max(trailingStop, chandelierStop) : ilkHardStop;

            bool trendBitisSinyali = (zirveFiyat >= alisFiyati * (1m + stopYuzdesi)) && (bugun.Kapanis < ema50 && bugun.Kapanis < ema20);

            if (bugun.Kapanis <= nihaiStop || trendBitisSinyali)
            {
                string etiket = (zirveFiyat >= alisFiyati * (1m + stopYuzdesi))
                    ? $"🎯 KÂR İZLEYEN STOP (Zirve: {zirveFiyat:F2} TL | Maliyet: {alisFiyati:F2} TL)"
                    : $"🛡️ ATR KORUMA STOPU (Maliyet: {alisFiyati:F2} TL)";

                _cuzdan.Sat(bugun.Sembol, bugun.Kapanis, _cuzdan.Lotlar[bugun.Sembol], bugun.Tarih, etiket);
                _sonStopTarihleri[bugun.Sembol] = bugun.Tarih;
                return;
            }
        }
    }

    public AlimAdayi AlimSinyaliVeSkorHesapla(List<StockData> tumGecmis, StockData bugun)
    {
        var hisseGecmisi = tumGecmis.Where(x => x.Sembol == bugun.Sembol && x.Kapanis > 0m).ToList();
        if (hisseGecmisi == null || hisseGecmisi.Count < 20) return null;

        if (bugun.Kapanis <= 0m) return null;

        int cooldownGun = _tur == YatirimciTuru.Garantici ? 15 : (_tur == YatirimciTuru.Dengeli ? 10 : 5);

        if (_sonStopTarihleri.ContainsKey(bugun.Sembol))
        {
            double gecenGun = (bugun.Tarih - _sonStopTarihleri[bugun.Sembol]).TotalDays;
            if (gecenGun < cooldownGun) return null;
        }

        var onceki20Gun = hisseGecmisi.Take(hisseGecmisi.Count - 1).Skip(Math.Max(0, hisseGecmisi.Count - 21)).Take(20).ToList();
        if (onceki20Gun.Count < 5) return null;

        decimal oncekiMax20GunFiyati = onceki20Gun.Max(x => x.Kapanis);

        var son10Gun = onceki20Gun.Skip(Math.Max(0, onceki20Gun.Count - 10)).Take(10).ToList();
        decimal ortalamaHacim = son10Gun.Count > 0 ? son10Gun.Average(x => x.Hacim) : 0m;

        decimal hacimOrani = ortalamaHacim > 0 ? (bugun.Hacim / ortalamaHacim) : 1m;

        int tolerans = _tur == YatirimciTuru.AgresifRiskli ? 3 : 2;
        bool riskliOlay = _makro.Any(m => m.Tarih.Date == bugun.Tarih.Date && m.EtkiDerecesi >= tolerans);

        int periyot = _tur == YatirimciTuru.Garantici ? 21 : (_tur == YatirimciTuru.Dengeli ? 14 : 9);
        decimal alEsik = _tur == YatirimciTuru.Garantici ? 38m : (_tur == YatirimciTuru.Dengeli ? 35m : 30m);

        decimal rsi = HesaplaRSI(hisseGecmisi, periyot);
        decimal ema20 = HesaplaEMA(hisseGecmisi, Math.Min(20, hisseGecmisi.Count));
        decimal ema50 = HesaplaEMA(hisseGecmisi, Math.Min(50, hisseGecmisi.Count));
        decimal ema200 = HesaplaEMA(hisseGecmisi, Math.Min(200, hisseGecmisi.Count));
        decimal atr = HesaplaATR(hisseGecmisi, 14);

        string makroRejim = HesaplaAnlikPiyasaKarakteri(hisseGecmisi, _pencereGunSayisi);
        bool bogaRejimMi = makroRejim.Contains("Boğa");

        // 🎯 DISIPLINLI BOĞA TRENDİ: Giriş sadece Boğa Piyasası Rejiminde yapılmalıdır (Yatay/Testere kırılımları elenir)!
        bool donchianBreakout = bugun.Kapanis > oncekiMax20GunFiyati;
        bool bogaTrendOnayi = (ema20 >= ema50) && (bugun.Kapanis >= ema20) && (bugun.Kapanis >= ema50) && (ema200 == 0m || bugun.Kapanis >= ema200);
        bool hacimOnayli = bugun.Hacim > ortalamaHacim || ortalamaHacim == 0m;
        bool rsiUygun = rsi <= 72m;

        if (donchianBreakout && bogaTrendOnayi && hacimOnayli && rsiUygun && !riskliOlay && bogaRejimMi)
        {
            decimal breakoutSkoru = 40m;
            decimal hacimSkoru = Math.Min(hacimOrani * 20m, 40m);
            decimal trendBonusu = (ema20 > ema50) ? 20m : 0m;

            return new AlimAdayi
            {
                GunVerisi = bugun,
                RSI = rsi,
                HacimOrani = hacimOrani,
                ATR = atr,
                EMA200 = ema200,
                Skor = breakoutSkoru + hacimSkoru + trendBonusu
            };
        }

        return null;
    }

    public void IslemYapTPlus1(BekleyenEmir emir, StockData bugunAcilisVerisi, List<StockData> tumGecmis)
    {
        decimal dunKapanis = emir.SinyalKapanisFiyati;
        decimal bugunAcilis = bugunAcilisVerisi.Acilis > 0 ? bugunAcilisVerisi.Acilis : bugunAcilisVerisi.Kapanis;

        var hisseGecmisi = tumGecmis.Where(x => x.Sembol == bugunAcilisVerisi.Sembol && x.Kapanis > 0m).ToList();
        string anlikKarakter = HesaplaAnlikPiyasaKarakteri(hisseGecmisi);
        decimal maxGapYuzdesi = 1.095m; // BİST %10 Tavan Sınırı Kalkanı (%9.5 üzeri tavan kilitli alım engeli)
        if (_piyasa == PiyasaTuru.AmerikaUS || bugunAcilisVerisi.Piyasa == PiyasaTuru.AmerikaUS)
        {
            maxGapYuzdesi = anlikKarakter.Contains("Boğa") ? 1.15m : 1.05m;
        }

        if (bugunAcilis > dunKapanis * maxGapYuzdesi)
        {
            Console.WriteLine($"   [İPTAL - Tavan/Gap Up Kalkanı] {bugunAcilisVerisi.Tarih:yyyy-MM-dd} | {emir.Aday.GunVerisi.Sembol,-10} | Dün: {dunKapanis:F2} TL -> Bugün Açılış: {bugunAcilis:F2} TL");
            return;
        }

        // 🎯 RİSK TABANLI VE SERMAYE ETKİNLİKLİ POZİSYON BOYUTLANDIRMASI (Dengeli: %40, Agresif: %60 Katlama Bütçesi)
        decimal eldekiHisseDegeri = 0m;
        foreach (var lot in _cuzdan.Lotlar)
        {
            if (lot.Value > 0)
            {
                var sonH = tumGecmis.Where(x => x.Sembol == lot.Key).LastOrDefault();
                decimal f = sonH != null ? sonH.Kapanis : (_cuzdan.Maliyetler.ContainsKey(lot.Key) ? _cuzdan.Maliyetler[lot.Key] : 0m);
                eldekiHisseDegeri += lot.Value * f;
            }
        }
        decimal toplamVarlik = _cuzdan.Bakiye + eldekiHisseDegeri;
        decimal maxPozisyonYuzdesi = _tur == YatirimciTuru.Garantici ? 0.35m : (_tur == YatirimciTuru.Dengeli ? 0.70m : 0.90m);
        decimal maxPozisyonButcesi = toplamVarlik * maxPozisyonYuzdesi;
        decimal ayrilacakButce = Math.Min(_cuzdan.Bakiye, maxPozisyonButcesi);

        int alinacakLot = (int)Math.Floor(ayrilacakButce / (bugunAcilis * 1.001m));
        if (alinacakLot > 0)
        {
            _cuzdan.Al(emir.Aday.GunVerisi.Sembol, bugunAcilis, alinacakLot, bugunAcilisVerisi.Tarih);
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
        try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { }
        Console.Title = "GravenAbyss - Multi-Asset Backtest Simulation Engine";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==========================================================================");
        Console.WriteLine("   🚀 GRAVENABYSS MULTI-ASSET PAPER-TRADING ALGORİTMİK YATIRIM SİSTEMİ ");
        Console.WriteLine("==========================================================================");
        Console.WriteLine("   Sistem Başarıyla Başlatıldı. Hoş Geldiniz!");
        Console.ResetColor();

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

        var simYillari = loader.TumVeriler.Select(x => x.Tarih.Year).Where(y => y >= 2025).Distinct().OrderBy(y => y).ToList();
        if (simYillari.Count == 0) simYillari.Add(2025);

        bool devamEt = true;

        while (devamEt)
        {
            decimal bakiye = 0m;
            decimal aylikEkleme = 0m;
            decimal aylikCekim = 0m;
            VadeTuru vade = VadeTuru.OrtaVade;
            YatirimciTuru tur = YatirimciTuru.Dengeli;
            PiyasaTuru piyasa = PiyasaTuru.TumPiyasalar;
            bool parametrelerTamam = false;
            bool yilSonuPozisyonlariTasi = true;


            while (!parametrelerTamam)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("   ⚙️ SİMÜLASYON PARAMETRE EKRANI (İptal / Baştan Başlamak İçin: '0')");
                Console.WriteLine("==========================================================================");
                Console.ResetColor();

                // 0. Borsa ve Piyasa Seçimi
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n   --- BORSA VE PİYASA SEÇİMİ ---");
                Console.WriteLine("   [1] Türkiye Borsa İstanbul (BİST - TL Cinsinden | Tavan/Taban Limitli 🇹🇷)");
                Console.WriteLine("   [2] Amerika Piyasası (NASDAQ / NYSE - Dolar Cinsinden | Yüksek Volatilite 🇺🇸)");
                Console.WriteLine("   [3] Tümü / Hibrit Portföy (Çoklu Piyasa Tarama 🌍)");
                Console.WriteLine("   [0] Baştan Başla / Geri Dön");
                Console.ResetColor();
                Console.Write("   Seçiminiz: ");
                string piyasaGirdi = Console.ReadLine()?.Trim();
                if (piyasaGirdi == "0") continue;

                int piyasaSecim;
                while (!int.TryParse(piyasaGirdi, out piyasaSecim) || piyasaSecim < 1 || piyasaSecim > 3)
                {
                    Console.Write("   Lütfen 1, 2 veya 3 seçiniz [0: Geri]: ");
                    piyasaGirdi = Console.ReadLine()?.Trim();
                    if (piyasaGirdi == "0") break;
                }
                if (piyasaGirdi == "0") continue;
                piyasa = (PiyasaTuru)piyasaSecim;

                // 1. Başlangıç Bakiyesi
                Console.Write("\n   ► Başlangıç Bakiyesi Giriniz (TL) [0: Sıfırla]: ");
                string bakiyeGirdi = Console.ReadLine()?.Trim();
                if (bakiyeGirdi == "0") continue;

                while (!decimal.TryParse(bakiyeGirdi, out bakiye) || bakiye <= 0)
                {
                    Console.Write("   Lütfen geçerli bir bakiye giriniz (TL) [0: Sıfırla]: ");
                    bakiyeGirdi = Console.ReadLine()?.Trim();
                    if (bakiyeGirdi == "0") break;
                }
                if (bakiyeGirdi == "0") continue;

                // 2. Nakit Akışı
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n   --- AYLIK NAKİT AKIŞI TERCİHİ ---");
                Console.WriteLine("   [1] Her Ay Kasaya Para EKLE (Birikim Modu - DCA)");
                Console.WriteLine("   [2] Her Ay Kasadan Para ÇEK  (Düzenli Gelir / Maaş Modu)");
                Console.WriteLine("   [3] Sabit Bakiye (Ekleme / Çekme Yapma)");
                Console.WriteLine("   [0] Baştan Başla / Geri Dön");
                Console.ResetColor();
                Console.Write("   Seçiminiz: ");
                string akisGirdi = Console.ReadLine()?.Trim();
                if (akisGirdi == "0") continue;

                int akisSecim;
                while (!int.TryParse(akisGirdi, out akisSecim) || akisSecim < 1 || akisSecim > 3)
                {
                    Console.Write("   Lütfen 1, 2 veya 3 seçiniz [0: Geri]: ");
                    akisGirdi = Console.ReadLine()?.Trim();
                    if (akisGirdi == "0") break;
                }
                if (akisGirdi == "0") continue;

                aylikEkleme = 0m;
                aylikCekim = 0m;

                if (akisSecim == 1)
                {
                    Console.Write("   Her Ay Kasaya Eklenecek Miktar (TL) [0: Geri]: ");
                    string eklemeGirdi = Console.ReadLine()?.Trim();
                    if (eklemeGirdi == "0") continue;
                    while (!decimal.TryParse(eklemeGirdi, out aylikEkleme) || aylikEkleme < 0)
                    {
                        Console.Write("   Geçerli bir miktar girin [0: Geri]: ");
                        eklemeGirdi = Console.ReadLine()?.Trim();
                        if (eklemeGirdi == "0") break;
                    }
                    if (eklemeGirdi == "0") continue;
                }
                else if (akisSecim == 2)
                {
                    Console.Write("   Her Ay Kasadan Çekilecek Miktar (TL) [0: Geri]: ");
                    string cekimGirdi = Console.ReadLine()?.Trim();
                    if (cekimGirdi == "0") continue;
                    while (!decimal.TryParse(cekimGirdi, out aylikCekim) || aylikCekim < 0)
                    {
                        Console.Write("   Geçerli bir miktar girin [0: Geri]: ");
                        cekimGirdi = Console.ReadLine()?.Trim();
                        if (cekimGirdi == "0") break;
                    }
                    if (cekimGirdi == "0") continue;
                }

                // 3. Yatırım Vadesi
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n   --- YATIRIM VADESİ SEÇİMİ ---");
                Console.WriteLine("   [1] Kısa Vade (20 Günlük Rejim Analizi  ~ 1 Ay)");
                Console.WriteLine("   [2] Orta Vade (60 Günlük Rejim Analizi  ~ 3 Ay - İdeal)");
                Console.WriteLine("   [3] Uzun Vade (200 Günlük Rejim Analizi ~ 1 Yıl)");
                Console.WriteLine("   [0] Baştan Başla / Geri Dön");
                Console.ResetColor();
                Console.Write("   Seçiminiz: ");
                string vadeGirdi = Console.ReadLine()?.Trim();
                if (vadeGirdi == "0") continue;

                int vadeSecim;
                while (!int.TryParse(vadeGirdi, out vadeSecim) || vadeSecim < 1 || vadeSecim > 3)
                {
                    Console.Write("   Lütfen 1, 2 veya 3 seçiniz [0: Geri]: ");
                    vadeGirdi = Console.ReadLine()?.Trim();
                    if (vadeGirdi == "0") break;
                }
                if (vadeGirdi == "0") continue;
                vade = (VadeTuru)vadeSecim;

                // 4. Strateji / Risk Profili
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n   --- STRATEJİ / RİSK PROFİLİ VE BÜTÇE SEÇİMİ ---");
                Console.WriteLine("   [1] Garantici Mod      (Bütçe: %35 | Stop: %5 | Defansif)");
                Console.WriteLine("   [2] Dengeli Mod        (Bütçe: %70 | Stop: %10 | Optimum 🔥)");
                Console.WriteLine("   [3] Agresif / Riskli   (Bütçe: %90 | Stop: %15 | MAKSİMUM KÂR & KATLAMA 🚀)");
                Console.WriteLine("   [4] Bollinger Bantları (Alt Bant AL | Üst Bant SAT)");
                Console.WriteLine("   [0] Baştan Başla / Geri Dön");
                Console.ResetColor();
                Console.Write("   Seçiminiz: ");
                string turGirdi = Console.ReadLine()?.Trim();
                if (turGirdi == "0") continue;

                int turSecim;
                while (!int.TryParse(turGirdi, out turSecim) || turSecim < 1 || turSecim > 4)
                {
                    Console.Write("   Lütfen 1, 2, 3 veya 4 seçiniz [0: Geri]: ");
                    turGirdi = Console.ReadLine()?.Trim();
                    if (turGirdi == "0") break;
                }
                if (turGirdi == "0") continue;
                tur = (YatirimciTuru)turSecim;

                // 5. Yıl Sonu Pozisyon Taşıma Tercihi
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n   --- YIL SONU POZİSYON TAŞIMA TERCİHİ ---");
                Console.WriteLine("   [1] Kârlı Hisseleri Yeni Yıla TAŞI (ÖNERİLEN - Kesintisiz Bileşik Getiri 🚀)");
                Console.WriteLine("   [2] Yıl Sonunda Tüm Hisseleri SAT  (Her Yıl Başı Nakit Portföy Sıfırla 💰)");
                Console.WriteLine("   [0] Baştan Başla / Geri Dön");
                Console.ResetColor();
                Console.Write("   Seçiminiz: ");
                string tasiGirdi = Console.ReadLine()?.Trim();
                if (tasiGirdi == "0") continue;

                int tasiSecim;
                while (!int.TryParse(tasiGirdi, out tasiSecim) || (tasiSecim != 1 && tasiSecim != 2))
                {
                    Console.Write("   Lütfen 1 veya 2 seçiniz [0: Geri]: ");
                    tasiGirdi = Console.ReadLine()?.Trim();
                    if (tasiGirdi == "0") break;
                }
                if (tasiGirdi == "0") continue;
                yilSonuPozisyonlariTasi = (tasiSecim == 1);

                parametrelerTamam = true;
            }

            PortfolioManager cuzdan = new PortfolioManager(bakiye);
            TradingBot bot = new TradingBot(cuzdan, tur, vade, loader.MakroOlaylar, piyasa);

            List<StockData> akanGecmis = new List<StockData>();
            List<BekleyenEmir> bekleyenEmirler = new List<BekleyenEmir>();
            List<decimal> gunlukPortfoyDegerleri = new List<decimal>();

            var secilenSemboller = new HashSet<string>();
            int sonIslenenAy = -1;
            decimal peakPortfoyVarligi = 0m;
            int devreKesiciKalanGun = 0;
            bool bastanBaslaGeri = false;

            var bistSemboller = new HashSet<string> { "THYAO", "THY", "GARAN", "GARANTİ", "TUPRAS", "TÜPRAŞ", "EREGL", "ERDEMİR", "KOC", "KOÇ", "SISE", "ŞİŞECAM", "SAHOL", "SABANCİ", "SOKM", "ŞOKM", "AKBNK", "ASELS", "BIMAS", "BİMAS", "CCOLA" };
            foreach (var simYili in simYillari)
            {
                if (bastanBaslaGeri) break;

                // YIL BAŞI HİSSE ANALİZİ VE İNTERAKTİF SEÇİM EKRANI
                int analizYili = simYili - 1;
                var gecmisYilVerileri = loader.TumVeriler.Where(x => x.Tarih.Year == analizYili).ToList();

                if (piyasa == PiyasaTuru.TurkiyeBIST)
                {
                    gecmisYilVerileri = gecmisYilVerileri.Where(x => bistSemboller.Contains(x.Sembol.ToUpper())).ToList();
                }
                else if (piyasa == PiyasaTuru.AmerikaUS)
                {
                    gecmisYilVerileri = gecmisYilVerileri.Where(x => !bistSemboller.Contains(x.Sembol.ToUpper())).ToList();
                }

                var hisseGruplari = gecmisYilVerileri.GroupBy(x => x.Sembol).ToList();

                if (hisseGruplari.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n==========================================================================");
                    Console.WriteLine($"   📁 {analizYili} YILI PERFORMANSINA GÖRE {hisseGruplari.Count} HİSSE ANALİZ EDİLİYOR...");
                    Console.WriteLine("==========================================================================");

                    var hisseSkorlari = new List<(string Sembol, decimal Getiri, decimal KompozitSkor, string Rejim)>();

                    foreach (var grup in hisseGruplari)
                    {
                        var hisseVerileri = grup.OrderBy(x => x.Tarih).ToList();
                        if (hisseVerileri.Count < 10) continue;

                        decimal ilkFiyat = hisseVerileri.First().Kapanis;
                        decimal sonFiyat = hisseVerileri.Last().Kapanis;
                        decimal getiri = ilkFiyat > 0 ? ((sonFiyat - ilkFiyat) / ilkFiyat) * 100m : 0m;

                        decimal zirve = 0m;
                        decimal maxDD = 0m;
                        int emaUzeriGun = 0;
                        decimal ema50 = bot.HesaplaEMA(hisseVerileri, Math.Min(50, hisseVerileri.Count));

                        decimal oncekiKapanis = 0m;
                        foreach (var d in hisseVerileri)
                        {
                            if (oncekiKapanis > 0m && d.Kapanis < oncekiKapanis * 0.78m)
                            {
                                zirve = d.Kapanis; // Split gapini duzelt, suni maxDD olusmasin
                            }
                            else if (d.Kapanis > zirve)
                            {
                                zirve = d.Kapanis;
                            }
                            decimal dd = zirve > 0 ? ((zirve - d.Kapanis) / zirve) * 100m : 0m;
                            if (dd > maxDD) maxDD = dd;
                            if (d.Kapanis >= ema50) emaUzeriGun++;
                            oncekiKapanis = d.Kapanis;
                        }
                        decimal trendKalmaOrani = (decimal)emaUzeriGun / Math.Max(1, hisseVerileri.Count);
                        decimal kompozitSkor = (getiri * (1m + trendKalmaOrani)) / Math.Max(1.0m, Math.Min(maxDD, 35m));

                        // 🛡️ AŞIRI DİK RALLİ (MEAN REVERSION) TUZAĞI KORUMASI:
                        // 1 yılda %80+ prim yapan hisseler ertesi yıl kâr satışına ve dinlenmeye çekilir.
                        // İstikrarlı bileşik büyüyen hisseleri (%20-%70 primlileri) ön plana çıkarmak için aşırı prim düzeltmesi yapılır.
                        if (getiri > 80m)
                        {
                            kompozitSkor *= 0.35m;
                        }
                        else if (getiri < 0m)
                        {
                            kompozitSkor *= 0.50m;
                        }

                        string rejim = bot.HesaplaAnlikPiyasaKarakteri(hisseVerileri, 250);

                        hisseSkorlari.Add((grup.Key, getiri, kompozitSkor, rejim));
                    }

                    var siraliHisseler = hisseSkorlari.OrderByDescending(x => x.KompozitSkor).ToList();

                    for (int i = 0; i < siraliHisseler.Count; i++)
                    {
                        var h = siraliHisseler[i];
                        Console.WriteLine($"   [{i + 1,2}] Sembol: {h.Sembol,-10} | {analizYili} Getiri: %{h.Getiri,6:F1} | Sağlık Skoru: {h.KompozitSkor,6:F2} | Rejim: {h.Rejim}");
                    }
                    Console.WriteLine("==========================================================================");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n   💡 ALGORİTMİK İPUCU: Sermayeyi katlamak için listelenen EN GÜÇLÜ 3 ila 5 hisseye odaklanmanız tavsiye edilir! (10+ hisse seçmek bütçeyi mikro lotlara böler)");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"   ► {simYili} Yılı İçin {analizYili} Performansına Göre EN İYİ Kaç Hisse Seçilsin? (1 - {siraliHisseler.Count}) [Önerilen: 5] [0: Baştan Başla]: ");
                    string hisseSayiGirdi = Console.ReadLine()?.Trim();
                    if (hisseSayiGirdi == "0")
                    {
                        Console.WriteLine("   🔄 Parametre Seçim Ekranına Geri Dönülüyor...");
                        bastanBaslaGeri = true;
                        break;
                    }

                    int secilecekHisseSayisi;
                    while (!int.TryParse(hisseSayiGirdi, out secilecekHisseSayisi) || secilecekHisseSayisi < 1 || secilecekHisseSayisi > siraliHisseler.Count)
                    {
                        Console.Write($"   Lütfen 1 ile {siraliHisseler.Count} arasında bir sayı giriniz [0: Baştan Başla]: ");
                        hisseSayiGirdi = Console.ReadLine()?.Trim();
                        if (hisseSayiGirdi == "0")
                        {
                            bastanBaslaGeri = true;
                            break;
                        }
                    }
                    if (bastanBaslaGeri) break;

                    secilenSemboller = new HashSet<string>(siraliHisseler.Take(secilecekHisseSayisi).Select(x => x.Sembol));

                    // 🧠 YIL BAŞI AKILLI REBALANS: Sağlık Skoru 1.50'nin altında kalan veya portföy dışı kalan eldeki hisseler satılır.
                    // Sağlık Skoru >= 1.50 olan güçlü hisseler SATILMAZ, kesintisiz taşınır!
                    foreach (var lot in cuzdan.Lotlar.ToList())
                    {
                        if (lot.Value > 0)
                        {
                            var hisseSkor = siraliHisseler.FirstOrDefault(x => x.Sembol == lot.Key);
                            decimal skor = hisseSkor.Sembol != null ? hisseSkor.KompozitSkor : 0m;

                            if (skor < 1.50m || !secilenSemboller.Contains(lot.Key))
                            {
                                var ilkHisseVerisi = loader.TumVeriler.FirstOrDefault(x => x.Tarih.Year == simYili && x.Sembol == lot.Key);
                                decimal f = ilkHisseVerisi != null ? (ilkHisseVerisi.Acilis > 0 ? ilkHisseVerisi.Acilis : ilkHisseVerisi.Kapanis) : cuzdan.Maliyetler[lot.Key];
                                DateTime t = ilkHisseVerisi != null ? ilkHisseVerisi.Tarih : new DateTime(simYili, 1, 1);

                                string neden = (skor < 1.50m)
                                    ? $"🔻 Düşük Sağlık Skoru (< 1.50) Yıl Başı Temizliği (Skor: {skor:F2})"
                                    : $"🔄 Yeni Yıl Portföy Dışı Satış (Skor: {skor:F2})";

                                cuzdan.Sat(lot.Key, f, lot.Value, t, neden);
                            }
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"   ✅ {simYili} Yılı Portföyüne Alınan Hisseler ({secilenSemboller.Count} Adet): {string.Join(", ", secilenSemboller)}");
                    Console.ResetColor();
                }

                // YIL BAŞI VARLIK VE NAKİT KAYDI
                var yilVerileri = loader.TumVeriler.Where(x => x.Tarih.Year == simYili && secilenSemboller.Contains(x.Sembol)).ToList();
                var yilTarihleri = yilVerileri.Select(x => x.Tarih.Date).Distinct().OrderBy(t => t).ToList();

                decimal yilBaslangicVarlik = cuzdan.Bakiye;
                foreach (var lot in cuzdan.Lotlar)
                {
                    if (lot.Value > 0)
                    {
                        var sonH = yilVerileri.FirstOrDefault(x => x.Sembol == lot.Key);
                        decimal f = sonH != null ? sonH.Kapanis : (cuzdan.Maliyetler.ContainsKey(lot.Key) ? cuzdan.Maliyetler[lot.Key] : 0m);
                        yilBaslangicVarlik += lot.Value * f;
                    }
                }
                decimal yilBaslangicYatirilan = cuzdan.ToplamYatirilanSermaye;
                decimal yilBaslangicCekilen = cuzdan.ToplamCekilenSermaye;
                peakPortfoyVarligi = yilBaslangicVarlik;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n   🚀 {simYili} YILI ALIM-SATIM SİMÜLASYONU BAŞLATILIYOR (Başlangıç Varlık: {yilBaslangicVarlik:N2} TL)...");
                Console.ResetColor();

                foreach (var tarih in yilTarihleri)
                {
                    var oGununVerileri = yilVerileri.Where(x => x.Tarih.Date == tarih).ToList();

                    if (tarih.Month != sonIslenenAy)
                    {
                        if (sonIslenenAy != -1)
                        {
                            if (aylikEkleme > 0)
                            {
                                cuzdan.SermayeEkle(aylikEkleme, tarih);
                                peakPortfoyVarligi += aylikEkleme;
                            }
                            if (aylikCekim > 0)
                            {
                                cuzdan.SermayeCekAkilli(aylikCekim, tarih, oGununVerileri);
                                peakPortfoyVarligi = Math.Max(0m, peakPortfoyVarligi - aylikCekim);
                            }
                        }
                        sonIslenenAy = tarih.Month;
                    }

                    // Anlık Portföy Değeri ve Devre Kesici Kontrolü
                    decimal anlikHisseVarligi = 0m;
                    foreach (var lot in cuzdan.Lotlar)
                    {
                        if (lot.Value > 0)
                        {
                            var hVeri = oGununVerileri.FirstOrDefault(x => x.Sembol == lot.Key);
                            decimal f = hVeri != null ? hVeri.Kapanis : (cuzdan.Maliyetler.ContainsKey(lot.Key) ? cuzdan.Maliyetler[lot.Key] : 0m);
                            anlikHisseVarligi += lot.Value * f;
                        }
                    }
                    decimal anlikToplamPortfoy = cuzdan.Bakiye + anlikHisseVarligi;
                    gunlukPortfoyDegerleri.Add(anlikToplamPortfoy);

                    if (anlikToplamPortfoy > peakPortfoyVarligi) peakPortfoyVarligi = anlikToplamPortfoy;

                    decimal anlikDrawdown = peakPortfoyVarligi > 0 ? (peakPortfoyVarligi - anlikToplamPortfoy) / peakPortfoyVarligi : 0m;

                    // Risk takibi bireysel hisse bazlı Akıllı Stop ile sağlanır (Panik Satış Devre Kesicisi kaldırıldı)

                    // 1. Bekleyen emirleri T+1 Açılış fiyatıyla çalıştır
                    if (bekleyenEmirler.Count > 0)
                    {
                        var siraliEmirler = bekleyenEmirler.OrderByDescending(x => x.Aday.Skor).ToList();
                        foreach (var emir in siraliEmirler)
                        {
                            var bugunHisseVerisi = oGununVerileri.FirstOrDefault(x => x.Sembol == emir.Aday.GunVerisi.Sembol);
                            if (bugunHisseVerisi != null)
                            {
                                bot.IslemYapTPlus1(emir, bugunHisseVerisi, akanGecmis);
                            }
                        }
                        bekleyenEmirler.Clear();
                    }

                    // 2. Satış ve Stop-Loss Kontrolleri
                    foreach (var gunVerisi in oGununVerileri)
                    {
                        bot.SatisKontroluVeZirveGuncelle(akanGecmis, gunVerisi);
                    }

                    // 3. Sinyal Taraması (Devre Kesici Modunda Değilsek)
                    if (devreKesiciKalanGun == 0)
                    {
                        foreach (var gunVerisi in oGununVerileri)
                        {
                            bool zatenVarMi = bekleyenEmirler.Any(x => x.Aday.GunVerisi.Sembol == gunVerisi.Sembol) ||
                                               (cuzdan.Lotlar.ContainsKey(gunVerisi.Sembol) && cuzdan.Lotlar[gunVerisi.Sembol] > 0);

                            if (!zatenVarMi)
                            {
                                var aday = bot.AlimSinyaliVeSkorHesapla(akanGecmis, gunVerisi);
                                if (aday != null)
                                {
                                    bekleyenEmirler.Add(new BekleyenEmir
                                    {
                                        Aday = aday,
                                        SinyalTarihi = gunVerisi.Tarih,
                                        SinyalKapanisFiyati = gunVerisi.Kapanis
                                    });
                                }
                            }
                        }
                    }
                    akanGecmis.AddRange(oGununVerileri);
                }

                // YIL SONU VARLIK VE FİNANSL RAPORU
                decimal yilBitisHisseVarligi = 0m;
                foreach (var lot in cuzdan.Lotlar)
                {
                    if (lot.Value > 0)
                    {
                        var sonHisse = yilVerileri.Where(x => x.Sembol == lot.Key).OrderBy(x => x.Tarih).LastOrDefault();
                        decimal sonFiyat = sonHisse != null ? sonHisse.Kapanis : 0m;
                        yilBitisHisseVarligi += lot.Value * sonFiyat;
                    }
                }
                decimal yilBitisVarlik = cuzdan.Bakiye + yilBitisHisseVarligi;

                decimal yilEklenenSermaye = cuzdan.ToplamYatirilanSermaye - yilBaslangicYatirilan;
                decimal yilCekilenSermaye = cuzdan.ToplamCekilenSermaye - yilBaslangicCekilen;

                decimal yilNetKazancTL = (yilBitisVarlik + yilCekilenSermaye) - (yilBaslangicVarlik + yilEklenenSermaye);
                decimal yilKarYuzdesi = yilBaslangicVarlik > 0 ? (yilNetKazancTL / yilBaslangicVarlik) * 100m : 0m;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n==========================================================================");
                Console.WriteLine($"          🎉 {simYili} YILI FİNAL PERFORMANS VE KÂR RAPORU                 ");
                Console.WriteLine("==========================================================================");
                Console.ResetColor();

                Console.WriteLine($"   ► {simYili} Yıl Başı Toplam Varlık : {yilBaslangicVarlik:N2} TL");
                Console.WriteLine($"   ► {simYili} Yıl Sonu Toplam Varlık : {yilBitisVarlik:N2} TL");
                if (yilEklenenSermaye > 0) Console.WriteLine($"   ► Bu Yıl Eklenen Sermaye      : +{yilEklenenSermaye:N2} TL");
                if (yilCekilenSermaye > 0) Console.WriteLine($"   ► Bu Yıl Çekilen Geçim Parası : -{yilCekilenSermaye:N2} TL");
                Console.WriteLine("   -----------------------------------------------------------------------");

                if (yilNetKazancTL >= 0)
                {

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"   ► {simYili} YIL NET KÂR (TL)         : +{yilNetKazancTL:N2} TL 🚀");
                    Console.WriteLine($"   ► {simYili} YILLIK KÂR ORANI (%)      : %{yilKarYuzdesi:F2} Net Kâr 🔥");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"   ► {simYili} YIL NET ZARAR (TL)       : {yilNetKazancTL:N2} TL 📉");
                    Console.WriteLine($"   ► {simYili} YILLIK ZARAR ORANI (%)    : %{yilKarYuzdesi:F2} Zarar");
                }
                Console.ResetColor();
                Console.WriteLine("==========================================================================");

                // 🔄 YIL SONU PORTFÖY REBALANSI: Kullanıcı tercihine göre pozisyonlar taşınır veya yıl sonu satılır.
                if (!yilSonuPozisyonlariTasi || simYili == simYillari.Last())
                {
                    var sonGunVerileri = yilVerileri.Where(x => x.Tarih.Date == yilTarihleri.Last()).ToList();
                    foreach (var lot in cuzdan.Lotlar.ToList())
                    {
                        if (lot.Value > 0)
                        {
                            var sonH = sonGunVerileri.FirstOrDefault(x => x.Sembol == lot.Key);
                            decimal f = sonH != null ? sonH.Kapanis : (cuzdan.Maliyetler.ContainsKey(lot.Key) ? cuzdan.Maliyetler[lot.Key] : 0m);
                            string etiket = simYili == simYillari.Last() ? "🏁 Simülasyon Sonu Final Nakde Geçiş" : "🔄 Yıl Sonu Nakde Geçiş Satışı";
                            cuzdan.Sat(lot.Key, f, lot.Value, yilTarihleri.Last(), etiket);
                        }
                    }
                }
            }
            // TÜM YILLAR FİNAL GENEL RAPORU
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n==========================================================================");
            Console.WriteLine("          🏆 ÇOKLU YIL TOPLAM SİMÜLASYON FİNAL BİLANÇOSU                 ");
            Console.WriteLine("==========================================================================");
            Console.ResetColor();

            decimal genelHisseDegeri = 0m;
            foreach (var lot in cuzdan.Lotlar)
            {
                if (lot.Value > 0)
                {
                    var sonHisse = loader.TumVeriler.Where(x => x.Sembol == lot.Key).OrderBy(x => x.Tarih).LastOrDefault();
                    decimal sonFiyat = sonHisse != null ? sonHisse.Kapanis : 0m;
                    genelHisseDegeri += lot.Value * sonFiyat;
                }
            }

            decimal genelVarlik = cuzdan.Bakiye + genelHisseDegeri;
            decimal cebimizdenCikanPara = cuzdan.ToplamYatirilanSermaye;
            decimal cebimizeGirenPara = cuzdan.ToplamCekilenSermaye;
            decimal netFinansalSonuc = (genelVarlik + cebimizeGirenPara) - cebimizdenCikanPara;
            decimal yuzdeselDegisim = cebimizdenCikanPara > 0 ? (netFinansalSonuc / cebimizdenCikanPara) * 100m : 0m;

            decimal maxPortfoyDrawdown = 0m;
            decimal zirveDeger = 0m;
            foreach (var v in gunlukPortfoyDegerleri)
            {
                if (v > zirveDeger) zirveDeger = v;
                decimal dd = zirveDeger > 0 ? (zirveDeger - v) / zirveDeger * 100m : 0m;
                if (dd > maxPortfoyDrawdown) maxPortfoyDrawdown = dd;
            }

            int kazanilanIslem = cuzdan.IslemKarZararListesi.Count(x => x > 0);
            int toplamIslem = cuzdan.IslemKarZararListesi.Count;
            decimal winRate = toplamIslem > 0 ? ((decimal)kazanilanIslem / toplamIslem) * 100m : 0m;

            decimal toplamKar = cuzdan.IslemKarZararListesi.Where(x => x > 0).Sum();
            decimal toplamZarar = Math.Abs(cuzdan.IslemKarZararListesi.Where(x => x < 0).Sum());
            decimal profitFactor = toplamZarar > 0 ? (toplamKar / toplamZarar) : (toplamKar > 0 ? 99.9m : 0m);

            Console.WriteLine($"   ► İlk Başlangıç Sermayesi         : {bakiye:N2} TL");
            Console.WriteLine($"   ► Cebinizden Çıkan Toplam Sermaye: {cebimizdenCikanPara:N2} TL (DCA Paraları Dahil)");
            if (cebimizeGirenPara > 0) Console.WriteLine($"   ► Kasadan Çekilen Geçim Parası   : +{cebimizeGirenPara:N2} TL");
            Console.WriteLine($"   ► Kasada Kalan Nakit             : {cuzdan.Bakiye:N2} TL");
            Console.WriteLine($"   ► Eldeki Portföy Hisse Değeri    : {genelHisseDegeri:N2} TL");
            Console.WriteLine($"   ► FİNAL TOPLAM PORTFÖY DEĞERİ    : {genelVarlik:N2} TL");
            Console.WriteLine("   -----------------------------------------------------------------------");

            if (netFinansalSonuc >= 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   ► NET GENEL PERFORMANS       : +{netFinansalSonuc:N2} TL (%{yuzdeselDegisim:F2} Net Kâr) 🚀");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   ► NET GENEL PERFORMANS       : {netFinansalSonuc:N2} TL (%{yuzdeselDegisim:F2} Zarar) 📉");
            }
            Console.ResetColor();

            Console.WriteLine("   -----------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("   📊 PERFORMANS ANALİTİĞİ VE BİLEŞİK GETİRİ METRİKLERİ:");
            Console.WriteLine($"   ► Maksimum Portföy Gerilemesi (MDD) : -%{maxPortfoyDrawdown:F2}");
            Console.WriteLine($"   ► Başarılı İşlem Oranı (Win Rate)  : %{winRate:F1} ({kazanilanIslem}/{toplamIslem} İşlem)");
            Console.WriteLine($"   ► Kâr / Zarar Oranı (Profit Factor) : {profitFactor:F2}");
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
