using System;

namespace PaperTradingBot.Models
{
    /// <summary>
    /// Alım sinyali üreten ve skorlanan hisse adayını temsil eder.
    /// </summary>
    public class AlimAdayi
    {
        public StockData GunVerisi { get; set; }
        public decimal RSI { get; set; }
        public decimal HacimOrani { get; set; }
        public decimal Skor { get; set; }
        public decimal ATR { get; set; }
        public decimal EMA200 { get; set; }
    }

    /// <summary>
    /// T günü kapanışında üretilip T+1 günü açılışında işlenecek bekleyen alım emri.
    /// </summary>
    public class BekleyenEmir
    {
        public AlimAdayi Aday { get; set; }
        public DateTime SinyalTarihi { get; set; }
        public decimal SinyalKapanisFiyati { get; set; }
    }
}
