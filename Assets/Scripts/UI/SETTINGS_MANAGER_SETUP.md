# SettingsManager Kurulum Rehberi

## 🎯 SettingsManager Nedir?

### **Görev:**
- ✅ **JSON tabanlı** ayar yönetimi
- ✅ **Brightness overlay** kontrolü
- ✅ **Audio ayarları** yönetimi
- ✅ **Video ayarları** yönetimi
- ✅ **Kalıcı** ayar saklama

## 🔧 Unity Editor'da Kurulum

### **1. SettingsManager GameObject Oluşturma:**
1. **Hierarchy'de** sağ tık → **Create Empty**
2. **İsim:** "SettingsManager"
3. **SettingsManager** component'ini ekle

### **2. Brightness Overlay Kurulumu:**

#### **Otomatik Oluşturma (Önerilen):**
- ✅ **SettingsManager** otomatik olarak brightness overlay oluşturur
- ✅ **Canvas** yoksa otomatik oluşturur
- ✅ **Screen Space - Overlay** olarak ayarlar
- ✅ **Sorting Order: 1000** (en üstte)

#### **Manuel Oluşturma:**
1. **Canvas** oluştur (Screen Space - Overlay)
2. **Image** ekle (tam ekran)
3. **Color:** Siyah (0, 0, 0, 0) - şeffaf
4. **SettingsManager'a** sürükle

### **3. Audio Manager Bağlantısı:**

#### **AudioManager GameObject:**
1. **Hierarchy'de** sağ tık → **Create Empty**
2. **İsim:** "AudioManager"
3. **AudioManager** component'ini ekle
4. **AudioSource** ekle (Music için)
5. **AudioSource** ekle (SFX için)

#### **AudioManager Atamaları:**
```
AudioManager Component:
├── Master Audio Mixer → Audio Mixer sürükle
├── Music Audio Source → Music AudioSource sürükle
├── SFX Audio Source → SFX AudioSource sürükle
├── Music Audio Clips → Müzik dosyaları sürükle
└── SFX Audio Clips → Ses efektleri sürükle
```

### **4. SettingsManager Atamaları:**

#### **Brightness Control:**
```
SettingsManager Component:
├── Brightness Overlay → Canvas'taki Image sürükle
├── Brightness Slider → UI Slider sürükle
└── Brightness Value Text → UI Text sürükle
```

#### **Audio Controls:**
```
SettingsManager Component:
├── Master Volume Slider → UI Slider sürükle
├── Sound Volume Slider → UI Slider sürükle
└── Music Volume Slider → UI Slider sürükle
```

#### **Video Controls:**
```
SettingsManager Component:
├── Resolution Dropdown → UI Dropdown sürükle
├── Fullscreen Toggle → UI Toggle sürükle
├── V-Sync Toggle → UI Toggle sürükle
├── Particle Effects Dropdown → UI Dropdown sürükle
└── Blur Quality Dropdown → UI Dropdown sürükle
```

## 🎵 Audio Manager Detaylı Kurulum

### **1. Audio Mixer Oluşturma:**
1. **Project Window'da** sağ tık
2. **Create** → **Audio** → **Audio Mixer**
3. **İsim:** "GameAudioMixer"

### **2. Audio Mixer Groups:**
```
GameAudioMixer:
├── Master Group
│   ├── Music Group
│   └── SFX Group
```

### **3. AudioManager Script Atamaları:**
```csharp
[Header("Audio Sources")]
[SerializeField] private AudioSource musicSource;
[SerializeField] private AudioSource sfxSource;

[Header("Audio Mixer")]
[SerializeField] private AudioMixer audioMixer;

[Header("Audio Clips")]
[SerializeField] private AudioClip[] sfxClips;
[SerializeField] private AudioClip levelMusic;
[SerializeField] private AudioClip gameOverMusic;
[SerializeField] private AudioClip winMusic;
```

### **4. AudioManager Unity Editor Atamaları:**
1. **Music Audio Source** → Music için AudioSource
2. **SFX Audio Source** → SFX için AudioSource
3. **Audio Mixer** → GameAudioMixer
4. **SFX Clips** → Ses efektleri array'i
5. **Music Clips** → Müzik dosyaları

## 🔧 Ses Ayarları Sorunu Çözümü

### **Sorun:**
- ❌ **Ses ayarları** kaydedilmiyor
- ❌ **Oyun açılıp kapanınca** ayarlar sıfırlanıyor

### **Çözüm:**

#### **1. AudioManager'da Atamalar:**
```
AudioManager GameObject:
├── AudioManager Component
│   ├── Master Audio Mixer → GameAudioMixer
│   ├── Music Audio Source → Music AudioSource
│   ├── SFX Audio Source → SFX AudioSource
│   ├── SFX Clips → Ses efektleri
│   ├── Level Music → Müzik dosyası
│   ├── Game Over Music → Müzik dosyası
│   └── Win Music → Müzik dosyası
```

#### **2. SettingsManager'da Atamalar:**
```
SettingsManager GameObject:
├── SettingsManager Component
│   ├── Master Volume Slider → UI Slider
│   ├── Sound Volume Slider → UI Slider
│   ├── Music Volume Slider → UI Slider
│   └── Brightness Overlay → Canvas Image
```

#### **3. MainMenuManager'da Atamalar:**
```
MainMenuManager GameObject:
├── MainMenuManager Component
│   ├── UI References
│   │   ├── Audio Manager → AudioManager GameObject
│   │   └── Settings Manager → SettingsManager GameObject
│   └── Audio Options
│       ├── Master Volume Slider → UI Slider
│       ├── Sound Volume Slider → UI Slider
│       └── Music Volume Slider → UI Slider
```

## 🐛 Sorun Giderme

### **Ses Ayarları Kaydedilmiyor:**
1. **AudioManager** atanmış mı?
2. **SettingsManager** atanmış mı?
3. **UI Slider'lar** doğru metod'u çağırıyor mu?
4. **JSON dosyası** oluşturuluyor mu?

### **Brightness Overlay Çalışmıyor:**
1. **Canvas** Screen Space - Overlay mi?
2. **Image** tam ekran mı?
3. **SettingsManager'a** atanmış mı?
4. **Sorting Order** yeterli mi?

### **Null Reference Exception:**
1. **Tüm referanslar** atanmış mı?
2. **GameObject'ler** aktif mi?
3. **Component'ler** doğru mu?

## ✅ Test Senaryoları

### **Test 1: Ses Ayarları**
```
1. Audio Options'a git
2. Master Volume'u değiştir
3. Oyunu kapat/aç
4. Ses ayarı korunuyor mu? ✅
```

### **Test 2: Brightness Ayarları**
```
1. Video Options → Brightness
2. Brightness'ı değiştir
3. Oyunu kapat/aç
4. Brightness korunuyor mu? ✅
```

### **Test 3: JSON Dosyası**
```
1. Ayar değiştir
2. %APPDATA%/../LocalLow/[CompanyName]/[ProductName]/settings.json
3. Dosya oluştu mu? ✅
4. İçerik doğru mu? ✅
```

Artık ayarlar kalıcı olarak kaydedilecek! 🎮✨ 