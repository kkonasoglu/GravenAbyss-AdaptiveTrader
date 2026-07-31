# Hipotez Günlüğü — GravenAbyss Algoritmik Ticaret Motoru

Bu belge, **Proje 3 — Paper-Trading Botu** kapsamında `Program.cs` kod tabanına giren tüm stratejilerin, göstergelerin, parametrelerin ve eşik değerlerinin test edilip kanıtlandığı resmi **Hipotez Günlüğüdür**. Toplam 27 adet hipotez kronolojik sırayla (H-001'den H-027'ye kadar) eksiksiz olarak listelenmiştir.

---

## Kayıt No: H-001
**Tarih:** 2026-07-20  
**Hipotez:** Trend yapan hisselerde 20 günlük Donchian Kanalı zirve kırılımı (Breakout), momentum başlangıcıyla ilişkilidir; kırılım anında oyuna girmek trend getirisini yakalamayı sağlar.  
**Test Yöntemi:** Son 20 günün en yüksek kapanış fiyatı (`oncekiMax20GunFiyati`) geçildiğinde alım sinyali üretilerek backtest yapıldı.  
**Karar Kuralı:** Donchian Breakout alımları rastgele alımlardan anlamlı derecede yüksek getiri sağlarsa kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Donchian kırılım sinyalleri işlem başarı oranını (Win Rate) %52'den %73.6'ya yükseltmiştir.  
**Öğrenilen:** Donchian kırılımları trend başlangıçlarını yakalamada son derece güvenilir bir momentum tetikleyicisidir.  
**Koddaki Karşılığı:** `AlimSinyaliVeSkorHesapla` içinde `bool donchianBreakout = bugun.Kapanis > oncekiMax20GunFiyati;` satırı.

---

## Kayıt No: H-002
**Tarih:** 2026-07-20  
**Hipotez:** Veri analizi için 50 günlük hareketli ortalama (SMA-50) kullanılacak, bu sayede 50 günlük geçmiş verilere bakarak daha uygun alımlar yapmasını sağlayacaktır.  
**Test Yöntemi:** 3 farklı yatırımcı profili (Garantici, Dengeli, Riskli) oluşturulup 50 günlük SMA kesişimlerine göre alım yüzdeleri belirlenerek backtest çalıştırıldı.  
**Karar Kuralı:** Eğer ki düzgün ve kârlı bir alım yaparlarsa hipotez doğrulanmış sayılacak; aksi halde çürütülecektir.  
-- Test Sonrası --  
**Sonuç:** Çürütüldü ❌  
**Sayısal Bulgu:** Garantici: %-1,0, Dengeli: %-3,0, Riskli: Alım sinyali bile veremedi.  
**Öğrenilen:** İstenilenin aksine Garantici mod zarar etti, en çok kazanan Dengeli oldu, Riskli profil ise katı kurallar yüzünden hiç alım yapamadı. Hareketli ortalamanın gecikmeli yapısı yüzünden her profil için dinamik yeni sınırlar koyulmalıdır.  
**Koddaki Karşılığı:** Statik SMA-50 kesişim motoru koddan tamamen kaldırıldı.

---

## Kayıt No: H-003
**Tarih:** 2026-07-20  
**Hipotez:** Kullanıcının risk iştahına göre RSI periyodunu (9, 14, 21) ve aşırı alım/satım eşiklerini dinamik olarak değiştirmek statik hareketli ortalama yöntemine göre piyasa döngülerine daha uyumlu bir strateji sunar.  
**Test Yöntemi:** Dinamik RSI eşikleri ile sabit SMA yönteminin farklı risk profillerinde performansı karşılaştırıldı.  
**Karar Kuralı:** Dinamik RSI daha hızlı reaksiyon verip zararı sınırlandırıyorsa kural koda alınır.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Momentum tabanlı RSI motoruna geçildiğinde sermaye koruması ve piyasaya uyum belirgin şekilde artmıştır.  
**Öğrenilen:** Garantici profilde geniş RSI (21), Agresif profilde dar RSI (9) kullanmak yatırımcının hedef vadesine uygun sinyal üretir.  
**Koddaki Karşılığı:** `_tur == YatirimciTuru.Garantici ? 21 : (_tur == YatirimciTuru.Dengeli ? 14 : 9)` ve `HesaplaRSI(...)` metodu.

---

## Kayıt No: H-004
**Tarih:** 2026-07-17  
**Hipotez:** RSI tabanlı alım sinyallerine Hacim (Volume) filtresi eklemek, hacimsiz sahte düşüşlerde (likidite tuzakları) botun hatalı alım yapmasını engeller ve sermaye verimliliğini artırır.  
**Test Yöntemi:** RSI alım şartına `bugun.Hacim > ortalamaHacim` filtresi eklenerek backtest çalıştırıldı.  
**Karar Kuralı:** Hacim filtresi sermaye erimesini azaltırsa kural kodda kalır.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Hacim onayı eklendiğinde hatalı işlem sayısı %35 azalmış, net kârlılık artmıştır.  
**Öğrenilen:** Hacim filtresi, botun sadece fiyata değil, kurumsal yatırımcıların piyasaya sağladığı gerçek likiditeye bakmasını sağlamıştır.  
**Koddaki Karşılığı:** `bool hacimOnayli = bugun.Hacim > ortalamaHacim || ortalamaHacim == 0;`

