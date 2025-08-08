# Audio Manager Detaylı Kurulum Rehberi

## 🎵 Audio Manager Nedir?

### **Görev:**
- ✅ **Müzik** çalma/yönetimi
- ✅ **SFX** çalma/yönetimi
- ✅ **Volume** kontrolü
- ✅ **Audio Mixer** entegrasyonu
- ✅ **Kalıcı** ses ayarları

## 🔧 Adım Adım Kurulum

### **1. Audio Mixer Oluşturma:**

#### **Audio Mixer Oluştur:**
1. **Project Window'da** sağ tık
2. **Create** → **Audio** → **Audio Mixer**
3. **İsim:** "GameAudioMixer"

#### **Audio Mixer Groups Oluştur:**
```
GameAudioMixer:
├── Master Group (varsayılan)
│   ├── Music Group
│   └── SFX Group
```

**Nasıl Oluşturulur:**
1. **GameAudioMixer** seç
2. **Groups** bölümünde **+** tıkla
3. **Music Group** oluştur
4. **SFX Group** oluştur

### **2. AudioManager GameObject Oluşturma:**

#### **Ana GameObject:**
1. **Hierarchy'de** sağ tık → **Create Empty**
2. **İsim:** "AudioManager"
3. **AudioManager** component'ini ekle

#### **Audio Sources Oluşturma:**
1. **AudioManager** altında **Create Empty**
2. **İsim:** "MusicSource"
3. **AudioSource** component'i ekle
4. **AudioManager** altında **Create Empty**
5. **İsim:** "SFXSource"
6. **AudioSource** component'i ekle

### **3. AudioManager Script Atamaları:**

#### **AudioManager Component'inde:**
```
[Header("Audio Sources")]
├── Music Audio Source → MusicSource GameObject
└── SFX Audio Source → SFXSource GameObject

[Header("Audio Mixer")]
└── Master Audio Mixer → GameAudioMixer

[Header("Audio Clips")]
├── SFX Clips → Ses efektleri array'i
├── Level Music → Müzik dosyası
├── Game Over Music → Müzik dosyası
└── Win Music → Müzik dosyası
```

### **4. Audio Sources Ayarları:**

#### **Music Audio Source:**
```
AudioSource Settings:
├── Output → Music Group
├── Play On Awake → ✓
├── Loop → ✓
└── Volume → 1
```

#### **SFX Audio Source:**
```
AudioSource Settings:
├── Output → SFX Group
├── Play On Awake → ✗
├── Loop → ✗
└── Volume → 1
```

### **5. Audio Mixer Ayarları:**

#### **Master Group:**
```
Parameters:
├── Master Volume → "MasterVolume"
└── Mute → "MasterMute"
```

#### **Music Group:**
```
Parameters:
├── Music Volume → "MusicVolume"
└── Mute → "MusicMute"
```

#### **SFX Group:**
```
Parameters:
├── SFX Volume → "SFXVolume"
└── Mute → "SFXMute"
```

## 🎮 Unity Editor'da Atama Adımları

### **1. AudioManager GameObject:**
```
Hierarchy:
└── AudioManager
    ├── AudioManager Component
    ├── MusicSource (AudioSource)
    └── SFXSource (AudioSource)
```

### **2. AudioManager Component Atamaları:**
1. **AudioManager** GameObject'ini seç
2. **Inspector'da** AudioManager component'ini bul
3. **Her field'a** ilgili GameObject'i sürükle:

```
AudioManager Component:
├── Music Audio Source → MusicSource GameObject
├── SFX Audio Source → SFXSource GameObject
├── Master Audio Mixer → GameAudioMixer asset
├── SFX Clips → Ses efektleri (array)
├── Level Music → Müzik dosyası
├── Game Over Music → Müzik dosyası
└── Win Music → Müzik dosyası
```

### **3. Audio Sources Atamaları:**
1. **MusicSource** GameObject'ini seç
2. **AudioSource** component'inde:
   - **Output** → **Music Group** seç
   - **Play On Awake** → ✓ işaretle
   - **Loop** → ✓ işaretle

