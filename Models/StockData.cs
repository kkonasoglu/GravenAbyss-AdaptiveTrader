using System;

namespace PaperTradingBot.Models
{
    /// <summary>
    /// İşlem yapılacak borsa ve piyasa türü.
    /// </summary>
    public enum PiyasaTuru
    {
        /// <summary> Türkiye Borsa İstanbul (BİST - TL Cinsinden | Tavan/Taban Limitli 🇹🇷) </summary>
        TurkiyeBIST = 1,

        /// <summary> Amerika Piyasası (NASDAQ / NYSE - Dolar Cinsinden | Yüksek Volatilite 🇺🇸) </summary>
        AmerikaUS = 2,

        /// <summary> Tümü / Hibrit Portföy (Çoklu Piyasa Tarama 🌍) </summary>
        TumPiyasalar = 3
    }

    /// <summary>
    /// Trend rejim analizi için bakılacak pencere gün sayısı (Kısa: 20 Gün, Orta: 60 Gün, Uzun: 200 Gün).
    /// </summary>
    public enum VadeTuru
    {
        KisaVade = 1,
        OrtaVade = 2,
        UzunVade = 3
    }

    /// <summary>
    /// Tek bir günlük hisse mum verisini (OHLCV) temsil eden veri modeli.
    /// </summary>
    public class StockData
    {
        public DateTime Tarih { get; set; }
        public string Sembol { get; set; }
        public decimal Kapanis { get; set; }
        public decimal Acilis { get; set; }
        public decimal Hacim { get; set; }
        public PiyasaTuru Piyasa { get; set; } = PiyasaTuru.TumPiyasalar;
    }
}