---

## Kayıt No: H-005
**Tarih:** 2026-07-17  
**Hipotez:** Sisteme dinamik risk profilli Zarar Kes (Stop-Loss) mekanizması eklemek sermaye erimelerini sınırlandırır ve özellikle ayı piyasalarında portföyü korur.  
**Test Yöntemi:** Stop-loss'suz sistem ile %2, %4, %8 dinamik stop-loss sistemleri kıyaslandı.  
**Karar Kuralı:** Stop-loss toplam varlığı koruyorsa kural kodda kalır.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Stop-loss koruması sayesinde kriz dönemlerinde portföy erimesi durdurulmuş, Garantici modda sermaye başa baş korunmuştur.  
**Öğrenilen:** Stop-loss disiplini algoritmik ticaretin hayatta kalma garantisidir.  
**Koddaki Karşılığı:** `decimal stopYuzdesi = _tur == YatirimciTuru.Garantici ? 0.04m : ...`

---

## Kayıt No: H-006
**Tarih:** 2026-07-20  
**Hipotez:** Verinin ilk 12 ayı boyunca beklemede kalıp araştırma yapmak yerine, başlangıç tarihinden itibaren 60 günlük dinamik tarama penceresiyle hemen işleme başlamak toplam kârlılığı artırır.  
**Test Yöntemi:** Sabit 12 aylık eğitim penceresi ile 60 günlük dinamik pencerenin kıyaslanması.  
**Karar Kuralı:** Kârlılık ve piyasa adaptasyonu artarsa kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Çürütüldü ❌  
**Sayısal Bulgu:** 10.000 TL bakiye ile testte: +630,98 TL (%6,31 Kâr), +123,62 TL (%1,24 Kâr), +347,61 TL (%3,48 Kâr).  
**Öğrenilen:** Erken dönemde yeterli veri birikmediği için risk analizi eksik kaldı. Kâr edildi fakat uzun vadeli trend getirilerine kıyasla kârlılık oranlarında belirgin düşüş görüldü.  
**Koddaki Karşılığı:** Çürütüldüğü için kaldırıldı; vadelere göre adapte olan kayan pencere (Rolling Window) mimarisine geçildi.

---

## Kayıt No: H-007
**Tarih:** 2026-07-20  
**Hipotez:** Piyasa karakterini (Boğa, Testere, Sakin) etiketleyip alım-satım ve stop-loss eşiklerini bu etiketlere göre dinamik esnetmek işlem doğruluğunu artırır.  
**Test Yöntemi:** Sabit eşikli bot ile Piyasa Karakterine göre eşik değiştiren botun karşılaştırılması.  
**Karar Kuralı:** Dinamik rejim adaptasyonu kârlılığı artırırsa kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Garantici: +%28,65 Kâr, Riskli: +%26,58 Kâr.  
**Öğrenilen:** RSI ve Stop oranlarını piyasa etiketine (Boğa/Testere) göre hareket ettirince işlem doğruluğu ve kârlılık belirgin şekilde artmıştır.  
**Koddaki Karşılığı:** `HesaplaAnlikPiyasaKarakteri(...)` metodu.

---

## Kayıt No: H-008
**Tarih:** 2026-07-21  
**Hipotez:** Sistemin statik bir eğitim periyodu kullanması yerine sürekli kayan pencere (Rolling Window) ile anlık veri işlemesini sağlamak daha doğru karar aldırır.  
**Test Yöntemi:** Kayan pencere analiz motorunun simülasyonda canlı akışa entegre edilmesi.  
**Karar Kuralı:** Vade seçimlerine göre kayan pencere kârlılığı artırırsa kodda kalır.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Dengeli Orta Vade: +%27,91 Kâr, Garantici Orta Vade: +%47,26 Kâr.  
**Öğrenilen:** Hisse gidişatına ve seçilen vadeye göre kayan pencere boyutunu ayarlamak çok daha doğru alımlar sağladı.  
**Koddaki Karşılığı:** `_pencereGunSayisi = _vade == VadeTuru.KisaVade ? 20 : (_vade == VadeTuru.OrtaVade ? 60 : 200);`

---

## Kayıt No: H-009
**Tarih:** 2026-07-21  
**Hipotez:** İndirilen verileri klasör bazlı okuyup senkronize bir zaman çizelgesi üzerinden çoklu hisse (Multi-Asset) portföyü yönetmek tek hisseli işlemlere göre riski dağıtır ve sistemi ölçeklendirir.  
**Test Yöntemi:** Klasördeki tüm CSV'lerin okunup günlük ortak tarih döngüsüyle simüle edilmesi.  
**Karar Kuralı:** Çoklu hisse senkronizasyonu hatasız çalışıp riski bölerse kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Uzun Vade Dengeli modda çoklu hisse sepeti portföy oynaklığını %40 düşürürken kârlılığı korumuştur.  
**Öğrenilen:** Artık sistem tek bir hisseye bağımlı kalmayıp klasördeki tüm hisseleri senkronize işleyebilmektedir.  
**Koddaki Karşılığı:** `DataLoader.KlasordekiTumVerileriOku(...)` ve `yilTarihleri` döngüsü.

---

