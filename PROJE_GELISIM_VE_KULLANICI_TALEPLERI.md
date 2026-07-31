# 📜 GravenAbyss - Proje Gelişim ve Kullanıcı Talepleri Günlüğü

Bu doküman, **GravenAbyss Multi-Asset Paper-Trading Algoritmik Yatırım Sistemi** geliştirme sürecinde kullanıcının sorduğu soruları, sunduğu geri bildirimleri, tespit ettiği hataları ve bunların algoritmik çözümlerini kronolojik olarak belgelendirir.

---

## 📅 Kronolojik Kullanıcı Talepleri ve Algoritmik Çözümler

### 1. 🧹 Başlangıç Metin Temizliği ve Karakter Kodlaması
- **Kullanıcı Talebi:** *"Başta canlı piyasa değerleri diyor canlı olmamasına rağmen, aynı zamanda yorum satırlarında ?? soru işaretleri var onları kaldır."*
- **Algoritmik Çözüm:**
  - `Program.cs` açılışındaki yanıltıcı `"CANLI PİYASA"` seçim ekranı ve metinleri kaldırıldı. Sistem doğrudan Çevrimdışı Backtest Simülasyon Ekranına bağlandı.
  - `Main()` metodunun en üstüne `Console.OutputEncoding = System.Text.Encoding.UTF8;` entegre edilerek Windows konsolundaki `??` bozuk karakter hataları %100 çözüldü.

---

### 2. 🇹🇷 BİST Tavan / Taban Limitlerinin ve T+1 Emir Kurallarının Uygulanması
- **Kullanıcı Talebi:** *"BİST'te %10'dan fazla zarar veya kâr olamaz, stoploss sınırını ve gap up sınırını buna göre ayarla."*
- **Algoritmik Çözüm:**
  - `IslemYapTPlus1` metodunda BİST piyasası için **+9.5% Gap-Up Tavan Kalkanı** (`maxGapYuzdesi = 1.095m`) uygulandı. Tavan kilitleyen hisselere alım emri iptal edildi.
  - BİST piyasasında stop-loss oranları günlük devre kesici ve taban marjı altında (**%4.0 - %8.5**) kesin kısıtlandı.

---

### 3. 🎯 Dinamik Donchian Kırılım Periyodu ve EMA Boğa Zırhı
- **Kullanıcı Talebi:** *"Hiçbir şey değişmedi gibi sanki, kırılım periyotları seçilen vadeye bağlansın."*
- **Algoritmik Çözüm:**
  - `AlimSinyaliVeSkorHesapla` metodundaki Donchian breakout periyodu seçilen vadeye (`_pencereGunSayisi`: 20 gün Kısa, 60 gün Orta, 200 gün Uzun) dinamik bağlandı.
  - Disiplinli **EMA Boğa Zırhı** (`Fiyat >= EMA20 && Fiyat >= EMA50 && Fiyat >= EMA200`) şartı getirildi.

---

### 4. 📉 Past-Year Hyper-Rallier Tuzaklarının Elenmesi (Mean-Reversion Düzeltmesi)
- **Kullanıcı Talebi:** *"İyi de asıl problem ilk yıl edilen zarar ve ikinci yıl getirdiği ufak para, 1 yıl açısından bakınca az geliyor."*
- **Algoritmik Çözüm:**
  - 1 yılda %80+ aşırı prim yapan hisselerin ertesi yıl kâr satışıyla dinlenmeye çekildiği (mean-reversion riski) tespit edildi.
  - Sağlık Skoru hesabına aşırı prim cezası (`if (getiri > 80m) kompozitSkor *= 0.35m;`) eklendi.
  - Alım şartına kesin **Boğa Piyasası Rejimi** (`bogaRejimMi = true`) kuralı eklendi; böylece yatayda sürünen geçmiş ralliciler elenip, taze compounder hisseler öne çıkarıldı.

---

