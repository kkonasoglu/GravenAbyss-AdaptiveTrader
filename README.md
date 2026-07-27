# 📈 GravenAbyss-AdaptiveTrader

**GravenAbyss-AdaptiveTrader**, BIST ve küresel finansal piyasalar için C# (.NET) ile geliştirilmiş; **Multi-Asset (Çoklu Varlık)**, **Zirve Fiyat Tabanlı İzleyen Stop (Trailing Stop-Loss)**, **T+1 Gerçek Zamanlı Emir Gerçekleme** ve **Otomatik Stok Split (Bölünme) Algılama** mimarisine sahip gelişmiş bir Algoritmik Ticaret ve Paper-Trading Simülatörüdür.

Sistem, `Veriler/` klasöründeki tüm `.csv` hisse verilerini senkronize işler. Statik göstergeler yerine piyasanın anlık karakterini (Boğa, Testere, Sakin) analiz ederek alım-satım eşiklerini ve stop-loss oranlarını dinamik olarak adapte eder.

---

## 🚀 Öne Çıkan Gelişmiş Özellikler

- 📁 **Klasör Tabanlı Multi-Asset Tarama:** `Veriler/` klasörüne atılan sınırsız sayıdaki CSV dosyasını (AKBNK, META, AVGO, ASELS vb.) otomatik algılar ve tek bir zaman döngüsünde senkronize işler.
- 🎯 **Zirve Fiyat Tabanlı İzleyen Stop (Trailing Stop-Loss):** Trend yapan hisselerde tepe fiyatları takip eder; fiyat zirveden esnediğinde kârı kilitler.
- ⏱️ **T+1 Gerçek Açılış Fiyatı & Sıçrama (Gap-Up) Kalkanı:** Sinyalleri T günü kapanışında üretip, emri T+1 günü açılış fiyatından simüle eder. Geleceği görme hatasını (Look-Ahead Bias) sıfırlar.
- 🔄 **Otomatik Stok Split & Temettü Düzeltmesi:** %12 ve üzerindeki maliyet düşüşlerini otomatik stok bölünmesi olarak yakalar, lot sayılarını katlayarak sahte stop satışlarını engeller.
- 💵 **Çift Yönlü Nakit Akışı Yönetimi:**
  - **[1] Birikim (DCA) Modu:** Her ay kasaya düzenli sermaye ekler.
  - **[2] Maaş / Düzenli Gelir Modu:** Kasadan her ay düzenli harçlık çeker. Kasada nakit azaldığında en kârlı hisseden parça satışı (Akıllı Rebalans) yaparak nakit yaratır.
- 🏆 **Odak Portföy (Focus Portfolio):** Sermayeyi onlarca hisseye bölmek yerine en yüksek skorlu EN GÜÇLÜ 5 HİSSEYE odaklayarak sermaye katlama hızını artırır.

---

## 📊 Örnek Simülasyon Metrikleri (2.5 Yıllık Backtest)

* 🟢 **Birikim Modu (+2.000 TL/Ay DCA):** 50.000 TL Anapara ➔ **457.965,88 TL (%+377,05 Net Kâr)**
* 🟢 **Maaş Modu (-2.000 TL/Ay Çekim):** 46.000 TL Maaş Çekildi ➔ Kasada Kalan: **150.230,92 TL (%+292,46 Net Kâr)**
* 🎯 **Başarı Oranı (Win Rate):** %69,3 – %73,6
* ⚖️ **Kâr/Zarar Oranı (Profit Factor):** 8.46 – 12.82

---

## 🛠️ Mimari ve Teknolojiler

- **Dil / Platform:** C# (.NET Console Application)
- **Veri Tipi:** Multi-Asset OHLCV CSV Parsing (TR / US Sayı ve Tarih Formatı Desteği)
- **Raporlama:** Yıllık ve Çoklu-Yıl Bileşik Performans Bilançosu

---

## 💻 Kurulum ve Çalıştırma

1. Repoyu klonlayın:
   ```bash
   git clone https://github.com/KULLANICI_ADIN/GravenAbyss-AdaptiveTrader.git
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