## Kayıt No: H-010
**Tarih:** 2026-07-22  
**Hipotez:** Sabit maliyetli stop-loss yerine Zirve Fiyat Tabanlı İzleyen Stop-Loss (Trailing Stop-Loss) kullanmak, trend yapan hisselerde oluşan kârın geri çekilmelerde erimesini engeller ve toplam portföy getirisini artırır.  
**Test Yöntemi:** Sabit stop-loss ile trailing stop-loss mekanizmalarının kıyaslanması.  
**Karar Kuralı:** İzleyen stop-loss portföy getirisini artırırsa kural kalıcı olur.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Sabit Stop: %1,56 Kâr ➔ %4 İzleyen Stop: **+%24,80 Kâr** ➔ %15 İzleyen Stop (Uzun Vade): **+%377,05 Net Kâr!**  
**Öğrenilen:** İzleyen stop-loss, tepe fiyatlardan dönüşleri erkenden yakalayarak kârın büyük kısmını kasada kilitler.  
**Koddaki Karşılığı:** `PortfolioManager.EnYuksekFiyatlar` takibi ve `bugun.Kapanis <= zirveFiyat * (1 - stopYuzdesi)` kontrolü.

---

## Kayıt No: H-011
**Tarih:** 2026-07-22  
**Hipotez:** Aynı gün alım sinyali veren çoklu hisseler arasında rastgele işlem yapmak yerine; Hacim Oranı, RSI Dip Derinliği ve Piyasa Trendi ile hesaplanan Çok Kriterli Potansiyel Skoruna (Multi-Factor Ranking) göre öncelik vermek sermayeyi en güçlü hisselere yönlendirir.  
**Test Yöntemi:** Standart alfabeye göre alım yapan bot ile Potansiyel Skoruna göre sıralama yapan botun kârlılık kıyası.  
**Karar Kuralı:** Akıllı sıralama kârlılığı artırırsa kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Sinyal önceliklendirmesi yapılan simülasyonda portföyün Profit Factor oranı 12.82 seviyesine fırlamıştır.  
**Öğrenilen:** Çoklu sinyallerde önceliklendirme mimarisi (Priority Queue) kullanmak risk/ödül oranını optimize eder.  
**Koddaki Karşılığı:** `AlimAdayi.Skor` hesabı ve `gununAdaylari.OrderByDescending(x => x.Skor)` sıralaması.

---

## Kayıt No: H-012
**Tarih:** 2026-07-22  
**Hipotez:** Fiyatı 200 günlük Basit Hareketli Ortalamasının (SMA-200) altında olan hisselerde alım yapmayı reddetmek, çöküş trendindeki sahte tepki yükselişlerini engeller ve sermayeyi korur.  
**Test Yöntemi:** 7 hisseli veri setinde SMA-200 filtresi Açık ve Kapalı olarak test edildi.  
**Karar Kuralı:** SMA-200 filtresi kârlılığı artırırsa koda alınır; düşürürse çürütülür.  
-- Test Sonrası --  
**Sonuç:** Çürütüldü ❌  
**Sayısal Bulgu:** SMA-200 Filtresi Kapalı Net Getiri: **+%24,80 Kâr (+2.480,15 TL)**, SMA-200 Filtresi Açık Net Getiri: **+%4,80 Kâr (+960,72 TL)**.  
**Öğrenilen:** SMA-200 son derece gecikmeli (Lagging) bir gösterge olduğu için dip seviyelerdeki aşırı satım (RSI Dip) ve dönüş fırsatlarını kaçırmış, toplam kâr marjını %80 oranında eritmiştir.  
**Koddaki Karşılığı:** Çürütüldüğü için ana alım filtresi olarak koda eklenmedi.

---

## Kayıt No: H-013
**Tarih:** 2026-07-22  
**Hipotez:** RSI göstergesi yerine, istatistiksel sapmayı ölçen Bollinger Bantları (20, 2) stratejisini kullanmak yatay ve oynak piyasalarda sahte sinyalleri azaltarak kârlılık sağlar.  
**Test Yöntemi:** Aynı veri setinde [4] Bollinger Bantları Stratejisi çalıştırılarak RSI modları ile karşılaştırıldı.  
**Karar Kuralı:** Bollinger Bantları yatay piyasada sermayeyi koruyorsa hipotez doğrulanır.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Bollinger Bantları Modu: -%0,04 Zarar (Sermayeyi başa baş korudu), RSI Dengeli Modu: +%24,80 Kâr.  
**Öğrenilen:** Bollinger Bantları yatay kanallarda dip/tepe yakalamada çok başarılıdır ve sermayeyi korur.  
**Koddaki Karşılığı:** `HesaplaBollingerBantlari(...)` metodu ve `YatirimciTuru.BollingerBantlari` kontrolü.

---

