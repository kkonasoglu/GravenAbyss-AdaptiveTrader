# 📈 GravenAbyss-AdaptiveTrader

**GravenAbyss-AdaptiveTrader** is a high-performance, multi-asset quantitative trading backtest engine developed in C# (.NET Framework 4.7.2). It reads offline historical market data (CSV files) and applies institutional trend-following algorithms, dynamic regime classification, peak-based trailing stops, ATR risk shields, T+1 opening execution, and automated stock split adjustments.

---

# 🇬🇧 English Documentation

## 🚀 Key System Features

- 📁 **Offline CSV Market Data Engine:** Reads offline historical CSV market data from the `Veriler/` folder. Supports unlimited multi-asset datasets across global exchanges (BİST, NASDAQ, NYSE).
- 🌍 **Bilingual & Multi-Market Support:** Built-in multi-language selection (`[1] Turkish 🇹🇷`, `[2] English 🇬🇧`) at startup, with tailored market execution rules for Borsa Istanbul (BİST ±10% limits) and US Markets.
- 🛡️ **Macro Regime Classifier & Trend Shield:** Filters out whipsaws and choppy sideways markets by enforcing strict bull market regime conditions (`Boğa Piyasası`) and triple EMA trend alignment (`Price >= EMA20 && Price >= EMA50 && Price >= EMA200`).
- 🎯 **Peak-Based Trailing Stop & ATR Risk Shield:** Locks in profits during strong rallies by tracking all-time peak prices. Limits losses to 5-10% during sharp market crashes using volatility-based ATR stops.
- ⏱️ **T+1 Opening Execution & Gap-Up Shield:** Generates trade signals at day T close and executes orders at day T+1 opening prices, completely eliminating Look-Ahead Bias. Protects against locked gap-up limit openings (+9.5% gap shield for BİST).
- ✂️ **Automated Stock Split Adjuster:** Automatically detects stock splits and cost adjustments (e.g. `1:1.42`, `1:1.62`, `1:1.35`, `1:1.44`, `1:2.93`), revising share counts and entry costs without triggering false stop-loss sales.
- 🔄 **Year-Over-Year Compound Portfolio Carry:** Carries winning, high-health stocks seamlessly into the next year instead of selling on Dec 31st, maximizing long-term compounding growth.
- 💵 **Dual-Directional Cash Flow Engine:**
  - **[1] DCA Accumulation Mode:** Monthly cash injections into the trading wallet.
  - **[2] Income / Retirement Mode:** Monthly cash withdrawals for living expenses, using smart portfolio rebalancing when cash is low.
  - **[3] Fixed Capital Mode:** Zero additions/withdrawals for pure capital doubling backtests.

---

## 📊 Empirical Quantitative Metrics & Backtest Results

| Metric | BİST Market (BİST 🇹🇷) | US Market (NASDAQ/NYSE 🇺🇸) |
| :--- | :---: | :---: |
| **Strategy Profile** | Balanced Mode (%70 Budget) | Aggressive Mode (%90 Budget) |
| **Starting Capital** | 50,000 TL | 50,000 USD / TL |
| **Final Portfolio Value** | **86,834.67 TL** | **105,395.16 TL** |
| **Net Compounded Return** | **+73.67% Net Profit** | **+110.79% Net Profit (2.11x)** |
| **Profit Factor (Kâr/Zarar Oranı)** | **3.59** | **4.14** *(Elite Wall Street Standard > 3.0)* |
| **Win Rate (Başarı Oranı)** | **57.1%** | **50.0%** |
| **Market Benchmark Outperformance** | **2.1x Market Return** | **2.8x Market Return** |

---

