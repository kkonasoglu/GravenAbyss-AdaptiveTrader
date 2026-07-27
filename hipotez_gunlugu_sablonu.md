# Hipotez Günlüğü — GravenAbyss Motoru

Her kayıt aşağıdaki formatta tutulur. Koddaki her kural/eşik/gösterge için bir kayıt zorunludur.

---

## Kayıt No: H-001
**Tarih:** 2026-07-20
**Hipotez:** DRNV hissesinde 10 günlük hareketli ortalamanın 30 günlüğü yukarı kesmesi, sonraki 5 günde pozitif getiriyle ilişkilidir; çünkü bu hisse yüksek oynaklıkta momentum karakteri gösteriyor gibi duruyor.
**Test Yöntemi:** İlk 12 aylık veride tüm kesişim noktalarını bul, sonraki 5 günlük getiriyi ölç, rastgele giriş noktalarıyla kıyasla.
**Karar Kuralı:** Ortalama getiri rastgele girişten anlamlı şekilde yüksekse kurala kodda yer ver; değilse hipotezi çürütülmüş say.
-- Test Sonrası --
**Sonuç:** (Test Aşamasında)
**Sayısal Bulgu:** -
**Öğrenilen:** -
**Koddaki Karşılığı:** -

---

## Kayıt No: H-002
**Tarih:** 2026-07-20
**Hipotez:** Veri analizi için hareketli ortalama kullanılacak, bu sayede 50 günlük geçmiş verilere bakarak daha uygun alımlar yapmasını sağlar.
**Test Yöntemi:** 3 farklı alıcı oluşturulacak ve onlara göre alım yüzdeleri belirlenecek ve koruma ile birlikte arama yapılacak.
**Karar Kuralı:** Eğer ki düzgün bir alım yaparlarsa hipotez doğrudur.
-- Test Sonrası --
**Sonuç:** Çürütüldü.
**Sayısal Bulgu:** -%1, -%3, alım yok.
**Öğrenilen:** İstenilenin aksine garantici zarar etti, en çok kazanan dengeli oldu, riskli olan hiçbir alım yapmadı. Verilen istatistik ve kurallardan kaynaklı her bir profil için yeni sınırlar koyulmalı.
**Koddaki Karşılığı:** Kaldırıldı.

---

## Kayıt No: H-003
**Tarih:** 2026-07-20
**Hipotez:** Hisse senedi piyasasında işlem yapan bir bot, kullanıcının risk iştahına göre RSI periyodunu (9, 14, 21) ve aşırı alım/satım eşiklerini (20-30-40) dinamik olarak değiştirerek, statik hareketli ortalama (SMA) yöntemine göre daha hızlı tepki veren ve piyasa döngülerine uyumlu bir strateji sergiler.
**Test Yöntemi:** Dinamik RSI eşikleri ile sabit SMA yönteminin farklı risk profillerinde performansını karşılaştır.
**Karar Kuralı:** Dinamik RSI daha hızlı reaksiyon verip zararı sınırlandırıyorsa kural koda alınır.
-- Test Sonrası --
**Sonuç:** Doğrulandı.
**Sayısal Bulgu:** Riskli: %5,1 zarar, Dengeli: %14,3 zarar, Garantici: %13,8 zarar.
**Öğrenilen:** Momentum tabanlı RSI motoruna geçtiğimizde, sistemin piyasaya dinamik olarak adapte olduğunu kanıtladık.
**Koddaki Karşılığı:**
`_tur == YatirimciTuru.Garantici ? 21 : (_tur == YatirimciTuru.Dengeli ? 14 : 9)`
`HesaplaRSI(...)` metodu.

---

## Kayıt No: H-004
**Tarih:** 2026-07-17
**Hipotez:** RSI tabanlı alım sinyallerine hacim (Volume) filtresi eklemek, hacimsiz sahte düşüşlerde (likidite tuzakları) botun hatalı alım yapmasını engeller ve sermaye verimliliğini artırır.
**Test Yöntemi:** RSI alım şartına `bugun.Hacim > ortalamaHacim` filtresi eklenerek backtest çalıştırıldı.
**Karar Kuralı:** Hacim filtresi sermaye erimesini azaltırsa kural kodda kalır.
-- Test Sonrası --
**Sonuç:** Doğrulandı.
**Sayısal Bulgu:** Riskli: %3,8 zarar, Garantici: %10,1 zarar, Dengeli: %13,3 zarar.
**Öğrenilen:** Hacim filtresi, botun sadece fiyat düşüşüne değil, piyasanın o fiyata verdiği kurumsal tepkiye (likiditeye) bakmasını sağlamıştır.
**Koddaki Karşılığı:** `bool hacimOnayli = bugun.Hacim > ortalamaHacim || ortalamaHacim == 0;`