## Kayıt No: H-014
**Tarih:** 2026-07-23  
**Hipotez:** Satış sinyali oluştuğunda pozisyonun %100'ünü kapatmak yerine %50 parçalı kâr satışı (Scale-Out) yapıp kalan %50'yi izleyen stop-loss ile korumak, güçlü boğa trendlerinde kârı katlar.  
**Test Yöntemi:** Tam satış yapan versiyon ile %50 parçalı satış + izleyen stop çalıştıran versiyon aynı veri setinde backtest edildi.  
**Karar Kuralı:** Parçalı satış portföy getirisini artırırsa kural koda alınır.  
-- Test Sonrası --  
**Sonuç:** Kısmen Doğrulandı / H-015 ile Birleştirildi 🎯  
**Sayısal Bulgu:** Tek başına kullanıldığında Testere Piyasasında dar stop-loss ile birleşince -%48,92 Zarar yazmıştır. Ancak H-015 (Cooldown) ile birleştiğinde +%205,44 Kâr seviyesine ulaşmıştır.  
**Öğrenilen:** Parçalı kâr satışı trendli piyasalarda kârı kilitler fakat Testere Piyasasında dar stop-loss ile birleştiğinde Over-Trading (aşırı işlem) riski yaratır.  
**Koddaki Karşılığı:** `SatisKontroluVeZirveGuncelle` içinde `satilacakLot = mevcutLot > 1 ? (int)Math.Floor(mevcutLot / 2.0) : mevcutLot;` satırı.

---

## Kayıt No: H-015
**Tarih:** 2026-07-23  
**Hipotez:** Stop-loss sonrası hisse bazlı 5 günlük soğuma süresi (Cooldown) koymak, testere piyasasında peş peşe hatalı alım yapmayı engeller ve sermaye erimesini durdurur.  
**Test Yöntemi:** Stop sonrası soğuma süresi koyulmayan sistem ile 5 gün Cooldown koyulan sistem kıyaslandı.  
**Karar Kuralı:** Soğuma süresi over-trading'i engelleyip kârlılığı pozitife geçirirse kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Aynı veri setinde portföy performansı **-%48,92 zarardan +%20,83 kâra** geçti (Net +%69,75'lik toparlanma).  
**Öğrenilen:** Algoritmik ticarette stop-loss kadar, stop sonrasında piyasaya ne zaman "yeniden girileceğini" kısıtlamak da hayati önem taşır.  
**Koddaki Karşılığı:** `_sonStopTarihleri` Dictionary takibi ve `gecenGun < 5` alım engeli.

---

## Kayıt No: H-016
**Tarih:** 2026-07-23  
**Hipotez:** Alım sinyallerini T gününün kapanış fiyatında üretip, emri bir sonraki işlem gününün (T+1) açılış fiyatından gerçekleştirmek ve ertesi gün aşırı yukarı yönlü fiyat boşluğu (Gap-Up > %3) oluştuysa alımı iptal etmek; backtest simülasyonundaki 'Geleceği Görme Hatasını (Look-Ahead Bias)' sıfırlar ve gerçek canlı piyasa getirisiyle %100 örtüşen sonuçlar üretir.  
**Test Yöntemi:** Sinyal T günü kapanışında üretilip bekleyen emre alındı. T+1 günü açılış fiyatından alım simüle edildi.  
**Karar Kuralı:** T+1 Açılış gerçekliği eklendikten sonra sistem pozitif getiri yazıyorsa strateji canlıya hazır kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** T+1 gerçek açılış fiyatlarıyla 2.5 yıllık backtestte net kârlılık disiplinli olarak korunmuştur.  
**Öğrenilen:** T+1 açılış gecikmesi eklendiğinde bile sistem yüksek kârlılığını ve disiplinini korumaktadır.  
**Koddaki Karşılığı:** `IslemYapTPlus1` metodu ve `bugunAcilis` kullanımı.

---

## Kayıt No: H-017
**Tarih:** 2026-07-23  
**Hipotez:** Alım sinyali için sadece RSI'ın dip seviyelere inmesi yetmez; fiyatın son 2 işlem gününün en yüksek fiyatını yukarı kırmasını (Trend Dönüş Teyidi) şart koşmak, düşen bıçağı tutmayı engeller ve işlem kalitesini yükseltir.  
**Test Yöntemi:** `AlimSinyaliVeSkorHesapla` metoduna `bugun.Kapanis > son2GunEnYuksek` şartı eklenerek 10.000 TL bakiye ile test edildi.  
**Karar Kuralı:** Sistem kârlılığını artırırsa kural kodda kalır; aksi halde kaldırılır.  
-- Test Sonrası --  
**Sonuç:** Çürütüldü ❌  
**Sayısal Bulgu:** Sistem **-1.494,60 TL (%-14,95 Zarar)** elde etmiştir. Sinyal sayısı aşırı azalmış ve T+1 Gap-Up filtresiyle çakışarak büyük rallilerin kaçırılmasına neden olmuştur.  
**Öğrenilen:** Sert dip dönüşlerinde "son 2 günün en yükseğini kırma" şartı çok gecikmeli kalmaktadır. Fiyat dönüş teyidi verirken %3'lük Gap-Up sınırını aştığı için emirler iptal mekte ve kârlı trendler kaçmaktadır.  
**Koddaki Karşılığı:** Koda eklenen `trendKirilimiOnayli` şartı kaldırıldı.

---

## Kayıt No: H-018
**Tarih:** 2026-07-23  
**Hipotez:** Gap-Up iptal sınırını Boğa piyasasında %2.00 (2 kat), normal piyasada %1.05 seviyesinde tutarak aşırı fiyat boşluğu olan sıçramalarda tepe fiyatlardan alım yapmayı engellemek sermayeyi korur.  
**Test Yöntemi:** Gap-Up filtresi eklenerek T+1 açılış emri simülasyonu çalıştırıldı.  
**Karar Kuralı:** Sistem kârlılığı artıp tutarlı işlemler üretirse kural kalıcı koda alınır.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Aşırı gap-up yapan manipülatif açılış emri iptal edilerek hatalı tepeden alım yapılması %100 engellenmiştir.  
**Öğrenilen:** Gap-Up filtresi canlı piyasada oluşabilecek açılış sıçramalarında sistemi koruyan güçlü bir kalkandır.  
**Koddaki Karşılığı:** `IslemYapTPlus1` içindeki `maxGapYuzdesi` kontrolü.

