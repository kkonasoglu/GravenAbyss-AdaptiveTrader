# 📈 GravenAbyss-AdaptiveTrader

**GravenAbyss-AdaptiveTrader**, BIST ve genel finansal piyasalar için C# (.NET) ile geliştirilmiş, **Multi-Asset (Çoklu Varlık)** ve **Dinamik Kayan Pencere (Rolling Window)** mimarisine sahip bir Paper-Trading ve Algoritmik Yatırım Simülatörüdür.

Sistem, tanımlanan bir dizindeki (`Veriler/`) tüm `.csv` uzantılı hisse senetlerini otomatik tarar. Statik göstergeler yerine piyasanın anlık karakterini (Boğa, Testere, Sakin) analiz ederek alım-satım eşiklerini ve stop-loss oranlarını dinamik olarak adapte eder.

---

## 🚀 Öne Çıkan Özellikler

- 📁 **Klasör Tabanlı Multi-Asset Tarama:** `Veriler/` klasörüne atılan sınırsız sayıdaki CSV dosyasını (THYAO, FROTO, BIMAS vb.) otomatik algılar ve tek bir piyasa havuzuna aktarır.
- 🧠 **Dinamik Kayan Pencere (Rolling Window):** İşlem yapılan her gün geriye dönük seçilen vadede (Kısa: 20 Gün, Orta: 60 Gün, Uzun: 200 Gün) volatilite ve trend analizi yaparak piyasa rejimini tespit eder.
- 🎯 **Adapte Olabilir RSI ve Stop-Loss Eşikleri:** Piyasa rejimine göre (Boğa / Testere) RSI alım-satım eşiklerini ve Stop-Loss oranlarını otomatik esnetir veya sıkar.
- 🛡️ **Çoklu Filtre Kalkanı (Hacim + Makro Takvim + Fiyat Kalkanı):** 
  - **Hacim Onayı:** Son 10 günlük ortalama hacmin altındaki sahte kırılımları (fake-out) filtreler.
  - **Makro Risk Kalkanı:** Yüksek riskli ekonomi/faiz karar günlerinde alım işlemlerini askıya alır.
  - **Sıfır Fiyat Kalkanı:** Veri setindeki bozuk/sıfır fiyatlı satırlarda hatalı alım-satım yapılmasını engeller.
- ⏱️ **Geleceği Dikizleme Yanılsaması Yok (Zero Look-Ahead Bias):** Simülasyon zaman serisini tarihlere göre senkronize eder, bot asla gelecekteki fiyatı göremez.
- 👤 **Kişiselleştirilebilir Yatırımcı Profilleri:** Garantici, Dengeli ve Riskli profiller ile farklı risk toleranslarına uygun simülasyon imkanı.
- 🔄 **Yeniden Başlatma & Parametre Test Alanı:** Simülasyon tamamlandıktan sonra tek tuşla (`0`) yeni strateji ve vadelerle anında tekrar test çalıştırma.

---

## 🛠️ Mimari ve Teknolojiler

- **Dil:** C# (.NET / Console Application)
- **Veri Tipi:** Multi-Asset OHLCV CSV Parsing
- **Desteklenen CSV Formatları:** Sentetik Çoklu Hisse CSV & Investing BIST/Global CSV Formatları (Otomatik ayırıcı ve başlık tespiti ile)

---

## 💻 Kurulum ve Çalıştırma

1. Repoyu klonlayın:
   ```bash
   git clone [https://github.com/KULLANICI_ADIN/GravenAbyss-AdaptiveTrader.git](https://github.com/KULLANICI_ADIN/GravenAbyss-AdaptiveTrader.git)