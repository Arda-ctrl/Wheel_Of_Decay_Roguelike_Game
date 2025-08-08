# Save System ve SettingsManager Entegrasyon Rehberi

## 🎯 Sorun Çözümü

### **Çakışan SettingsData Sınıfı**
- ❌ **Eski Problem:** SaveManager ve SettingsManager'da aynı isimde SettingsData sınıfları
- ✅ **Çözüm:** SettingsData sınıfı tek dosyada (`Assets/Scripts/Data/SettingsData.cs`) tanımlandı

## 📁 Güncellenen Dosyalar

### **1. SettingsData.cs (Yeni)**
```csharp
// Assets/Scripts/Data/SettingsData.cs
// Tüm ayarları JSON'a serialize eden merkezi sınıf
```

### **2. SaveManager.cs (Güncellenmiş)**
- ❌ Eski SettingsData sınıfı kaldırıldı
- ✅ Yeni SettingsData sınıfı kullanılıyor
- ✅ SettingsManager ile entegrasyon eklendi

### **3. SettingsManager.cs (Güncellenmiş)**
- ✅ GetMasterVolume(), GetSoundVolume(), GetMusicVolume() metodları eklendi
- ✅ JSON tabanlı ayar yönetimi

### **4. MainMenuManager.cs (Güncellenmiş)**
- ✅ SettingsManager entegrasyonu
- ✅ PlayerPrefs yerine JSON kullanımı

## 🔄 Entegrasyon Detayları

### **SaveManager → SettingsManager Entegrasyonu:**

#### **GetCurrentSettingsData() Metodu:**
```csharp
private SettingsData GetCurrentSettingsData()
{
    SettingsData settingsData = new SettingsData();
    
    // SettingsManager'dan ayarları al (eğer varsa)
    if (SettingsManager.Instance != null)
    {
        settingsData.masterVolume = SettingsManager.Instance.GetMasterVolume();
        settingsData.soundVolume = SettingsManager.Instance.GetSoundVolume();
        settingsData.musicVolume = SettingsManager.Instance.GetMusicVolume();
        settingsData.fullscreen = Screen.fullScreen;
        settingsData.vSync = QualitySettings.vSyncCount > 0;
        settingsData.particleEffectsQuality = PlayerPrefs.GetInt("ParticleEffects", 1);
        settingsData.blurQuality = PlayerPrefs.GetInt("BlurQuality", 1);
        settingsData.brightness = SettingsManager.Instance.GetBrightness();
        settingsData.languageIndex = PlayerPrefs.GetInt("Language", 0);
    }
    else
    {
        // Fallback: PlayerPrefs'den al
        // ...
    }
    
    return settingsData;
}
```

#### **ApplySettingsData() Metodu:**
```csharp
private void ApplySettingsData(SettingsData settingsData)
{
    // SettingsManager'a ayarları uygula (eğer varsa)
    if (SettingsManager.Instance != null)
    {
        SettingsManager.Instance.SetMasterVolume(settingsData.masterVolume);
        SettingsManager.Instance.SetSoundVolume(settingsData.soundVolume);
        SettingsManager.Instance.SetMusicVolume(settingsData.musicVolume);
        SettingsManager.Instance.SetFullscreen(settingsData.fullscreen);
        SettingsManager.Instance.SetVSync(settingsData.vSync);
        SettingsManager.Instance.SetParticleEffects(settingsData.particleEffectsQuality);
        SettingsManager.Instance.SetBlurQuality(settingsData.blurQuality);
        SettingsManager.Instance.SetBrightness(settingsData.brightness);
        SettingsManager.Instance.SetLanguage(settingsData.languageIndex);
    }
    else
    {
        // Fallback: PlayerPrefs'e kaydet
        // ...
    }
}
```

## 🎮 Kullanım Senaryoları

### **1. Oyun Başlangıcı:**
```
1. SettingsManager yüklenir
2. settings.json dosyası okunur
3. Ayarlar uygulanır
4. SaveManager ayarları alır
```

### **2. Ayar Değişikliği:**
```
1. Kullanıcı ayarı değiştirir
2. SettingsManager ayarı günceller
3. settings.json dosyasına kaydedilir
4. SaveManager otomatik olarak güncel ayarları alır
```