---

## Kayıt No: H-005
**Tarih:** 2026-07-17
**Hipotez:** Sisteme dinamik risk profilli Zarar Kes (Stop-Loss) mekanizması eklemek, sermaye erimelerini sınırlandırır ve özellikle ayı piyasalarında botun kârlılık rasyolarını pozitife çevirir.
**Test Yöntemi:** Stop-loss'suz sistem ile %2, %4, %7 dinamik stop-loss sistemlerinin kıyası.
**Karar Kuralı:** Stop-loss toplam varlığı koruyorsa kural kodda kalır.
-- Test Sonrası --
**Sonuç:** Doğrulandı.
**Sayısal Bulgu:** Garantici: 10.101,59 TL (Kâr), Riskli: 9.697,20 TL, Dengeli: 9.551,16 TL.
**Öğrenilen:** Stop-loss koruması, özellikle Garantici modda (%2) düşüş trendlerine karşı mükemmel bir kalkan görevi görmüştür.
**Koddaki Karşılığı:** `decimal stopYuzdesi = _tur == YatirimciTuru.Garantici ? 0.02m : ...`

---

## Kayıt No: H-006
**Tarih:** 2026-07-20
**Hipotez:** 12 ay araştırma yapıp ondan sonraki 6 aylık işlem yapmak elde edilebilecek kârı azaltıyor. Onun yerine başlangıç tarihinden itibaren 60 gün tarama yaptıktan sonra işlem yapması kârı artırır.
**Test Yöntemi:** Sabit 12 aylık eğitim penceresi ile 60 günlük dinamik pencerenin kıyaslanması.
**Karar Kuralı:** Kârlılık ve adaptasyon artarsa kabul edilir.
-- Test Sonrası --
**Sonuç:** Çürütüldü.
**Sayısal Bulgu:** (10k üzerinden) +630,98 TL (%6,31 Kâr), +123,62 TL (%1,24 Kâr), +347,61 TL (%3,48 Kâr).
**Öğrenilen:** Risk analizi eksikliğinden kaynaklı istenilen olmadı. Kâr edildi fakat artış değil oranlarda düşüş görüldü.
**Koddaki Karşılığı:** Çürütüldüğü için kaldırıldı.

---

## Kayıt No: H-007
**Tarih:** 2026-07-20
**Hipotez:** Piyasa karakterini (Boğa, Testere, Sakin) etiketleyip alım-satım ve stop-loss eşiklerini bu etiketlere göre dinamik esnetmek işlem doğruluğunu artırır.
**Test Yöntemi:** Sabit eşikli bot ile Piyasa Karakterine göre eşik değiştiren botun karşılaştırılması.
**Karar Kuralı:** Dinamik rejim adaptasyonu kârlılığı artırırsa kabul edilir.
-- Test Sonrası --
**Sonuç:** Doğrulandı.
**Sayısal Bulgu:** Garantici: +2.865,46 TL (%28,65 Kâr), Dengeli: +515,08 TL (%5,15 Kâr), Riskli: +2.657,61 TL (%26,58 Kâr).
**Öğrenilen:** RSI ve Stop oranlarını piyasa etiketine (Boğa/Testere) göre hareket ettirince işlem doğruluğu ve kârlılık belirgin şekilde arttı.
**Koddaki Karşılığı:** `HesaplaAnlikPiyasaKarakteri(...)` metodu ve `if (anlikKarakter.Contains("Boğa")) ...` bloğu.

---

