# 📈 GravenAbyss-AdaptiveTrader

**GravenAbyss-AdaptiveTrader**, BİST ve küresel finansal piyasalar için C# (.NET Framework 4.7.2) ile geliştirilmiş; **Multi-Asset (Çoklu Varlık)**, **Gelişmiş Backtest Simülasyon Motoru**, **Zirve Fiyat Tabanlı Kâr İzleyen Stop (Trailing Stop-Loss)**, **ATR Koruma Kalkanı**, **T+1 Gerçek Zamanlı Emir Gerçekleme** ve **Otomatik Stok Split (Bölünme) Algılama** mimarisine sahip profesyonel bir Algoritmik Ticaret ve Paper-Trading Simülatörüdür.

Sistem, `Veriler/` klasöründeki tüm `.csv` hisse verilerini senkronize işler. Statik göstergeler yerine piyasanın anlık karakterini (Boğa, Testere, Sakin) ve Sağlık Skorunu analiz ederek alım-satım eşiklerini ve stop-loss oranlarını dinamik olarak adapte eder.

---

## 🚀 Öne Çıkan Gelişmiş Özellikler

- 📁 **Klasör Tabanlı Multi-Asset Tarama:** `Veriler/` klasörüne atılan sınırsız sayıdaki CSV dosyasını (BİST ve US Hisseleri) otomatik algılar ve tek bir zaman döngüsünde senkronize işler.
- 🎯 **Zirve Fiyat Tabanlı İzleyen Stop & ATR Kalkanı:** Trend yapan hisselerde tepe fiyatları takip eder; fiyat zirveden esnediğinde kârı kilitler, piyasa çöküşlerinde ATR kalkanı ile zararı %6-10 seviyesinde kısıtlar.
- ⏱️ **T+1 Gerçek Açılış Fiyatı & Sıçrama (Gap-Up) Kalkanı:** Sinyalleri T günü kapanışında üretip, emri T+1 günü açılış fiyatından simüle eder. Geleceği görme hatasını (Look-Ahead Bias) sıfırlar.
- 🔄 **Otomatik Stok Split (Bölünme) & Grace Period:** Maliyet düşüşlerini ve bölünmeleri (%12+ maliyet esnemesi) otomatik algılar, sahte stop satışlarını %100 engeller ve lot katlamasını yapar.
- 🔄 **Çoklu Yıl Bileşik Pozisyon Taşıma:** 31 Aralık'ta kârlı hisseleri satmak yerine yeni yıla kesintisiz aktararak bileşik büyüme hızını maksimuma çıkarır.
- 💵 **Çift Yönlü Nakit Akışı Yönetimi (DCA / Maaş Modu):**
  - **[1] Birikim (DCA) Modu:** Her ay kasaya düzenli sermaye ekler.
  - **[2] Maaş / Düzenli Gelir Modu:** Kasadan her ay düzenli harçlık çeker. Kasada nakit azaldığında en kârlı hisseden parça satışı (Akıllı Rebalans) yaparak nakit yaratır.
- 🏆 **Odak Portföy (Focus Portfolio):** Sermayeyi onlarca hisseye bölmek yerine en yüksek Sağlık Skorlu **EN GÜÇLÜ 3 ila 5 HİSSEYE** odaklayarak sermaye katlama hızını artırır.

---

## 📊 Doğrulanmış Backtest Simülasyon Metrikleri (2 Yıllık Test)

* 🟢 **Başlangıç Kasa:** 100.000,00 TL
* 💵 **Aylık DCA Eklemesi:** +2.000,00 TL / Ay (22 İşlem Günlük Periyot)
* 💳 **Cebinizden Çıkan Toplam Sermaye:** 146.000,00 TL
* 🏆 **2026 Yıl Sonu Final Portföy Değeri:** **205.277,32 TL**
* 🚀 **Net Bileşik Kâr:** **+59.277,32 TL (%+40,60 Net Kâr)**
* 📈 **2026 Yılı Tekil Performansı:** **+61.486,27 TL (%+51,33 Yıllık Net Kâr)**
* ⚖️ **Kâr/Zarar Oranı (Profit Factor):** **2.40** (Kurumsal Standartlarda)
* 🎯 **Başarı Oranı (Win Rate):** **%50.0** (İzleyen Stop disipliniyle zararlar küçük, kârlar devasa)

---

## ⚙️ Strateji ve Risk Profili Modları

1. **Garantici Mod:** Bütçe: %35 | Stop: %4.0 | Defansif Sermaye Koruması
2. **Dengeli Mod (Önerilen):** Bütçe: %70 | Stop: %7.0 | Optimum Kâr / Risk Dengesi 🔥
3. **Agresif Mod:** Bütçe: %90 | Stop: %8.5 | Maksimum Kâr & Sermaye Katlama 🚀 *(BİST Devre Kesici %9 marjı altı)*
4. **Bollinger Bantları Modu:** Alt Bant AL | Üst Bant SAT (Yatay Piyasa Kalkanı)

---

## 🛠️ Mimari ve Teknolojiler

- **Dil / Platform:** C# (.NET Framework 4.7.2 Console Application)
- **Veri Tipi:** Multi-Asset OHLCV CSV Parsing (TR / US Sayı ve Tarih Formatı Desteği)
- **Raporlama:** Yıllık ve Çoklu-Yıl Bileşik Performans Bilançosu, Win Rate, Profit Factor, Max Drawdown (MDD)

---

## 💻 Kurulum ve Çalıştırma

1. Repoyu klonlayın:
   ```bash
   git clone https://github.com/Konasoglu/GravenAbyss-AdaptiveTrader.git
   ```
2. Proje dizinine gidin ve derleyin:
   ```bash
   dotnet build
   ```
3. `Veriler/` klasörüne test etmek istediğiniz `.csv` verilerini atın.
4. Programı çalıştırın:
   ```bash
   dotnet run
   ```

---

## ⚠️ YASAL UYARI VE SORUMLULUK REDDİ BEYANI (DISCLAIMER)

> **DİKKAT:** Bu yazılım yalnızca **eğitim, akademik araştırma ve simülasyon (Paper-Trading)** amacıyla geliştirilmiş açık kaynaklı bir algoritmik strateji takip aracıdır. **Kesinlikle bir yatırım tavsiyesi (YTD) veya finansal danışmanlık hizmeti değildir.**

### 📌 Kullanıcıların Dikkat Etmesi Gereken Önemli Hususlar:
1. **Gelecek Garantisi Yoktur:** Finansal piyasalar doğası gereği belirsizlik, rastlantısallık, tahmin ve şans unsurları içerir. Geçmiş veri üzerinde yapılan simülasyonlardaki kârlılık ve başarı oranları (Win Rate), gelecekte veya canlı piyasada aynı sonuçların elde edileceğini **garanti etmez**.
2. **Kullanıcı Verisi Bağımlılığı:** Bot, kullanıcının sisteme sağladığı CSV verilerine ve seçtiği parametrelere göre mekanik kuralları çalıştırır. Hatalı/eksik veri veya piyasa krizleri beklenmeyen kayıplara yol açabilir.
3. **Sorumluluk Kabul Edilmez:** Bu yazılımı kullanan veya kodları canlı piyasalarda gerçek parayla işleme sokan kişilerin uğrayabileceği doğrudan veya dolaylı **hiçbir maddi/manevi zarardan yazılım geliştiricisi sorumlu tutulamaz**. Tüm finansal kararlar ve riskler tamamen kullanıcıya aittir.
