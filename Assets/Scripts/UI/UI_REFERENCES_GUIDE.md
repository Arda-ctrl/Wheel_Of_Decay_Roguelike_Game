# UI References ve Main Menu Navigation Rehberi

## 🎯 UI References Bölümü

### **UI References Nedir?**
Bu bölüm, MainMenuManager'ın diğer manager sınıflarıyla iletişim kurmasını sağlar.

```csharp
[Header("UI References")]
[SerializeField] private UI_Manager uiManager;
[SerializeField] private SaveManager saveManager;
[SerializeField] private AudioManager audioManager;
[SerializeField] private SettingsManager settingsManager;
```

### **Her Reference'ın Görevi:**

#### **1. UI_Manager**
- **Görev:** Genel UI yönetimi
- **Kullanım:** `uiManager.StartGame()` - Oyunu başlatır
- **Unity Editor'da:** UI Canvas'taki UI_Manager component'ini sürükle

#### **2. SaveManager**
- **Görev:** Oyun kaydetme/yükleme
- **Kullanım:** `saveManager.CreateNewGame()`, `saveManager.LoadGame()`
- **Unity Editor'da:** SaveManager GameObject'ini sürükle

#### **3. AudioManager**
- **Görev:** Ses yönetimi
- **Kullanım:** `audioManager.PlaySFX()`, `audioManager.SetVolume()`
- **Unity Editor'da:** AudioManager GameObject'ini sürükle

#### **4. SettingsManager**
- **Görev:** Ayar yönetimi (JSON tabanlı)
- **Kullanım:** `settingsManager.SetBrightness()`, `settingsManager.SetVolume()`
- **Unity Editor'da:** SettingsManager GameObject'ini sürükle

### **Unity Editor'da Atama:**
1. **MainMenuManager** component'ini seç
2. **UI References** bölümünü bul
3. **Her field'a** ilgili GameObject'i sürükle:
   - `uiManager` → UI Canvas'taki UI_Manager
   - `saveManager` → SaveManager GameObject
   - `audioManager` → AudioManager GameObject
   - `settingsManager` → SettingsManager GameObject

## 🎮 Main Menu Navigation Bölümü

### **Main Menu Navigation Nedir?**
Bu bölüm, menü geçişlerini ve animasyonları kontrol eder.

```csharp
[Header("Menu Navigation")]
[SerializeField] private GameObject menuSelectionIndicator;
[SerializeField] private float menuTransitionDelay = 0.1f;
```

### **Her Elementin Görevi:**

#### **1. menuSelectionIndicator**
- **Görev:** Hangi menü öğesinin seçili olduğunu gösterir
- **Kullanım:** Klavye/Controller ile menüde gezinirken görsel feedback
- **Unity Editor'da:** Seçim göstergesi olan UI element'ini sürükle

#### **2. menuTransitionDelay**
- **Görev:** Panel geçişlerinde bekleme süresi
- **Değer:** 0.1f (100ms) - Ayarlanabilir
- **Kullanım:** Menü geçişlerinde smooth animasyon

### **Unity Editor'da Atama:**
1. **menuSelectionIndicator** için:
   - Menüde seçili öğeyi gösteren UI element'ini oluştur
   - (Örn: Parlak border, ok işareti, highlight)
   - Bu element'i MainMenuManager'a sürükle

2. **menuTransitionDelay** için:
   - Değeri ayarla (0.1f önerilen)
   - Düşük değer = Hızlı geçiş
   - Yüksek değer = Yavaş geçiş

## 🔧 Kod Kullanımı

### **UI References Kullanımı:**
```csharp
// Oyun başlatma
if (uiManager != null)
    uiManager.StartGame();

// Oyun kaydetme
if (saveManager != null)
    saveManager.SaveGame();

// Ses ayarlama
if (audioManager != null)
    audioManager.SetMasterVolume(0.8f);

// Brightness ayarlama
if (settingsManager != null)
    settingsManager.SetBrightness(0.7f);
```

### **Menu Navigation Kullanımı:**
```csharp
// Panel geçişi
StartCoroutine(TransitionToPanel(targetPanel));

// Seçim göstergesi güncelleme
if (menuSelectionIndicator != null)
    menuSelectionIndicator.transform.position = selectedButton.transform.position;
```

## 🎨 UI Tasarım Önerileri

### **Menu Selection Indicator:**
```
┌─────────────────┐
│  ▶ Continue     │ ← Ok işareti seçili öğeyi gösterir
│    New Game     │
│    Options      │
│    Extras       │
│    Quit         │
└─────────────────┘
```

### **Transition Delay Ayarları:**
- **0.05f** = Çok hızlı (60fps)
- **0.1f** = Normal (önerilen)
- **0.2f** = Yavaş (dramatik)
- **0.5f** = Çok yavaş (sadece özel durumlar)

## 🐛 Sorun Giderme

### **UI References Çalışmıyor:**
1. **Null Reference Exception** alıyorsan:
   - Unity Editor'da referansları kontrol et
   - `FindFirstObjectByType<>()` kullanılıyor mu kontrol et

2. **Manager'lar bulunamıyor:**
   - Scene'de ilgili GameObject'ler var mı?
   - Component'ler doğru atanmış mı?

### **Menu Navigation Çalışmıyor:**
1. **Selection Indicator görünmüyor:**
   - GameObject aktif mi?
   - Position doğru mu?
   - Sorting order yeterli mi?

2. **Transition çok hızlı/yavaş:**
   - `menuTransitionDelay` değerini ayarla
   - 0.1f önerilen başlangıç değeri

## ✅ Özet

### **UI References:**
- ✅ **Manager sınıfları** ile iletişim sağlar
- ✅ **Null check** ile güvenli çalışır
- ✅ **Unity Editor'da** kolay atama

### **Menu Navigation:**
- ✅ **Smooth geçişler** sağlar
- ✅ **Visual feedback** verir
- ✅ **Ayarlanabilir** delay süresi

Bu sistemler sayesinde menü sistemi profesyonel ve kullanıcı dostu olur! 🎮✨ 