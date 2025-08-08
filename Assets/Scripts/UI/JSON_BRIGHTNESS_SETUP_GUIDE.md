# JSON Tabanlı URP 2D Brightness Sistemi Kurulum Rehberi

## 🎯 Sistem Özellikleri

### **Ana Özellikler:**
- ✅ **UI Overlay Kontrolü** - Işık yerine UI Image ile brightness kontrolü
- ✅ **JSON Tabanlı Kayıt** - PlayerPrefs yerine JSON dosyası kullanımı
- ✅ **Screen Space - Overlay** - Canvas render mode desteği
- ✅ **Fade Efektleri** - Sahne geçişlerinde fade-in/fade-out
- ✅ **Tüm Ayarlar Tek Dosyada** - Ses, video, oyun ayarları JSON'da

## 📁 Gerekli Dosyalar

### **1. SettingsData.cs**
```csharp
// Assets/Scripts/Data/SettingsData.cs
// Tüm ayarları JSON'a serialize eden sınıf
```

### **2. SettingsManager.cs**
```csharp
// Assets/Scripts/Managers/SettingsManager.cs
// JSON tabanlı ayar yönetimi ve brightness kontrolü
```

### **3. MainMenuManager.cs (Güncellenmiş)**
```csharp
// Assets/Scripts/UI/MainMenuManager.cs
// SettingsManager ile entegre edilmiş menü sistemi
```

## 🛠️ Kurulum Adımları

### **1. SettingsManager Prefab Oluşturma**

1. **Hierarchy'de Yeni GameObject:**
   ```
   GameObject → Create Empty
   Name: "SettingsManager"
   ```

2. **SettingsManager Script Ekleme:**
   ```
   Add Component → Scripts → Managers → SettingsManager
   ```

3. **Inspector Ayarları:**
   - **Brightness Control** bölümünde UI elementlerini atayın
   - **Audio Controls** bölümünde slider'ları atayın
   - **Video Controls** bölümünde dropdown'ları atayın

### **2. Brightness Overlay Oluşturma**

**Otomatik Oluşturma (Önerilen):**
- SettingsManager otomatik olarak brightness overlay oluşturur
- Canvas yoksa otomatik oluşturur
- Screen Space - Overlay modunda çalışır

**Manuel Oluşturma:**
```
Canvas (Screen Space - Overlay)
└── Brightness Overlay (Image)
    ├── Color: Black (0,0,0,0)
    ├── Raycast Target: false
    └── RectTransform: Fill Screen
```

### **3. UI Elementlerini Bağlama**

**Brightness Panel:**
```
- Brightness Slider (0-1 range)
- Brightness Value Text (TextMeshPro)
- Back Button
```

**Audio Panel:**
```
- Master Volume Slider
- Sound Volume Slider  
- Music Volume Slider
- Reset Defaults Button
```

**Video Panel:**
```
- Resolution Dropdown
- Fullscreen Toggle
- V-Sync Toggle
- Particle Effects Dropdown
- Blur Quality Dropdown
- Brightness Button (Brightness Panel'i açar)
```

## 🎮 Kullanım

### **Brightness Kontrolü:**
```csharp
// Slider değeri değiştiğinde
SettingsManager.Instance.SetBrightness(sliderValue);

// Manuel brightness ayarı
SettingsManager.Instance.SetBrightness(0.5f);

// Brightness değerini alma
float brightness = SettingsManager.Instance.GetBrightness();
```

### **Fade Efektleri:**
```csharp
// Sahne geçişinde fade-in
SettingsManager.Instance.PlayFadeIn();

// Sahne geçişinde fade-out  
SettingsManager.Instance.PlayFadeOut();
```

### **Ayarları Sıfırlama:**
```csharp
// Tüm ayarları sıfırla
SettingsManager.Instance.ResetAllSettings();

// Sadece audio ayarlarını sıfırla
SettingsManager.Instance.ResetAudioSettings();

// Sadece video ayarlarını sıfırla
SettingsManager.Instance.ResetVideoSettings();
```