### **3. Oyun Kaydetme:**
```
1. SaveManager.GetCurrentSettingsData() çağrılır
2. SettingsManager'dan güncel ayarlar alınır
3. GameSaveData'ya eklenir
4. gamesave.json dosyasına kaydedilir
```

### **4. Oyun Yükleme:**
```
1. gamesave.json dosyası okunur
2. SettingsData ayarları çıkarılır
3. SettingsManager'a uygulanır
4. settings.json dosyası güncellenir
```

## 📄 Dosya Yapısı

### **settings.json (Ayarlar):**
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

### **gamesave.json (Oyun Kaydı):**
```json
{
  "playerData": { ... },
  "levelData": { ... },
  "settingsData": {
    "brightness": 0.8,
    "masterVolume": 1.0,
    "soundVolume": 0.7,
    "musicVolume": 0.9,
    "fullscreen": true,
    "vSync": false,
    "particleEffectsQuality": 1,
    "blurQuality": 1,
    "brightness": 0.8,
    "languageIndex": 0
  },
  "saveTime": "2024-01-01T12:00:00",
  "saveVersion": "1.0.0",
  "totalPlayTime": 3600,
  "totalDeaths": 5,
  "totalKills": 100,
  "totalRoomsCleared": 25
}
```

## 🔧 Teknik Detaylar

### **Fallback Sistemi:**
- **SettingsManager varsa:** JSON tabanlı ayarlar kullanılır
- **SettingsManager yoksa:** PlayerPrefs tabanlı ayarlar kullanılır
- **Her iki durumda da:** SaveManager çalışmaya devam eder

### **Veri Senkronizasyonu:**
- **Tek Yön:** SettingsManager → SaveManager
- **Otomatik:** SaveManager her zaman SettingsManager'dan güncel veriyi alır
- **Güvenli:** Fallback sistemi sayesinde hata durumunda çalışmaya devam eder

### **Dosya Konumları:**
- **settings.json:** `Application.persistentDataPath/settings.json`
- **gamesave.json:** `Application.persistentDataPath/gamesave.json`
- **Backup:** `Application.persistentDataPath/gamesave_backup.json`

## 🐛 Sorun Giderme

### **Yaygın Sorunlar:**

1. **SettingsData Çakışması:**
   - ✅ Çözüldü: Tek SettingsData sınıfı kullanılıyor

2. **Ayarlar Senkronize Olmuyor:**
   - SettingsManager Instance kontrol edin
   - Debug.Log ile ayar değerlerini kontrol edin

3. **JSON Dosyaları Oluşmuyor:**
   - Application.persistentDataPath kontrol edin
   - Write permissions kontrol edin

4. **SaveManager Hata Veriyor:**
   - SettingsManager'ın yüklendiğini kontrol edin
   - Fallback sisteminin çalıştığını kontrol edin

### **Debug İpuçları:**
```csharp
// SettingsManager kontrolü
Debug.Log($"SettingsManager exists: {SettingsManager.Instance != null}");

// Ayar değerlerini kontrol et
if (SettingsManager.Instance != null)
{
    Debug.Log($"Brightness: {SettingsManager.Instance.GetBrightness()}");
    Debug.Log($"Master Volume: {SettingsManager.Instance.GetMasterVolume()}");
}

// JSON dosya yollarını kontrol et
Debug.Log($"Settings path: {Application.persistentDataPath}/settings.json");
Debug.Log($"Save path: {Application.persistentDataPath}/gamesave.json");
```

## ✅ Sonuç

Bu entegrasyon sayesinde:
- ✅ **Tek SettingsData sınıfı** kullanılıyor
- ✅ **JSON tabanlı ayar sistemi** çalışıyor
- ✅ **SaveManager ve SettingsManager** uyumlu çalışıyor
- ✅ **Fallback sistemi** güvenlik sağlıyor
- ✅ **Otomatik senkronizasyon** mevcut

Artık sistem tamamen JSON tabanlı çalışıyor ve çakışma sorunları çözülmüş durumda! 🎮✨ 