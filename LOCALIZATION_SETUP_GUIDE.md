# Localization Sistemi Kurulum Rehberi

Bu rehber, oyununuzda Unity Localization sistemi ile Türkçe ve İngilizce dil desteği kurmanızı sağlar.

## 🚀 Hızlı Başlangıç

### 1. Unity Localization Package Kurulumu
- Unity Package Manager'ı açın (Window > Package Manager)
- "Localization" paketini arayın ve kurun
- Unity 2022.3+ gereklidir

### 2. Localization Settings Oluşturma
- Window > Localization > Localization Settings
- "Create Localization Settings" butonuna tıklayın
- Bu, proje için gerekli localization ayarlarını oluşturacak

### 3. Locale'leri Ekleme
- Localization Settings'de "Available Locales" bölümüne tıklayın
- "Add Locale" ile şu dilleri ekleyin:
  - **English (en)** - Varsayılan dil
  - **Turkish (tr)** - Türkçe
  - **German (de)** - Almanca (opsiyonel)

### 4. String Table Oluşturma
- Window > Localization > String Table Collection
- "Create String Table Collection" ile yeni tablo oluşturun
- Table Name: "UI"
- Bu tablo, tüm UI metinlerini içerecek

### 5. Localization Assets Oluşturma
- String Table Collection'da "Add Locale Override" ile her dil için asset oluşturun
- Her dil için ayrı asset dosyası oluşturulacak

## 📁 Dosya Yapısı

```
Assets/
├── Languages/
│   ├── Localization Settings.asset
│   ├── UI_StringTable.asset (String Table Collection)
│   ├── UI_en.asset (İngilizce metinler)
│   ├── UI_tr.asset (Türkçe metinler)
│   └── UI_de.asset (Almanca metinler)
└── Scripts/
    ├── Managers/
    │   ├── LocalizationManager.cs
    │   └── LocalizationSetup.cs
    └── UI/
        └── MainMenuManager.cs
```

## 🔧 Script Kurulumu

### LocalizationManager
- Boş bir GameObject oluşturun
- "LocalizationManager" script'ini ekleyin
- Localization Settings referansını atayın
- Bu GameObject'i DontDestroyOnLoad yapın

### MainMenuManager Güncelleme
- MainMenuManager'da LocalizationManager referansını ekleyin
- InitializeLocalization() method'u otomatik olarak çalışacak

## 📝 Metin Ekleme

### 1. String Table'a Key Ekleme
- UI_StringTable.asset'i açın
- "Add Entry" ile yeni key ekleyin
- Key: "StartGame", Value: "Start Game"

### 2. Her Dil İçin Çeviri
- UI_en.asset: "Start Game"
- UI_tr.asset: "Oyunu Başlat"
- UI_de.asset: "Spiel Starten"

### 3. Script'te Kullanım
```csharp
// LocalizedText component ile
[SerializeField] private LocalizedString startGameText;

// Veya LocalizationManager ile
string text = LocalizationManager.Instance.GetLocalizedString("StartGame", "Start Game");
```

## 🎮 Oyun İçinde Kullanım

### Dil Değiştirme
```csharp
// Language dropdown'da
public void OnLanguageChanged(int index)
{
    LocalizationManager.Instance.SetLanguage(index);
}
```

### Dinamik Metin Güncelleme
```csharp
// Tüm localized text'leri güncelle
LocalizationManager.Instance.RefreshLocalization();
```

## 🧪 Test Etme

### LocalizationSetup Script'i
- LocalizationSetup script'ini bir GameObject'e ekleyin
- Context Menu'den "Test Localization" çalıştırın
- Console'da localization durumunu kontrol edin

### Runtime Test
- Oyunu çalıştırın
- Options > Game Options > Language dropdown'dan dil değiştirin
- UI metinlerinin değiştiğini kontrol edin

## 🔍 Sorun Giderme

### LocalizationManager Bulunamıyor
- LocalizationManager script'inin bir GameObject'te olduğundan emin olun
- Script'in compile edildiğini kontrol edin

### Metinler Değişmiyor
- String Table'da key'lerin doğru olduğunu kontrol edin
- Locale asset'lerinin doğru referans edildiğini kontrol edin
- Localization Settings'de locale'lerin aktif olduğunu kontrol edin

### Build Hatası
- Localization Settings'de "Preload Behavior" ayarını kontrol edin
- Addressables kullanıyorsanız, localization asset'lerini build'e dahil edin

## 📚 Önemli Notlar

1. **Key Naming**: Tutarlı key isimlendirme kullanın (örn: "StartGame", "LoadGame")
2. **Fallback Text**: Her localized string için fallback text sağlayın
3. **Performance**: Çok fazla localized text varsa, lazy loading kullanmayı düşünün
4. **Testing**: Her dil için UI testleri yapın

## 🎯 Sonraki Adımlar

- Daha fazla UI elementi için localization ekleyin
- Audio localization (ses dosyaları) ekleyin
- Font localization (Türkçe karakterler için) ekleyin
- Runtime locale switching implementasyonu geliştirin

## 📞 Destek

Herhangi bir sorun yaşarsanız:
1. Console log'larını kontrol edin
2. Localization Settings'i yeniden oluşturun
3. Script'leri yeniden compile edin
4. Unity Localization dokümantasyonunu inceleyin

---

**Not**: Bu sistem Unity 2022.3+ ve Localization package gerektirir. Eski Unity sürümlerinde çalışmayabilir.