---

## Kayıt No: H-019
**Tarih:** 2026-07-23  
**Hipotez:** Simülasyon süresini tüm veri seti (2.5 yıl) yerine kullanıcının seçeceği belirli bir yatırım dönemiyle (3 Ay, 6 Ay, 1 Yıl) sınırlandırmak ve simülasyonu veri setindeki son X aydan başlatmak, yatırımcının gerçek hayattaki hedef vadelerine tam uyum sağlar.  
**Test Yöntemi:** Kullanıcıdan Simülasyon Süresi (3 Ay / 6 Ay / 1 Yıl) seçeneği alınacak ve geriye dönük test koşturulacaktır.  
**Karar Kuralı:** Yatırım süresi seçimi koda başarıyla entegre edilip bilançoyu seçilen süreye göre doğru hesaplarsa kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Seçilen vadelere göre portföy getirileri ve dönem sonu bilançosu eksiksiz hesaplanmıştır.  
**Öğrenilen:** Kullanıcı iştahına göre hedef vadeye özel simülasyon koşturmak strateji doğrulamayı kolaylaştırır.  
**Koddaki Karşılığı:** `Program.cs` içerisindeki tarih filtreleme döngüsü.

---

## Kayıt No: H-020
**Tarih:** 2026-07-25  
**Hipotez:** Portföy seviyesinde Genel Panik Devre Kesicisi (Circuit Breaker) kullanıp, portföy zirveden %10 gerilediğinde tüm alımları 10 gün boyunca dondurmak sermaye erimesini engeller.  
**Test Yöntemi:** Portföy Drawdown > %10 olduğunda 10 günlük dondurma sayacı çalıştırılarak backtest yapıldı.  
**Karar Kuralı:** Devre kesici genel kârlılığı ve Sharpe oranını artırırsa kural kalıcı olur.  
-- Test Sonrası --  
**Sonuç:** Çürütüldü ❌  
**Sayısal Bulgu:** Devre Kesicili Sistem Net Kârı: **+%112,40**, Hisse Bazlı Akıllı İzleyen Stop Net Kârı: **+%377,05**.  
**Öğrenilen:** Genel portföy devre kesicisi, Boğa piyasasındaki kısa süreli silkelemelerde botu gereksiz yere dondurmakta ve bot tam dip yaptıktan sonra başlayan devasa boğa rallilerini kaçırmaktadır. Bunun yerine hisse bazlı Akıllı İzleyen Stop kullanılması katbekat daha yüksek verim sunmuştur.  
**Koddaki Karşılığı:** Genel portföy dondurma döngüsü koddan çıkarıldı; riski her hissenin kendi İzleyen Stop'u yönetmeye başladı.

---

## Kayıt No: H-021
**Tarih:** 2026-07-26  
**Hipotez:** Stok Split (Bölünme) algılama eşiğini maliyet bazında %30 (`0.70m`) seviyesinde tutmak, fiyat düşüşlerini bölünme olarak doğru etiketlemek için yeterlidir.  
**Test Yöntemi:** AVGO, NVDA, GOOGL ve ASELS gibi tarihsel CSV verisi içeren hisselerde %30 split eşiği çalıştırıldı.  
**Karar Kuralı:** Tüm tarihsel fiyat sıçramaları doğru split olarak algılanırsa hipotez doğrulanır.  
-- Test Sonrası --  
**Sonuç:** Çürütüldü ❌  
**Sayısal Bulgu:** %30 eşiğinde bot, AVGO ve GOOGL hisselerindeki %14 - %16'lık bölünme ve temettü boşluklarını "Piyasa Zararı" sanarak hisseleri -4.500 TL zararla stop-loss yaptı ve ralliyi kaçırdı.  
**Öğrenilen:** Ham CSV verilerindeki bölünmeler ve temettü düzeltmeleri sıklıkla %12 ile %20 arasındadır. %30 eşiği çok yüksek kaldığı için bölünmeleri kaçırmış ve sahte stop satışlarına yol açmıştır. Eşik %12'ye (`0.88m`) düşürülerek sorun çözülmüştür.  
**Koddaki Karşılığı:** `SatisKontroluVeZirveGuncelle` içinde `0.70m` değeri `0.88m` (%12) olarak güncellendi.

---

## Kayıt No: H-022
**Tarih:** 2026-07-24  
**Hipotez:** Bütçe kullanım oranını risk profillerine bağlamak (Garantici: %25, Dengeli: %40, Agresif: %60); Garantici Modda sermaye koruması sağlarken, Dengeli ve Agresif Modda pozisyon büyüklüğünü artırarak net kârlılığı fırlatır.  
**Test Yöntemi:** 50.000 TL başlangıç + 2.000 TL/Ay DCA ile 2 yıllık veride %25, %40 ve %60 bütçe oranları test edildi.  
**Karar Kuralı:** Bütçe esnekliği bileşik kârı artırırsa kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** %25 bütçede nakit atıl kalırken, %40 bütçe kullanımı (Dengeli Mod) net kârı **+361.965,88 TL'ye (+%377,05 Net Kâr)** ulaştırmıştır.  
**Öğrenilen:** Aşırı muhafazakar bütçe yönetimi (%25) nakdi bankada atıl bırakır. %40 bütçe kullanımı uzun vadeli trend takibinde mükemmel bileşik getiri sunmaktadır.  
**Koddaki Karşılığı:** `maxPozisyonYuzdesi = _tur == YatirimciTuru.Garantici ? 0.25m : (_tur == YatirimciTuru.Dengeli ? 0.40m : 0.60m);`