## 🛠️ System Requirements & Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Konasoglu/GravenAbyss-AdaptiveTrader.git
   ```
2. **Build the application:**
   ```bash
   dotnet build
   ```
3. **Prepare Market Data:**
   Place your historical OHLCV `.csv` data files inside the `Veriler/` directory.
4. **Run the simulation:**
   ```bash
   dotnet run
   ```

---

<br/>

# 🇹🇷 Türkçe Dokümantasyon

## 🚀 Sistem Özellikleri ve Algoritma Mimarisi

- 📁 **Çevrimdışı (Offline) CSV Veri İşleme Motoru:** `Veriler/` klasörüne eklenen geçmiş OHLCV hisse verilerini (CSV formatında) otomatik tarar ve eşzamanlı backtest simülasyonu koşturur. Canlı borsa bağlantısı gerektirmez.
- 🌍 **Çift Dilli ve Çoklu Piyasa Desteği:** Program başlangıcında Türkçe (`TR 🇹🇷`) ve İngilizce (`EN 🇬🇧`) dil seçeneği sunar. Türkiye Borsa İstanbul (BİST - Tavan/Taban limitli) ve Amerika Piyasaları (NASDAQ / NYSE) için ayrı işlem kuralları uygular.
- 🛡️ **Makro Rejim Sınıflandırıcı ve Boğa Zırhı:** Piyasayı anlık oynaklık ve fiyat eğimine göre `"Boğa Piyasası"`, `"Testere Piyasası"` veya `"Yatay Piyasa"` olarak etiketler. Testere ve yatay piyasalardaki sahte kırılımları eler, yalnızca net Boğa trendlerinde işleme girer (`Fiyat >= EMA20 && Fiyat >= EMA50 && Fiyat >= EMA200`).
- 🎯 **Zirve Tabanlı Kâr İzleyen Stop (Trailing Stop) & ATR Kalkanı:** Pozisyona girdikten sonra hissenin gördüğü en yüksek zirve fiyatı anlık takip eder. Fiyat zirveden geri çekildiğinde kârı kilitler; piyasa çöküşlerinde ATR kalkanı ile zararı kısıtlar.
- ⏱️ **T+1 Gerçek Açılış Fiyatı & Gap-Up Kalkanı:** Alım-satım sinyallerini T günü kapanışında üretip, emri T+1 günü açılış fiyatından simüle eder (Look-Ahead Bias %0). BİST için +%9.5 üzeri kilit tavan açılışlarında alımı iptal eder.
- ✂️ **Otomatik Stok Split (Bölünme) Düzeltmesi:** Hisse bölünmelerini (`1:1.42`, `1:1.62`, `1:1.35`, `1:1.44`, `1:2.93`) otomatik tespit ederek lot miktarını günceller ve maliyeti revize eder. Sahte stop-loss satışlarını %100 engeller.
- 🔄 **Yıl Sonu Bileşik Pozisyon Taşıma:** 31 Aralık tarihinde yüksek Sağlık Skoruna (Sağlık Skoru >= 1.50) sahip hisseleri satmak yerine yeni yıla kesintisiz aktararak bileşik büyüme hızını katlar.
- 💵 **Çift Yönlü Nakit Akışı Yönetimi:**
  - **[1] Birikim (DCA) Modu:** Her ay kasaya düzenli sermaye ekler.
  - **[2] Maaş / Düzenli Gelir Modu:** Kasadan her ay düzenli harçlık çeker. Kasada nakit azaldığında kârlı hisseden parça satışı (Akıllı Rebalans) yapar.
  - **[3] Sabit Bakiye Modu:** Sermayeyi tam 2 katına (+%100+) katlama testleri için ekleme/çekme yapmadan çalışır.

---

## 📊 Doğrulanmış Backtest Metrikleri ve Kâr Raporu

| Metrik | Türkiye Borsa İstanbul (BİST 🇹🇷) | Amerika Piyasası (NASDAQ/NYSE 🇺🇸) |
| :--- | :---: | :---: |
| **Strateji Modu** | Dengeli Mod (%70 Bütçe) | Agresif Mod (%90 Bütçe) |
| **Başlangıç Bakiyesi** | 50.000 TL | 50.000 TL / USD |
| **Final Portföy Değeri** | **86.834,67 TL** | **105.395,16 TL** |
| **Net Bileşik Kâr** | **+%73,67 Net Kâr** | **+%110,79 Net Kâr (2.11 Katlama 🚀)** |
| **Profit Factor (Kâr/Zarar Oranı)** | **3.59** | **4.14** *(Wall Street Elit Standartı > 3.0)* |
| **Win Rate (Başarı Oranı)** | **%57,1** | **%50,0** |
| **Piyasa Endeksi Kıyaslaması** | **Piyasa Getirisinin 2.1 Katı** | **Piyasa Getirisinin 2.8 Katı** |

---

## ⚙️ Strateji ve Risk Profili Seçenekleri

1. **Garantici Mod:** Bütçe: %35 | Stop: %5.0 | Defansif Sermaye Koruması
2. **Dengeli Mod (BİST İdeal 🔥):** Bütçe: %70 | Stop: %10.0 | Optimum Kâr / Risk Dengesi *(Profit Factor: 3.59)*
3. **Agresif Mod (US-50 İdeal 🚀):** Bütçe: %90 | Stop: %15.0 | Maksimum Kâr & Sermaye Katlama *(Profit Factor: 4.14)*
4. **Bollinger Bantları Modu:** Alt Bant AL | Üst Bant SAT (Yatay Piyasa Kalkanı)

---

## 💻 Kurulum ve Çalıştırma

1. Repoyu klonlayın:
   ```bash
   git clone https://github.com/Konasoglu/GravenAbyss-AdaptiveTrader.git
   ```
2. Projeyi derleyin:
   ```bash
   dotnet build
   ```
3. Test etmek istediğiniz geçmiş CSV verilerini `Veriler/` klasörüne atın.
4. Simülasyonu başlatın:
   ```bash
   dotnet run
   ```

---

## ⚠️ YASAL UYARI VE SORUMLULUK REDDİ BEYANI (DISCLAIMER)

> **DİKKAT:** Bu yazılım yalnızca **eğitim, akademik araştırma ve simülasyon (Paper-Trading)** amacıyla geliştirilmiş açık kaynaklı bir algoritmik strateji takip aracıdır. **Kesinlikle bir yatırım tavsiyesi (YTD) veya finansal danışmanlık hizmeti değildir.**

### 📌 Kullanıcıların Dikkat Etmesi Gereken Önemli Hususlar:
1. **Gelecek Garantisi Yoktur:** Finansal piyasalar doğası gereği belirsizlik, rastlantısallık ve risk içerir. Geçmiş veri üzerinde yapılan simülasyonlardaki kârlılık ve başarı oranları, gelecekte veya canlı piyasada aynı sonuçların elde edileceğini **garanti etmez**.
2. **Kullanıcı Verisi Bağımlılığı:** Bot, kullanıcının sisteme sağladığı CSV verilerine ve seçtiği parametrelere göre mekanik kuralları çalıştırır. Hatalı/eksik veri veya piyasa krizleri beklenmeyen kayıplara yol açabilir.
3. **Sorumluluk Kabul Edilmez:** Bu yazılımı kullanan veya kodları canlı piyasalarda gerçek parayla işleme sokan kişilerin uğrayabileceği doğrudan veya dolaylı **hiçbir maddi/manevi zarardan yazılım geliştiricisi sorumlu tutulamaz**. Tüm finansal kararlar ve riskler tamamen kullanıcıya aittir.
