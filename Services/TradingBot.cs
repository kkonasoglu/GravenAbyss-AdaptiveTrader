using System;
using System.Collections.Generic;
using System.Linq;
using PaperTradingBot.Models;

namespace PaperTradingBot.Services
{
    /// <summary>
    /// Algoritmik alım-satım motoru, makro rejim sınıflandırıcısı, gösterge hesaplayıcısı ve risk yöneticisi sınıf.
    /// </summary>
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
        private Dictionary<string, List<DateTime>> _stopGecmisi = new Dictionary<string, List<DateTime>>();

        public TradingBot(PortfolioManager p, YatirimciTuru t, VadeTuru v, List<MacroEvent> m, PiyasaTuru piyasa = PiyasaTuru.TumPiyasalar)
        {
            _cuzdan = p;
            _tur = t;
            _vade = v;
            _makro = m;
            _piyasa = piyasa;

            _pencereGunSayisi = _vade == VadeTuru.KisaVade ? 20 : (_vade == VadeTuru.OrtaVade ? 60 : 200);
        }

        /// <summary>
        /// Hissenin pencere gün sayısı ve standart sapmasına bakarak piyasa rejimini ("Boğa Piyasası", "Testere Piyasası" veya "Sakin / Yatay Piyasa") hesaplar.
        /// </summary>
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

        /// <summary>
        /// Hissenin bağıl güç endeksini (RSI) periyot üzerinden hesaplar.
        /// </summary>
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

        /// <summary>
        /// Hissenin üstel hareketli ortalamasını (EMA) hesaplar.
        /// </summary>
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

        /// <summary>
        /// Hissenin ortalama gerçek aralık oynaklık değerini (ATR) hesaplar.
        /// </summary>
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

        /// <summary>
        /// Açık pozisyondaki hissenin zirve fiyatını günceller, trailing stop, ATR kalkanı ve stok split kontrollerini yapar.
        /// </summary>
        public void SatisKontroluVeZirveGuncelle(List<StockData> tumGecmis, StockData bugun)
        {
            if (bugun.Kapanis <= 0m) return;

            var hisseGecmisi = tumGecmis.Where(x => x.Sembol == bugun.Sembol && x.Kapanis > 0m).ToList();
            int periyot = _tur == YatirimciTuru.Garantici ? 21 : (_tur == YatirimciTuru.Dengeli ? 14 : (_tur == YatirimciTuru.AgresifRiskli ? 9 : 20));

            decimal atr = HesaplaATR(hisseGecmisi, 14);
            decimal stopYuzdesi;
            if (_piyasa == PiyasaTuru.AmerikaUS)
            {
                stopYuzdesi = _tur == YatirimciTuru.Garantici ? 0.06m : (_tur == YatirimciTuru.Dengeli ? 0.12m : (_tur == YatirimciTuru.AgresifRiskli ? 0.15m : 0.08m));
            }
            else if (_piyasa == PiyasaTuru.TurkiyeBIST)
            {
                stopYuzdesi = _tur == YatirimciTuru.Garantici ? 0.04m : (_tur == YatirimciTuru.Dengeli ? 0.07m : (_tur == YatirimciTuru.AgresifRiskli ? 0.085m : 0.05m));
            }
            else
            {
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

                    _cuzdan.Maliyetler[bugun.Sembol] = bugun.Kapanis;
                    _cuzdan.EnYuksekFiyatlar[bugun.Sembol] = bugun.Kapanis;
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

                // 🎯 RİSK PROFİLİ TABANLI DİNAMİK TREND VE ATR STOPU (+%3 Kâr Görünce Kâr İzleyen Stop Devreye Girer)
                decimal ilkHardStop = alisFiyati * (1m - stopYuzdesi);
                decimal trailingStop = zirveFiyat * (1m - stopYuzdesi);
                decimal chandelierStop = zirveFiyat - (3.0m * (atr > 0m ? atr : zirveFiyat * stopYuzdesi));
                decimal nihaiStop = (zirveFiyat >= alisFiyati * 1.03m) ? Math.Max(trailingStop, chandelierStop) : ilkHardStop;

                bool trendBitisSinyali = (zirveFiyat >= alisFiyati * 1.03m) && (bugun.Kapanis < ema50 && bugun.Kapanis < ema20);

                if (bugun.Kapanis <= nihaiStop || trendBitisSinyali)
                {
                    string etiket = (zirveFiyat >= alisFiyati * 1.03m)
                        ? $"🎯 KÂR İZLEYEN STOP (Zirve: {zirveFiyat:F2} TL | Maliyet: {alisFiyati:F2} TL)"
                        : $"🛡️ ATR KORUMA STOPU (Maliyet: {alisFiyati:F2} TL)";

                    _cuzdan.Sat(bugun.Sembol, bugun.Kapanis, _cuzdan.Lotlar[bugun.Sembol], bugun.Tarih, etiket);
                    _sonStopTarihleri[bugun.Sembol] = bugun.Tarih;
                    if (!_stopGecmisi.ContainsKey(bugun.Sembol)) _stopGecmisi[bugun.Sembol] = new List<DateTime>();
                    _stopGecmisi[bugun.Sembol].Add(bugun.Tarih);
                    return;
                }
            }
        }