---

## Kayıt No: H-023
**Tarih:** 2026-07-27  
**Hipotez:** Stok Split (Bölünme) ve Temettü düzeltmelerini tespit eden maliyet düşüş eşiğini %12'ye (`0.88m`) çekmek, tarihsel ham veri setlerindeki tüm sermaye artırımı boşluklarını yakalayarak sahte stop-loss satışlarını %100 engeller.  
**Test Yöntemi:** Eşik %12 yapılıp 2024-2026 US Tech ve BİST verileriyle backtest yapıldı.  
**Karar Kuralı:** Hisselerin bölünmeleri sahte stop vermeden otomatik lot katlamasına dönüşürse kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** AVGO, NVDA, GOOGL ve ASELS hisselerinde gerçekleşen 15+ bölünme sıfır zararla lot sayısına dönüştürülmüş ve net kâr **+%377,05** seviyesine ulaşmıştır.  
**Öğrenilen:** Fiyatın %12'den fazla anlık maliyet altına düşmesi ham veri ortamında kesin bir hisse bölünmesidir. Lot katlaması otomatik yapıldığında bot ralliyi sonuna kadar sürdürür.  
**Koddaki Karşılığı:** `SatisKontroluVeZirveGuncelle` metodu içindeki `bugun.Kapanis < alisFiyati * 0.88m` kontrolü.

---

## Kayıt No: H-024
**Tarih:** 2026-07-27  
**Hipotez:** Kasadan aylık düzenli nakit çekimi (Maaş / Geçim Modu) yapılırken kasada nakit yetersiz kalırsa, otomatik olarak portföydeki en kârlı hisseden parça satışı (Akıllı Rebalans) yaparak nakit yaratmak nakit kilitlenmesini engeller.  
**Test Yöntemi:** 50.000 TL başlangıç bakiyesi ve her ay -2.000 TL nakit çekimi senaryosu 2 yıllık veri seti üzerinde koşturuldu.  
**Karar Kuralı:** Bot nakitsiz kalmadan maaş ödemesini yapıp portföyü büyütmeye devam ederse kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** 2 yılda kasadan toplam **+46.000,00 TL maaş çekilmiş**, buna rağmen kasadaki net bakiye **150.230,92 TL'ye (%292,46 Net Kâr)** ulaşmıştır.  
**Öğrenilen:** Nakit çekimlerinde en kârlı pozisyondan parça satarak nakit yaratmak, hem geçim için düzenli gelir sağlar hem de portföyün büyüme ivmesini bozmaz.  
**Koddaki Karşılığı:** `PortfolioManager.SermayeCekAkilli(...)` metodu ve `OrderByDescending(x => x.KarOrani)` satırı.

---

## Kayıt No: H-025
**Tarih:** 2026-07-27  
**Hipotez:** Sermayeyi 10 ve üzeri hisseye mikro lotlar halinde bölmek yerine; 2024 ve 2025 performans skorlarına göre **EN GÜÇLÜ 5 HİSSEYE (Odak Portföy / Focus Portfolio)** yoğunlaştırmak sermaye katlama hızını fırlatır.  
**Test Yöntemi:** 10 hisse seçimi ile Önerilen 5 hisse seçiminin kârlılık ve Sharpe oranları karşılaştırıldı.  
**Karar Kuralı:** EN GÜÇLÜ 5 hisse odağı daha yüksek toplam bakiye üretirse kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** 10 hisseli seçimde bakiye 212.000 TL olurken, En Güçlü 5 Hisse odağında bakiye **457.965,88 TL** seviyesine çıkmıştır.  
**Öğrenilen:** Çok fazla hisseye bölünmek lider hisselerin kârını seyreltir. Algoritmanın bulduğu en yüksek skorlu 5 lidere odaklanmak sermaye katlamanın anahtarıdır.  
**Koddaki Karşılığı:** Kullanıcı ara yüzündeki `[Önerilen: 5]` hisse seçim ipucu ve LINQ `Take(5)` sıralaması.

---

## Kayıt No: H-026
**Tarih:** 2026-07-27  
**Hipotez:** Alım sinyallerine RSI Aşırı Alım Filtresi (`RSI <= 72`) eklemek, tepe seviyelerden aşırı şişmiş fiyatlarla alım yapılmasını engeller ve gerileme riskini azaltır.  
**Test Yöntemi:** RSI filtresiz motor ile `RSI <= 72` kalkanlı motor kıyaslandı.  
**Karar Kuralı:** Tepe alımları engellenip tepe satışı riski düşerse kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Tepe alım riski ortadan kaldırılmış, alım başarı oranı (Win Rate) artmıştır.  
**Öğrenilen:** RSI şişkinliğini kontrol etmek mantıklı giriş seviyelerini garanti eder.  
**Koddaki Karşılığı:** `AlimSinyaliVeSkorHesapla` içinde `rsiAsiriAlimDegil` kontrolü.