## 📄 JSON Dosya Yapısı

**settings.json örneği:**
```json
{
  "brightness": 0.8,
  "masterVolume": 1.0,
  "soundVolume": 0.7,
  "musicVolume": 0.9,
  "resolutionIndex": 2,
  "fullscreen": true,
  "vSync": false,
  "particleEffectsQuality": 1,
  "blurQuality": 1,
  "languageIndex": 0,
  "controllerEnabled": true,
  "keyboardEnabled": true
}
```

## 🔧 Teknik Detaylar

### **Brightness Çalışma Prensibi:**
1. **UI Image Overlay** - Tam ekran siyah Image
2. **Alpha Kontrolü** - `color.a` değeri ile karartma
3. **Ters Mantık** - 0 = aydınlık, 1 = karanlık
4. **Raycast Target: false** - UI etkileşimini engellemez

### **JSON Kayıt Sistemi:**
- **Dosya Konumu:** `Application.persistentDataPath/settings.json`
- **Otomatik Kayıt:** Her ayar değişikliğinde
- **Otomatik Yükleme:** Oyun başlangıcında
- **Hata Yönetimi:** Dosya yoksa varsayılan değerler

### **Fade Sistemi:**
- **AnimationCurve** - Özelleştirilebilir fade eğrisi
- **Duration** - Ayarlanabilir fade süresi
- **Non-blocking** - Coroutine ile asenkron çalışma

## 🎨 UI Tasarım Önerileri

### **Brightness Panel:**
```
┌─────────────────────────┐
│      BRIGHTNESS         │
│                         │
│    [██████████] 80%    │
│                         │
│        [BACK]           │
└─────────────────────────┘
```

### **Slider Ayarları:**
- **Min Value:** 0
- **Max Value:** 1
- **Whole Numbers:** false
- **Interactable:** true

### **Text Ayarları:**
- **Font:** TextMeshPro
- **Alignment:** Center
- **Color:** White
- **Size:** 16-18

## 🐛 Sorun Giderme

### **Yaygın Sorunlar:**

1. **Brightness Overlay Görünmüyor:**
   - Canvas render mode kontrol edin
   - Sorting order kontrol edin
   - Alpha değeri kontrol edin

2. **JSON Dosyası Oluşmuyor:**
   - Application.persistentDataPath kontrol edin
   - Write permissions kontrol edin
   - Debug.Log ile dosya yolunu kontrol edin

3. **Ayarlar Kaydedilmiyor:**
   - SettingsManager Instance kontrol edin
   - UI elementlerinin atandığını kontrol edin
   - Console'da hata mesajlarını kontrol edin

4. **Fade Efekti Çalışmıyor:**
   - Brightness overlay atandığını kontrol edin
   - Coroutine'in çalıştığını kontrol edin
   - AnimationCurve ayarlarını kontrol edin

### **Debug İpuçları:**
```csharp
// JSON dosya yolunu yazdır
Debug.Log($"Settings path: {settingsFilePath}");

// Brightness değerini kontrol et
Debug.Log($"Current brightness: {GetBrightness()}");

// Overlay alpha değerini kontrol et
Debug.Log($"Overlay alpha: {brightnessOverlay.color.a}");
```

## 🔄 Entegrasyon

### **Mevcut Sistemlerle:**
- **AudioManager** - Otomatik entegrasyon
- **UI_Manager** - Koordinasyon
- **SaveManager** - Bağımsız çalışma

### **Yeni Özellikler:**
- **Scene Transition** - Fade efektleri
- **Settings Persistence** - JSON tabanlı
- **UI Overlay** - Işık bağımsız brightness

Bu sistem tamamen 2D URP projeleri için optimize edilmiştir ve Hollow Knight tarzı menü sistemleri için mükemmel uyum sağlar. 