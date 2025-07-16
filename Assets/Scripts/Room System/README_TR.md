# Oda Sistemi - Kapalı Bağlantılar

Wheel of Decay roguelike oyunu için dungeon oluşturma sistemi artık tüm kapı bağlantılarının doğru şekilde kapatıldığından emin olur.

## Kapalı Grafik Algoritması

Oda oluşturma sistemi, artık "kapalı grafik" algoritması kullanarak tüm kapıların düzgün şekilde bağlandığından veya kapatıldığından emin olur. Bu, hiçbir yere çıkmayan açık kapılardan kaynaklanan sorunları önler.

### Ana Bileşenler

1. **ImprovedDungeonGenerator.cs**: Ana dungeon oluşturma sınıfı, artık açık bağlantıları işleme işlevselliği ile güçlendirildi.
2. **DoorCap.cs**: Kullanılmayan kapıları görsel olarak kapatmak için prefablara eklenebilen bir bileşen.
3. **DoorCapSetup.cs**: Farklı yönlerde kapı kapaklarının oluşturulmasına yardımcı olan bir araç.
4. **DoorCapWizard.cs**: Unity Editor'da kapı kapaklarının kolayca oluşturulması için özel editor penceresi.

### Nasıl Çalışır

1. Dungeon oluşturulduktan sonra, sistem tüm odaları analiz ederek bağlantısız kapıları bulur.
2. Her açık bağlantı için:
   - Önce komşu bir odaya bağlanmaya çalışır
   - Bu başarısız olursa, yeni bir oda yerleştirmeyi dener
   - Bu da başarısız olursa, kapıyı atanan door cap prefabı ile kapatır

### Özel Boss Odası İşleme

Boss odası (son oda), tek bir girişi olan çıkmaz sokak olacak şekilde yapılandırılabilir. Bu, oyuncunun boss odasına girip başka çıkışlar olmadığı daha doğal bir akış yaratır.

## Kurulum Talimatları

### 1. Door Cap Prefabını Oluşturma

**Otomatik Yöntem (Önerilen)**:
1. Unity Editor'da "Room System" menüsünden "Door Cap Wizard" seçeneğine tıklayın
2. Cap Sprite alanına bir sprite atayın (duvar veya kapı engeli)
3. Sprite ve Collider boyutlarını ayarlayın
4. Room Generator alanına ImprovedDungeonGenerator'ı içeren GameObject'i atayın
5. "Create Door Cap Prefabs" düğmesine tıklayın
6. "Auto-Assign to Room Generator" düğmesine tıklayarak prefabı otomatik olarak atayın

**Manuel Yöntem**:
1. Yeni bir boş GameObject oluşturun
2. Kapalı bir kapıyı temsil eden görsel bileşen ekleyin (SpriteRenderer gibi)
3. `DoorCap` bileşenini ekleyin
4. Oyuncunun geçişini engellemek için gerekli collider'ı ayarlayın
5. Prefab olarak kaydedin

### 2. ImprovedDungeonGenerator'a Atama

1. ImprovedDungeonGenerator bulunan sahnenizi açın
2. Inspector'da "Connection Closure" bölümünü bulun
3. Door Cap Prefab alanına oluşturduğunuz door cap prefabını atayın
4. Boss odasının çıkmaz sokak olmasını istiyorsanız "Allow Boss Dead End" seçeneğini işaretleyin

### 3. Dungeon Oluşturmayı Test Etme

1. Oyunu çalıştırın
2. Tüm kapıların başka bir odaya bağlı veya düzgün şekilde kapatılmış olduğunu kontrol edin
3. Ayar etkinleştirilmişse, boss odasının yalnızca bir girişi olduğunu doğrulayın
4. Console penceresinde, açık bağlantıların nasıl işlendiğine dair detaylı log mesajlarını görebilirsiniz

## Sorun Giderme

**Açık kapılar hala görünüyor:**
- ImprovedDungeonGenerator'a bir door cap prefabı atandığından emin olun
- Door cap prefabının doğru boyutlarda ve konumda olduğunu kontrol edin
- Debug loglarını kontrol ederek hangi kapıların hala açık olduğunu görün

**Boss odası birden fazla kapıya sahip:**
- "Allow Boss Dead End" ayarının açık olduğundan emin olun
- Boss odası (isEndRoom = true) tanımlanmış olmalıdır

**Door Cap Wizard menüsü görünmüyor:**
- Scripts/Room System/Editor klasöründe DoorCapWizard.cs dosyasının bulunduğundan emin olun
- Unity Editor'ı yeniden başlatmayı deneyin 