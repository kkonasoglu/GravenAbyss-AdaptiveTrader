using System;

namespace PaperTradingBot.Models
{
    /// <summary>
    /// Yatırımcının risk toleransını ve bütçe kullanım oranını belirleyen strateji modu.
    /// </summary>
    public enum YatirimciTuru
    {
        /// <summary> Defansif Mod: Bütçe %35, Stop %4.0 - %5.0 </summary>
        Garantici = 1,

        /// <summary> Dengeli Mod (Önerilen / BİST İdeal): Bütçe %70, Stop %7.0 - %10.0 </summary>
        Dengeli = 2,

        /// <summary> Agresif Mod (ABD İdeal): Bütçe %90, Stop %8.5 - %15.0 </summary>
        AgresifRiskli = 3,

        /// <summary> Bollinger Bantları Modu: Alt Bant AL, Üst Bant SAT </summary>
        BollingerBantlar = 4
    }
}