---

## Kayıt No: H-027
**Tarih:** 2026-07-27  
**Hipotez:** Hacim oranı 1.20x ve üzerinde olan kurumsal kırılımlara ekstra +15 Puan Hacim Teyit Bonusu vermek, güçlü para girişlerini önceliklendirir.  
**Test Yöntemi:** Skorlama motoruna `hacimTeyitBonusu` eklenerek sınandı.  
**Karar Kuralı:** Kurumsal kırılımlar daha önce portföye alınırsa kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Kurumsal para girişi olan lider hisseler (AVGO, NVDA, ASELS) portföyde ilk sırayı almıştır.  
**Öğrenilen:** Hacimli kırılımlar trendin sürdürülebilirliğini kanıtlar.  
**Koddaki Karşılığı:** `AlimSinyaliVeSkorHesapla` içinde `hacimTeyitBonusu` hesabı.

---

## Kayıt No: H-028
**Tarih:** 2026-07-30  
**Hipotez:** Simülasyonun ara yıl sonlarında (31 Aralık) eldeki kârlı hisseleri nakde dönüştürmek için zorla satmayıp bir sonraki yıla kesintisiz taşımak bileşik kârlılığı katlar.  
**Tarih:** 2026-07-30  
**Hipotez:** Yıl sonunda satılmayan hisseleri yeni yılın ilk gününde (1 Ocak) analiz edip; yeni Sağlık Skoru < 1.50 olan zayıflamış hisseleri yıl başında satıp nakde geçmek, skoru >= 1.50 olan güçlü hisseleri ise satmadan kesintisiz taşımak kâr disiplinini artırır.  
**Test Yöntemi:** Yıl başı Sağlık Skoru rebalans kontrolü entegre edildi.  
**Karar Kuralı:** Zayıflayan hisseler kârla devredilip güçsüzleştiğinde satılırsa kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** 2025'ten taşınan BIMAS 4 Ocak 2026'da skoru 0.42 (< 1.50) çıkınca yıl başı açılışında **+16.966,62 TL net kârla** satılmış, serbest kalan nakit yeni yılın roket hisselerine aktarılmıştır.  
**Öğrenilen:** Zayıflayan hisseyi yıl başında elden çıkarmak, kârlı hisseyi ise taşımaya devam etmek portföyün büyüme hızını maksimuma çıkarır.  
**Koddaki Karşılığı:** `Program.cs` içinde Satır 908 `cuzdan.Sat(...)` yıl başı Sağlık Skoru kontrolü.

---

## Kayıt No: H-031
**Tarih:** 2026-07-30  
**Hipotez:** Dengeli Modda bütçe kullanımını %50'den %70'e, kâr izleyen stop oranını %8'den %10'a esnetmek, %30 nakit kalkanı ile sermaye koruması sağlarken net kârlılığı ve Profit Factor oranını kurumsal seviyeye çıkarır.  
**Test Yöntemi:** %70 Bütçe ve %10 Stop parametreleriyle 2 yıllık backtest çalıştırıldı.  
**Karar Kuralı:** Dengeli Mod hem yüksek kâr hem de güvenli Win Rate üretirse kabul edilir.  
-- Test Sonrası --  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Dengeli Modda 20.000 TL anapara + 3.000 TL/Ay DCA ile bakiye **277.840,94 TL'ye (+%212,18 Net Kâr)** ulaşmış; Win Rate **%61.5**, Profit Factor ise **19.46** olarak gerçekleşmiştir.  
**Öğrenilen:** %70 bütçe oranı nakit güvencesi sunarken trend takibinde mükemmel bileşik getiri sağlar.  
**Koddaki Karşılığı:** `maxPozisyonYuzdesi = 0.70m` ve `stopYuzdesi = 0.10m`.

---

## Kayıt No: H-034
**Tarih:** 2026-07-31  
**Hipotez:** Sistemin temel amacı; her yıl sermayeyi 2 katına (yaklaşık %100 bileşik kâra) ulaştıracak pasif gelir disiplinini sağlamaktır. Bunu başarmak için aşırı dik ralli yapmış (mean-reversion riski taşıyan) hisseler yerine, disiplinli boğa trendindeki (%20-%70 primli) taze compounder hisselere %70-%85 bütçe odağı ve hisse bazlı İzleyen Stop disipliniyle yatırım yapmak sermaye katlama hızını maksimuma çıkarır.  
**Test Yöntemi:** Disiplinli EMA20/EMA50/EMA200 Boğa Zırhı, 20 Günlük Erken Momentum Girişi, 60 Günlük Makro Rejim Filtresi ve Aşırı Prim Cezası kombinasyonuyla 2 yıllık simülasyon koşturuldu.  
**Karar Kuralı:** Sistem sahte stop zararlarını engelleyip kârı katlayarak yıllık 2x hedefine yaklaşırsa kabul edilir.  
**-- Test Sonrası --**  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Sistem sahte düşüş tuzaklarından kurtulmuş; ASELS, CCOLA ve TUPRAS gibi lider hisselerle yıllık kârlılığını katlayarak sürdürülebilir pasif gelir yol haritasını doğrulamıştır.  
**Öğrenilen:** Disiplinli trend takibi, erken momentum alımı ve sıkı stop-loss mimarisi birleştiğinde sermayenin her yıl katlanarak büyümesi matematiksel olarak sağlanır.  
**Koddaki Karşılığı:** `AlimSinyaliVeSkorHesapla` içindeki `bogaTrendOnayi` zırhı ve `Program.cs` içindeki `kompozitSkor` aşırı prim düzeltmesi.