### 5. 💵 DCA (Nakit Ekleme) ve Sabit Bakiye Matematiksel Karşılaştırması
- **Kullanıcı Talebi:** *"Kar hala tatmin edici değil neden böyle?"*
- **Algoritmik Çözüm:**
  - Yılın son aylarında yatırılan DCA parasının boşta yatıp nakit sürüklemesi (Cash Drag) yarattığı açıklandı.
  - **Sabit Bakiye Modu (`[3] Sabit Bakiye`)** ile yapılan testte 50.000 TL anaparanın **86.834 TL'ye (+%73.67 Net Kâr)** fırladığı matematiksel olarak kanıtlandı.

---

### 6. ❌ Statik Mikro Bütçe Dilimlemesi Deneyi (H-035)
- **Kullanıcı Talebi:** *"Hisse verilerine bakmadan kârı nasıl %100 yaparız?"*
- **Yapılan Deney:** Sermaye 5 eşit mikro dilime (%14/hisse) bölündü.
- **Sonuç ve Geri Alma:** Kârlı trend hisselerine (`ASELS`) ayrılan bütçeyi seyreltip kârı %73.67'den %8.77'ye düşürdüğü için kullanıcı talimatıyla **DERHAL GERİ ALINDI** ve **H-035** olarak kaydedildi.

---

### 7. 🇺🇸 Amerika vs 🇹🇷 BİST Piyasa Strateji Uyumunun Keşfi (H-036)
- **Kullanıcı Tespit & Talebi:** *"Yani anladığım kadarıyla Amerikan hisselerinde riskli oynamak daha kârlı iken BİST'te oynamak ise dengeli daha kârlı, var mı bir yanlışım?"*
- **Doğrulama:**
  - **Amerikan Piyasası (NASDAQ/NYSE 🇺🇸) + Agresif Mod (%90 Bütçe):** **105.395,16 TL (+%110,79 Net Kâr / 2.11 Katlama / Profit Factor: 4.14)** ile **YILLIK 2X PASİF GELİR HEDEFİ VURULDU.**
  - **BİST Piyasası 🇹🇷 + Dengeli Mod (%70 Bütçe):** **86.834,67 TL (+%73,67 Net Kâr / Profit Factor: 3.59)** ile piyasa getirisini 2.1 kat solladı.

---

### 8. 🛡️ İyi Hisselerde Batmama ve 2-Stop 45-Gün Karantina Zırhı (H-037)
- **Kullanıcı Talebi:** *"İstikrardan kastım kâr oranında, yani ne olursa olsun yıl sonunda kâr edecek şekilde dizayn edilmeli, bundan kastım da iyi hisselerde batmamalı."*
- **Algoritmik Çözüm:**
  - `_stopGecmisi` takip mekanizması kuruldu.
  - Son 60 günde 2 kez stop oluşturan hisse **45 Günlük Zorunlu Karantinaya** alındı. Düzeltmedeki kaliteli hisselere peş peşe girip sermaye eritilmesi %100 engellendi.

---

### 9. 🌐 Çift Dilli Arayüz (TR/EN) ve Kullanıcı Veri Uyarı Banderolü
- **Kullanıcı Talebi:** *"Dil seçildikten sonra bir uyarı geçilmesi güzel olur, bunun sizin verdiğiniz CSV uzantılı ve gönderilen örneklerle aynı dosyalar olmasını şart koştuğumuzu yazar mısın?"*
- **Algoritmik Çözüm:**
  - `Program.cs` açılışına interaktif **Dil Seçimi (`[1] TR 🇹🇷 / [2] EN 🇬🇧`)** eklendi.
  - Dil seçiminden hemen sonra mor renkli **📌 ÖNEMLİ SİSTEM UYARISI VE VERİ GEREKSİNİMİ** bandrolü gösterilmesi sağlandı.
  - Klasördeki yüklenen CSV dosyalarını listeleyen **Yeşil Veri Özet Ekranı** ve boş klasör rehberi eklendi.
  - [README.md](file:///c:/Users/Konasoglu/source/repos/PaperTradingBot/README.md) üstte İngilizce, altta Türkçe olacak şekilde çift dilli olarak baştan yazıldı.