## Kayıt No: H-008
**Tarih:** 2026-07-21
**Hipotez:** Sistemin statik bir eğitim periyodu kullanması yerine sürekli kayan pencere (Rolling Window) ile anlık veri işlemesini sağlamak daha doğru karar aldırır.
**Test Yöntemi:** Kayan pencere analiz motorunun simülasyonda canlı akışa entegre edilmesi.
**Karar Kuralı:** Vade seçimlerine göre kayan pencere kârlılığı artırırsa kodda kalır.
-- Test Sonrası --
**Sonuç:** Doğrulandı.
**Sayısal Bulgu:** Dengeli Orta Vade: +%27,91 Kâr, Garantici Orta Vade: +%47,26 Kâr, Riskli Orta Vade: +%8,02 Kâr.
**Öğrenilen:** Hisse gidişatına ve seçilen vadeye göre kayan pencere boyutunu ayarlamak çok daha doğru alımlar sağladı.
**Koddaki Karşılığı:** `_pencereGunSayisi = _vade == VadeTuru.KisaVade ? 20 : (_vade == VadeTuru.OrtaVade ? 60 : 200);`

---

## Kayıt No: H-009
**Tarih:** 2026-07-21
**Hipotez:** İndirilen verileri klasör bazlı okuyup senkronize bir zaman çizelgesi üzerinden çoklu hisse (Multi-Asset) portföyü yönetmek tek hisseli işlemlere göre riski dağıtır ve sistemi ölçeklendirir.
**Test Yöntemi:** Klasördeki tüm CSV'lerin okunup günlük ortak tarih döngüsüyle simüle edilmesi.
**Karar Kuralı:** Çoklu hisse senkronizasyonu hatasız çalışıp riski bölerse kabul edilir.
-- Test Sonrası --
**Sonuç:** Doğrulandı.
**Sayısal Bulgu:** Kısa Vade Dengeli: -%40,27, Orta Vade Dengeli: -%3,60, Uzun Vade Dengeli: +%8,15 Kâr.
**Öğrenilen:** Artık sistem tek bir hisseye bağımlı kalmayıp klasördeki tüm hisseleri senkronize işleyebilmektedir.
**Koddaki Karşılığı:** `DataLoader.KlasordekiTumVerileriOku(...)` ve `tumTarihler` döngüsü.

---

## Kayıt No: H-010
**Tarih:** 2026-07-22
**Hipotez:** Sabit maliyetli stop-loss yerine Zirve Fiyat Tabanlı İzleyen Stop-Loss (Trailing Stop-Loss) kullanmak, trend yapan hisselerde oluşan kârın geri çekilmelerde erimesini engeller ve toplam portföy getirisini artırır.
**Test Yöntemi:** Sabit stop-loss ile trailing stop-loss mekanizmalarının kıyaslanması.
**Karar Kuralı:** İzleyen stop-loss portföy getirisini artırırsa kural kalıcı olur.
-- Test Sonrası --
**Sonuç:** Doğrulandı.
**Sayısal Bulgu:** 1. Evre (Sabit Stop): %1,56 Kâr, 2. Evre (%3 İzleyen Stop): %16,90 Kâr, 3. Evre (%4 İzleyen Stop + Uzun Vade): +%24,80 Kâr 🎯.
**Öğrenilen:** İzleyen stop-loss, tepe fiyatlardan dönüşleri erkenden yakalayarak kârın büyük kısmını kasada kilitler.
**Koddaki Karşılığı:** `PortfolioManager.EnYuksekFiyatlar` takibi ve `bugun.Kapanis <= zirveFiyat * (1 - stopYuzdesi)` kontrolü.

---

## Kayıt No: H-011
**Tarih:** 2026-07-22
**Hipotez:** Aynı gün alım sinyali veren çoklu hisseler arasında rastgele işlem yapmak yerine; Hacim Oranı, RSI Dip Derinliği ve Piyasa Trendi ile hesaplanan Çok Kriterli Potansiyel Skoruna (Multi-Factor Ranking) göre öncelik vermek sermayeyi en güçlü hisselere yönlendirir.
**Test Yöntemi:** Standart alfabeye göre alım yapan bot ile Potansiyel Skoruna göre sıralama yapan botun kârlılık kıyası.
**Karar Kuralı:** Akıllı sıralama kârlılığı artırırsa kabul edilir.
-- Test Sonrası --
**Sonuç:** Doğrulandı.
**Sayısal Bulgu:** Sinyal önceliklendirmesi yapılan simülasyonda portföyün Sharpe oranı ve net kârlılığı yükselmiştir.
**Öğrenilen:** Çoklu sinyallerde önceliklendirme mimarisi (Priority Queue) kullanmak risk/ödül oranını optimize eder.
**Koddaki Karşılığı:** `AlimAdayi.Skor` hesabı ve `gununAdaylari.OrderByDescending(x => x.Skor)` LINQ sıralaması.

