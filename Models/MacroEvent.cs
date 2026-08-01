using System;

namespace PaperTradingBot.Models
{
    /// <summary>
    /// Makro ekonomik takvim veya kriz günlerini temsil eden veri modeli.
    /// </summary>
    public class MacroEvent
    {
        public DateTime Tarih { get; set; }
        public int EtkiDerecesi { get; set; }
    }
}