        /// <summary>
        /// Donchian 20-günlük kırılımı, EMA Boğa Zırhı, hacim onayı, RSI seviyesi ve 2-stop karantina zırhını kontrol ederek alım adayı skoru üretir.
        /// </summary>
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

            // 🛡️ İYİ HİSSE KORUMA ZIRHI: Son 60 günde 2 kez stop yiyen hisseye 45 gün zorunlu karantina uygulanır!
            if (_stopGecmisi.ContainsKey(bugun.Sembol))
            {
                int son60GunStopCount = _stopGecmisi[bugun.Sembol].Count(t => (bugun.Tarih - t).TotalDays <= 60);
                if (son60GunStopCount >= 2)
                {
                    DateTime sonStopTarihi = _stopGecmisi[bugun.Sembol].Last();
                    if ((bugun.Tarih - sonStopTarihi).TotalDays < 45) return null;
                }
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

            // 🎯 DİSİPLİNLİ BOĞA TRENDİ: Erken momentum alımı (20-Gün Kırılım + EMA Boğa Zırhı)
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

        /// <summary>
        /// T günü kapanışında üretilen sinyali T+1 günü açılış fiyatından emre dönüştürür. BİST +%9.5 tavan gap-up kalkanını denetler.
        /// </summary>
        public void IslemYapTPlus1(BekleyenEmir emir, StockData bugunAcilisVerisi, List<StockData> tumGecmis)
        {
            decimal dunKapanis = emir.SinyalKapanisFiyati;
            decimal bugunAcilis = bugunAcilisVerisi.Acilis > 0 ? bugunAcilisVerisi.Acilis : bugunAcilisVerisi.Kapanis;

            decimal maxGapYuzdesi = 1.15m;
            if (_piyasa == PiyasaTuru.TurkiyeBIST || bugunAcilisVerisi.Piyasa == PiyasaTuru.TurkiyeBIST)
            {
                maxGapYuzdesi = 1.095m; // BİST +%9.5 Tavan Kalkanı
            }

            if (bugunAcilis > dunKapanis * maxGapYuzdesi)
            {
                Console.WriteLine($"   [İPTAL - Tavan/Gap Up Kalkanı] {bugunAcilisVerisi.Tarih:yyyy-MM-dd} | {emir.Aday.GunVerisi.Sembol,-10} | Dün: {dunKapanis:F2} TL -> Bugün Açılış: {bugunAcilis:F2} TL");
                return;
            }

            // 🎯 RİSK TABANLI VE DENGELİ POZİSYON BOYUTLANDIRMASI (Hisse başına %35 dengeli bütçe ile şampiyon hisselere eşit sermaye dağılımı)
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
}