---

## Kayıt No: H-012
**Tarih:** 2026-07-22
**Hipotez:** Fiyatı 200 günlük Basit Hareketli Ortalamasının (SMA-200) altında olan hisselerde alım yapmayı reddetmek, çöküş trendindeki sahte tepki yükselişlerini engeller.
**Test Yöntemi:** 7 hisseli veri setinde SMA-200 filtresi Açık ve Kapalı olarak test edildi.
**Karar Kuralı:** SMA-200 filtresi kârlılığı artırırsa koda alınır.
-- Test Sonrası --
**Sonuç:** Çürütüldü.
**Sayısal Bulgu:** SMA-200 Kapalı Net Getiri: +%24,80 Kâr (+2.480,15 TL), SMA-200 Açık Net Getiri: +%4,80 Kâr (+960,72 TL).
**Öğrenilen:** SMA-200 gecikmeli (Lagging) bir gösterge olduğu için dip seviyelerdeki aşırı satım fırsatlarını kaçırmış, kâr marjını %80 oranında eritmiştir.
**Koddaki Karşılığı:** Çürütüldüğü için koda eklenmedi.

---

## Kayıt No: H-013
**Tarih:** 2026-07-22
**Hipotez:** RSI göstergesi yerine, istatistiksel sapmayı ölçen Bollinger Bantları (20, 2) stratejisini kullanmak yatay ve oynak piyasalarda sahte sinyalleri azaltarak kârlılık sağlar.
**Test Yöntemi:** Aynı veri setinde [4] Bollinger Bantları Stratejisi çalıştırılarak RSI modları ile karşılaştırıldı.
**Karar Kuralı:** Bollinger Bantları yatay piyasada sermayeyi koruyorsa hipotez doğrulanır.
-- Test Sonrası --
**Sonuç:** Doğrulandı.
**Sayısal Bulgu:** Bollinger Bantları Modu: -%0,04 Zarar (Sermayeyi başa baş korudu), RSI Dengeli Modu: +%24,80 Kâr (Trendleri daha uzun sürdü).
**Öğrenilen:** Bollinger Bantları yatay kanallarda dip/tepe yakalamada çok başarılıdır ve sermayeyi korur. Ancak dik Boğa trendlerinde pozisyonu erken kapatır.
**Koddaki Karşılığı:** `HesaplaBollingerBantlari(...)` metodu ve `YatirimciTuru.BollingerBantlari` kontrolü.

---

## Kayıt No: H-014
**Tarih:** 2026-07-23
**Hipotez:** Satış sinyali oluştuğunda pozisyonun %100'ünü kapatmak yerine %50 parçalı kâr satışı (Scale-Out) yapıp kalan %50'yi izleyen stop-loss ile korumak, güçlü boğa trendlerinde kârı katlar.
**Test Yöntemi:** Tam satış yapan versiyon ile %50 parçalı satış + izleyen stop çalıştıran versiyon aynı veri setinde backtest edildi.
**Karar Kuralı:** Parçalı satış portföy getirisini artırırsa kural koda alınır.
-- Test Sonrası --
**Sonuç:** Kısmen Doğrulandı / Yan Etkisi Tespit Edildi.
**Sayısal Bulgu:** Tek başına kullanıldığında Testere Piyasasında dar stop-loss ile birleşince -%48,92 Zarar yazmıştır. Ancak H-015 (Cooldown) ile birleştiğinde +%205,44 Kâr seviyesine ulaşmıştır.
**Öğrenilen:** Parçalı kâr satışı trendli piyasalarda kârı kilitler fakat Testere Piyasasında dar stop-loss ile birleştiğinde Over-Trading (aşırı işlem) riski yaratır.
**Koddaki Karşılığı:** `SatisKontroluVeZirveGuncelle` içinde `satilacakLot = mevcutLot > 1 ? (int)Math.Floor(mevcutLot / 2.0) : mevcutLot;` satırı.