3. **SFXSource** GameObject'ini seç
4. **AudioSource** component'inde:
   - **Output** → **SFX Group** seç
   - **Play On Awake** → ✗ işareti kaldır
   - **Loop** → ✗ işareti kaldır

## 🔧 Ses Ayarları Sorunu Çözümü

### **Sorun: Ses Ayarları Kaydedilmiyor**

#### **Çözüm 1: AudioManager Atamaları**
```
AudioManager GameObject:
├── AudioManager Component
│   ├── Master Audio Mixer → GameAudioMixer ✓
│   ├── Music Audio Source → MusicSource ✓
│   ├── SFX Audio Source → SFXSource ✓
│   ├── SFX Clips → Ses efektleri ✓
│   ├── Level Music → Müzik dosyası ✓
│   ├── Game Over Music → Müzik dosyası ✓
│   └── Win Music → Müzik dosyası ✓
```

#### **Çözüm 2: SettingsManager Atamaları**
```
SettingsManager GameObject:
├── SettingsManager Component
│   ├── Master Volume Slider → UI Slider ✓
│   ├── Sound Volume Slider → UI Slider ✓
│   ├── Music Volume Slider → UI Slider ✓
│   └── Brightness Overlay → Canvas Image ✓
```

#### **Çözüm 3: MainMenuManager Atamaları**
```
MainMenuManager GameObject:
├── MainMenuManager Component
│   ├── UI References
│   │   ├── Audio Manager → AudioManager GameObject ✓
│   │   └── Settings Manager → SettingsManager GameObject ✓
│   └── Audio Options
│       ├── Master Volume Slider → UI Slider ✓
│       ├── Sound Volume Slider → UI Slider ✓
│       └── Music Volume Slider → UI Slider ✓
```

## 🐛 Sorun Giderme

### **Ses Çalmıyor:**
1. **AudioSource'lar** atanmış mı?
2. **AudioClip'ler** atanmış mı?
3. **Audio Mixer** bağlantısı var mı?
4. **Volume** 0'dan büyük mü?

### **Ses Ayarları Kaydedilmiyor:**
1. **SettingsManager** atanmış mı?
2. **UI Slider'lar** doğru metod'u çağırıyor mu?
3. **JSON dosyası** oluşturuluyor mu?
4. **AudioManager** bağlantısı var mı?

### **Null Reference Exception:**
1. **Tüm referanslar** atanmış mı?
2. **GameObject'ler** aktif mi?
3. **Component'ler** doğru mu?

## ✅ Test Senaryoları

### **Test 1: Ses Çalma**
```
1. AudioManager'da PlaySFX(0) çağır
2. Ses çalıyor mu? ✅
3. AudioManager'da PlayMusic() çağır
4. Müzik çalıyor mu? ✅
```

### **Test 2: Volume Kontrolü**
```
1. Audio Options'a git
2. Master Volume'u değiştir
3. Ses seviyesi değişiyor mu? ✅
4. Music Volume'u değiştir
5. Müzik seviyesi değişiyor mu? ✅
```

### **Test 3: Kalıcı Ayarlar**
```
1. Ses ayarlarını değiştir
2. Oyunu kapat/aç
3. Ayarlar korunuyor mu? ✅
```

## 🎯 Özet

### **Gerekli Atamalar:**
- ✅ **AudioManager** → Tüm audio referansları
- ✅ **SettingsManager** → UI elementleri
- ✅ **MainMenuManager** → Manager referansları
- ✅ **Audio Mixer** → Volume parametreleri

### **Kontrol Listesi:**
- ✅ Audio Mixer oluşturuldu
- ✅ Audio Sources atandı
- ✅ Audio Clips yüklendi
- ✅ UI Slider'lar bağlandı
- ✅ Manager referansları atandı

Artık ses sistemi tam olarak çalışacak! 🎵✨ 