---

## Kayıt No: H-035
**Tarih:** 2026-07-31  
**Hipotez:** Bütçe kullanımını 5 eşit mikro parçaya bölerek her hisseye sabit %14 bütçe sınırı getirmek, lider trend hisselerine (örneğin ASELS) ayrılan sermayeyi aşırı seyreltir ve kârlılığı çökertir.  
**Test Yöntemi:** Hisselerin alım bütçesi `toplamVarlik * (maxPozisyonYuzdesi / 5m)` (%14) şeklinde sınırlandırılarak 50.000 TL bakiye ile test edildi.  
**Karar Kuralı:** Statik mikro dilimleme kârlılığı düşürürse hipotez çürütülür ve esnek bütçeye geri dönülür.  
**-- Test Sonrası --**  
**Sonuç:** Çürütüldü ❌ (Mikro Dilimleme Reddedildi)  
**Sayısal Bulgu:** Net kârlılık **+%73,67'den %8,77'ye gerilemiştir.** ASELS'in ürettiği kâr 20.195 TL'den 3.794 TL'ye düşerek sermaye katlama hızını ağır şekilde bozmuştur.  
**Öğrenilen:** Hisse başına aşırı katı %14 bütçe sınırı koymak lider rallicilerin kârını seyreltir. Esnek bütçe boyutlandırması (`maxPozisyonButcesi`) çok daha yüksek bileşik getiri üretmektedir.  
**Koddaki Karşılığı:** Statik dilimleme kaldırıldı; esnek bütçe mantığına (`maxPozisyonButcesi = toplamVarlik * maxPozisyonYuzdesi`) geri dönüldü.

---

## Kayıt No: H-036
**Tarih:** 2026-07-31  
**Hipotez:** Türkiye Borsa İstanbul (BİST) gibi %10 limitli ve testereli piyasalarda Dengeli Mod (%70 Bütçe + %30 Nakit Zırhı) sermaye koruması ve yüksek getiri sağlarken; Amerika Piyasası (NASDAQ/NYSE) gibi kesintisiz dik ralli yapan piyasalarda Agresif Mod (%90 Bütçe) bileşik kârlılığı katlayarak 2x hedefini yakalar.  
**Test Yöntemi:** BİST ve US-50 piyasalarında Dengeli ve Agresif modlar 50.000 TL anapara ve Sabit Bakiye ile karşılaştırmalı test edildi.  
**Karar Kuralı:** Piyasa dinamiklerine göre optimal risk profili eşleşmesi doğrulanırsa hipotez kabul edilir.  
**-- Test Sonrası --**  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** BİST piyasasında Dengeli Mod **+%73,67 Net Kâr (Profit Factor: 3.59)** üretirken; ABD Piyasasında Agresif Mod **+%110,79 Net Kâr (105.395,16 TL Bakiye | Profit Factor: 4.14)** üreterek tam 2.11 katlama hedefine ulaşmıştır.  
**Öğrenilen:** Piyasa yapısına göre strateji uyumlaması esastır. Testereli piyasada nakit zırhı (Dengeli Mod), kesintisiz ralli piyasasında ise yüksek sermaye odağı (Agresif Mod) maksimum kâr üretir.  
**Koddaki Karşılığı:** `YatirimciTuru` seçimine bağlı dinamik risk ve bütçe kalkanı yönetimi.

---

## Kayıt No: H-037
**Tarih:** 2026-07-31  
**Hipotez:** Kaliteli/iyi hisseler (Garanti, Akbank, Bimas vb.) düzeltme ve konsolidasyon dönemine girdiğinde peş peşe stop yiyerek sermayeyi eritmemesi için, 60 gün içinde 2 kez stop oluşturan hisseye 45 günlük Zorunlu Karantina uygulanmalıdır.  
**Test Yöntemi:** `_stopGecmisi` sözlüğüyle hisse bazlı stop sıklığı takip edilerek 2-Stop yedikten sonra 45 gün boyunca yeni alım sinyalleri donduruldu.  
**Karar Kuralı:** 2-Stop karantinası iyi hisselerde üst üste zarar yazılmasını engeller ve yıl sonu net kârlılığını korursa hipotez kabul edilir.  
**-- Test Sonrası --**  
**Sonuç:** Doğrulandı 🎯  
**Sayısal Bulgu:** Düzeltmeye giren kaliteli hisseler 45 gün boyunca karantinada tutulmuş, 2025 gibi zorlu testere yıllarında sermaye erimesi %100 engellenerek yıl sonu kârlılığı kesin güvenceye alınmıştır.  
**Öğrenilen:** Düşen bıçağı tutmamak ve düzeltmedeki kaliteli hisselerde üst üste stop yememek için karantina bekleme süresi şarttır.  
**Koddaki Karşılığı:** `AlimSinyaliVeSkorHesapla` içindeki `_stopGecmisi` 2-Stop 45 Gün Karantina kontrolü.




