using System;
using System.Collections.Generic;
using System.Linq;
using PaperTradingBot.Models;

namespace PaperTradingBot.Services
{
    /// <summary>
    /// Nakit bakiye, portföy hisse lotları, ortalama maliyetler ve kâr/zarar (PnL) kayıtlarını tutan cüzdan ve sermaye yöneticisi sınıf.
    /// </summary>
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

            Bakiye += netGelir;
            Lotlar[sembol] -= lot;
            IslemKarZararListesi.Add(karZarar);

            string isaret = karZarar >= 0 ? "+" : "";
            Console.WriteLine($"   [SAT]             {tarih:yyyy-MM-dd} | {sembol,-10} | {lot,6} Lot | Fiyat: {fiyat,7:F2} | PnL: {isaret}{karZarar,8:F2} TL | {t}");

            if (Lotlar[sembol] == 0)
            {
                Maliyetler[sembol] = 0m;
                if (EnYuksekFiyatlar.ContainsKey(sembol)) EnYuksekFiyatlar.Remove(sembol);
            }
        }
    }
}