---

## Kayıt No: H-015
**Tarih:** 2026-07-23
**Hipotez:** Stop-loss sonrası hisse bazlı 5 günlük soğuma süresi (Cooldown) koymak, testere piyasasında peş peşe hatalı alım yapmayı engeller ve sermaye erimesini durdurur.
**Test Yöntemi:** -%48,92 zarar eden H-014 test parametrelerine `Cooldown = 5 Gün` kuralı eklenerek backtest tekrarlandı.
**Karar Kuralı:** Soğuma süresi over-trading'i engelleyip kârlılığı pozitife geçirirse kabul edilir.
-- Test Sonrası --
**Sonuç:** Doğrulandı 🎯
**Sayısal Bulgu:** Aynı veri setinde portföy performansı **-%48,92 zarardan +%20,83 kâra** geçti (Net +%69,75'lik toparlanma).
**Öğrenilen:** Algoritmik ticarette stop-loss kadar, stop sonrasında piyasaya ne zaman "yeniden girileceğini" kısıtlamak da hayati önem taşır.
**Koddaki Karşılığı:** `_sonStopTarihleri` Dictionary takibi ve `gecenGun < 5` alım engeli.

---

## Kayıt No: H-016
**Tarih:** 2026-07-23
**Hipotez:** Alım sinyallerini T gününün kapanış fiyatında (`Price`) üretip, emri bir sonraki işlem gününün (T+1) açılış fiyatından (`Open`) gerçekleştirmek ve ertesi gün aşırı yukarı yönlü fiyat boşluğu (Gap-Up > %3) oluştuysa alımı iptal etmek; backtest simülasyonundaki 'Geleceği Görme Hatasını (Look-Ahead Bias)' sıfırlar ve gerçek canlı piyasa getirisiyle %100 örtüşen sonuçlar üretir.
**Test Yöntemi:** Sinyal T günü kapanışında üretilip bekleyen emre alındı. T+1 günü açılış fiyatından alım simüle edildi; %3 üzeri Gap-Up durumlarında emir iptal edildi.
**Karar Kuralı:** T+1 Açılış gerçekliği ve Gap Filtresi eklendikten sonra sistem pozitif getiri yazıyorsa strateji canlıya hazır kabul edilir.
-- Test Sonrası --
**Sonuç:** Doğrulandı 🎯
**Sayısal Bulgu:** Tırnak-virgül parsing düzeltmesi ve T+1 gerçek açılış fiyatlarıyla 2.5 yıllık backtestte 25.000 TL sermaye **76.358,79 TL (+%205,44 Kâr)** seviyesine ulaştı.
**Öğrenilen:** T+1 açılış gecikmesi ve aşırı sıçrama (Gap-Up) filtreleri eklendiğinde bile sistem yüksek kârlılığını ve disiplinini korumaktadır.
**Koddaki Karşılığı:** `SplitCsvLine`, `TemizleVeParseEt` ve `IslemYapTPlus1` metodları.

## Kayıt No: H-017
**Tarih:** 2026-07-23
**Hipotez:**Alım sinyali için sadece RSI'ın dip seviyelere inmesi yetmez; fiyatın en azından son 2 işlem gününün en yüksek kapanış fiyatını yukarı kırmasını (Trend Dönüş Teyidi) şart koşmak, düşen bıçağı tutmayı (Falling Knife) engeller, sahte dip alımlarını eler ve işlem kalitesini yükseltir."*

## Kayıt No: H-017
**Tarih:** 2026-07-23
**Hipotez:** Alım sinyali için sadece RSI'ın dip seviyelere inmesi yetmez; fiyatın son 2 işlem gününün en yüksek fiyatını yukarı kırmasını (Trend Dönüş Teyidi) şart koşmak işlem kalitesini yükseltir.
**Test Yöntemi:** `AlimSinyaliVeSkorHesapla` metoduna `bugun.Kapanis > son2GunEnYuksek` şartı eklenerek 10.000 TL bakiye ile test edildi.
**Karar Kuralı:** Sistem kârlılığını artırırsa kural kodda kalır; aksi halde esnetilir veya kaldırılır.
**-- Test Sonrası --**
**Sonuç:** ÇÜRÜTÜLDÜ ❌
**Sayısal Bulgu:** Sistem -1.494,60 TL (%-14,95 Zarar) elde etmiştir. Sinyal sayısı aşırı azalmış ve T+1 Gap-Up filtresiyle çakışarak büyük rallilerin kaçırılmasına neden olmuştur.
**Öğrenilen:** Sert dip dönüşlerinde "son 2 günün en yükseğini kırma" şartı çok gecikmeli (lagging) kalmaktadır. Fiyat dönüş teyidi verirken %3'lük Gap-Up sınırını aştığı için emirler iptal olmakta ve kârlı trendler kaçmaktadır.
**Koddaki Karşılığı:** Koda eklenen `trendKirilimiOnayli` şartı kaldırılmalı veya sadece hisse sert düşüşteyken (Testere modunda) devreye girecek şekilde daraltılmalıdır.

## Kayıt No: H-018
**Tarih:** 2026-07-23
**Hipotez:** Gap-Up iptal sınırını %3'ten %6'ya esnetmek ve gecikmeli H-017 teyit filtresini kaldırarak T+1 açılışla doğrudan dip sinyalinde oyuna girmek, kaçan fırsatları yakalayarak kârlılığı artırır.
**Test Yöntemi:** H-017 filtresi kaldırıldı, Gap-Up üst limiti %6 yapılarak T+1 gerçek açılış fiyatlarıyla 2.5 yıllık simülasyon çalıştırıldı.
**Karar Kuralı:** Sistem kârlılığı artıp tutarlı işlemler üretirse kural kalıcı koda alınır.
**-- Test Sonrası --**
**Sonuç:** DOĞRULANDI 🎯
**Sayısal Bulgu:** 10.000 TL sermaye Orta Vade / Bollinger Bantları modunda **20.712,05 TL (+%107,12 Kâr)** seviyesine ulaşmış; Dengeli modda ise T+1 gerçek fiyatlarıyla yüksek getiri oranları korunmuştur.
**Öğrenilen:** Aşırı korumacı filtreleri esnetmek sistemin kârını boğmasını engellemiş; T+1 açılış fiyatı ve %6'lık gap kalkanı sayesinde gerçek dünya piyasa şartlarıyla tam uyumlu, yüksek getirili bir denge kurulmuştur.
**Koddaki Karşılığı:** `IslemYapTPlus1` içindeki `dunKapanis * 1.06m` kontrolü ve kaldırılan teyit engelleri.

## Kayıt No: H-018 / H-019
**Tarih:** 2026-07-23
**Hipotez:** 
> *"Simülasyon süresini tüm veri seti (2.5 yıl) yerine kullanıcının seçeceği belirli bir yatırım dönemiyle (3 Ay, 6 Ay, 1 Yıl) sınırlandırmak ve simülasyonu veri setindeki son X aydan başlatmak; yatırımcının gerçek hayattaki hedef vadelerine tam uyum sağlar ve dönem sonu bilançosunu daha anlaşılır kılar."*

**Test Yöntemi:** 
Kullanıcıdan Simülasyon Süresi (3 Ay / 6 Ay / 1 Yıl) seçeneği alınacak. Veri setindeki en son tarih başlangıç kabul edilerek geriye dönük ilgili ay kadar veri filtrelenip çalıştırılacak.

**Karar Kuralı:** 
Yatırım süresi seçimi koda başarıyla entegre edilip bilançoyu seçilen süreye göre doğru hesaplarsa kabul edilir.

-- Test Sonrası --
**Sonuç:** (Test sonrası doldurulacak)
**Sayısal Bulgu:** (Test sonrası doldurulacak)
**Öğrenilen:** (Test sonrası doldurulacak)
**Koddaki Karşılığı:** `Program.cs` içerisindeki tarih filtreleme döngüsü.

## Kayıt No: H-027 (Risk Profil ve Bütçe Katmanlaşması Testi)
**Tarih:** 2026-07-24
**Hipotez:** 
> *"Bütçe kullanım oranını (%25 vs %50) ve stop-loss esnekliğini risk profillerine bağlamak; Garantici Modda tam sermaye koruması sağlarken, Dengeli Modda pozisyon büyüklüğünü artırarak net kârlılığı fırlatır."*

**Test Yöntemi:** 
10.000 TL başlangıç + 2.000 TL/Ay düzenli DCA eklemesiyle 2.5 yıllık veride [1] Garantici Mod ve [2] Dengeli Mod koşturuldu.

**Sonuç:** DOĞRULANDI 🎯
**Sayısal Bulgu:** 
- **Garantici Mod (%25 Bütçe | %2 Stop):** 80.000 TL toplam sermayeyi başa baş koruyarak **79.992,32 TL (%-0,01)** ile tamamladı.
- **Dengeli Mod (%50 Bütçe | %4 Stop):** Aynı 80.000 TL sermayeyi **162.912 TL'ye (+%67,09 Kâr)** ulaştırdı.

**Öğrenilen:** 
Aşırı muhafazakar bütçe yönetimi (%25) sermaye eritme riskini sıfırlasa da bileşik getiriyi engeller. %50 bütçe kullanımı uzun vadeli birikimlerde ideal kâr/risk dengesini sunmaktadır.



*Yapay zeka sormalı sistem önerdi:* **reddedildi**



1. "Sinyalci / Karar Destek" Botuna Dönüşme Fikri
AI Önerisi: Botun tam otomatik al-sat yapması yerine, sadece fırsat gördüğünde kullanıcıya "Şu hissede kırılım var, alalım mı?" diye soru soran bir bildirim/sinyal mekanizmasına dönüştürülmesi.

 Kararın: *Reddedildi*.

Tasarım Gerekçesi: İnsan duygularını ve anlık kararlarını sürece dahil etmek istemedin. Sistemin insan müdahalesi olmadan tamamen otonom, disiplinli ve kural tabanlı çalışması hedeflendi.

2. Kural Tabanlı Botu Bırakıp "Breakout / 1:3 Risk-Ödül" Mimarisine Geçiş
AI Önerisi: RSI/Bollinger dip avcılığını tamamen çöpe atıp, botu sıfırdan "Yükseliş Kırılımı (Breakout)" yapan hisseleri takip eden yeni bir mimariye geçirmek.

 Kararın: *Reddedildi*.

Tasarım Gerekçesi: Mevcut motorun uzun vadedeki başarısını tutarlı buldun. Sıfırdan karmaşık bir mimariye geçmek yerine, çalışan efsanevi ana motorun korunmasını ve onun üzerinden devam edilmesini istedin.

3. İnteraktif İsim Sorma ve Kullanıcı Girdileri (UX)
AI Önerisi: Program her başladığında kullanıcıdan isim alan (Lütfen İsminizi Giriniz:) interaktif kişiselleştirme adımı.

 Kararın: *Reddedildi*.

Tasarım Gerekçesi: Geliştirici/operatör deneyimini yavaşlatan gereksiz akış adımlarından kaçınmak istedin; doğrudan karşılama ekranı ve veri analiz paneliyle hızlı bir başlatma tercih ettin.

4. Karmaşık/Ayrıntılı Veri Göstergeleri (Order Book / Derinlik Analizi)
AI Önerisi: Alım-satım hacimlerini ayrıştırmak için derinlik verisi veya daha karmaşık sipariş defteri (Order Book) göstergeleri eklemek.

 Kararın: *Reddedildi*.

Tasarım Gerekçesi: Mevcut CSV verilerinin sadeliğini korumak, sistemi ek veri bağımlılıklarıyla karmaşıklaştırmamak ve basit OHLCV (Açılış, Yüksek, Düşük, Kapanış, Hacim) yapısıyla çözüme gitmek istedin.

5. Kısa Vadeli Başarısızlık Yüzünden Sistemi Yetersiz Sayıp Terk Etmek
AI Önerisi: 3 aylık testlerdeki cılız kârlar nedeniyle botun tamamen kullanışsız/avantajsız olduğunu kabul etmek.

 Kararın: *Reddedildi*.

Tasarım Gerekçesi: Kısa vadeli gürültülerin uzun vadeli ana stratejiyi gölgelemesine izin vermedin; botun asıl gücünün uzun vadeli trend takibinde (+%110 kâr) olduğunu görerek sistemi çöpe atmak yerine uzun vadeli odağa geri döndürdün.
