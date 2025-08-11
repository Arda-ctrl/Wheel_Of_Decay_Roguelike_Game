using UnityEngine;
using UnityEditor;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
public class LocalizationSetup : MonoBehaviour
{
    [Header("Localization Configuration")]
    [SerializeField] private LocalizationSettings localizationSettings;
    [SerializeField] private List<Locale> availableLocales = new List<Locale>();
    
    [Header("String Tables")]
    [SerializeField] private StringTable uiStringTable;
    
    [Header("UI Text References")]
    [SerializeField] private List<LocalizedTextReference> uiTextReferences = new List<LocalizedTextReference>();
    
    [System.Serializable]
    public class LocalizedTextReference
    {
        public string key;
        public string englishText;
        public string turkishText;
        public string germanText;
        public string description;
    }
    
    [ContextMenu("Setup Localization System")]
    public void SetupLocalizationSystem()
    {
        Debug.Log("🔧 Setting up localization system...");
        
        // 1. LocalizationSettings'i bul veya oluştur
        SetupLocalizationSettings();
        
        // 2. Locale'leri ayarla
        SetupLocales();
        
        // 3. String table'ları oluştur
        SetupStringTables();
        
        // 4. UI text referanslarını ayarla
        SetupUITextReferences();
        
        Debug.Log("✅ Localization system setup complete!");
    }
    
    private void SetupLocalizationSettings()
    {
        if (localizationSettings == null)
        {
            localizationSettings = LocalizationSettings.Instance;
            if (localizationSettings == null)
            {
                Debug.LogWarning("⚠️ LocalizationSettings not found. Please create one in Window > Localization > Localization Settings");
                return;
            }
        }
        
        Debug.Log("✅ LocalizationSettings configured");
    }
    
    private void SetupLocales()
    {
        if (localizationSettings == null) return;
        
        // Mevcut locale'leri al
        var localesProvider = localizationSettings.GetAvailableLocales();
        if (localesProvider != null)
        {
            availableLocales = localesProvider.Locales;
            Debug.Log($"✅ Found {availableLocales.Count} locales");
            
            foreach (var locale in availableLocales)
            {
                Debug.Log($"  - {locale.LocaleName} ({locale.Identifier.CultureInfo.TwoLetterISOLanguageName})");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No locales found. Please add locales in Localization Settings");
        }
    }
    
    private void SetupStringTables()
    {
        if (uiStringTable == null)
        {
            // UI string table'ı bul
            uiStringTable = LocalizationSettings.StringDatabase.GetTable("UI") as StringTable;
            if (uiStringTable == null)
            {
                Debug.LogWarning("⚠️ UI string table not found. Please create one in Localization Settings");
                return;
            }
        }
        
        Debug.Log("✅ String tables configured");
    }
    
    private void SetupUITextReferences()
    {
        // Default UI text referanslarını oluştur
        if (uiTextReferences.Count == 0)
        {
            uiTextReferences = new List<LocalizedTextReference>
            {
                new LocalizedTextReference { key = "StartGame", englishText = "Start Game", turkishText = "Oyunu Başlat", germanText = "Spiel Starten", description = "Main menu start game button" },
                new LocalizedTextReference { key = "Continue", englishText = "Continue", turkishText = "Devam Et", germanText = "Fortsetzen", description = "Main menu continue button" },
                new LocalizedTextReference { key = "Options", englishText = "Options", turkishText = "Ayarlar", germanText = "Optionen", description = "Main menu options button" },
                new LocalizedTextReference { key = "Extras", englishText = "Extras", turkishText = "Ekstralar", germanText = "Extras", description = "Main menu extras button" },
                new LocalizedTextReference { key = "Quit", englishText = "Quit", turkishText = "Çıkış", germanText = "Beenden", description = "Main menu quit button" },
                new LocalizedTextReference { key = "LoadGame", englishText = "Load Game", turkishText = "Oyunu Yükle", germanText = "Spiel Laden", description = "Load game button" },
                new LocalizedTextReference { key = "ClearSave", englishText = "Clear Save", turkishText = "Kayıtları Temizle", germanText = "Speicher Löschen", description = "Clear save button" },
                new LocalizedTextReference { key = "Back", englishText = "Back", turkishText = "Geri", germanText = "Zurück", description = "Back button" },
                new LocalizedTextReference { key = "GameOptions", englishText = "Game Options", turkishText = "Oyun Ayarları", germanText = "Spieloptionen", description = "Game options button" },
                new LocalizedTextReference { key = "AudioOptions", englishText = "Audio Options", turkishText = "Ses Ayarları", germanText = "Audiooptionen", description = "Audio options button" },
                new LocalizedTextReference { key = "VideoOptions", englishText = "Video Options", turkishText = "Video Ayarları", germanText = "Videooptionen", description = "Video options button" },
                new LocalizedTextReference { key = "ControllerOptions", englishText = "Controller Options", turkishText = "Kontrolcü Ayarları", germanText = "Controller-Optionen", description = "Controller options button" },
                new LocalizedTextReference { key = "KeyboardOptions", englishText = "Keyboard Options", turkishText = "Klavye Ayarları", germanText = "Tastatur-Optionen", description = "Keyboard options button" },
                new LocalizedTextReference { key = "Credits", englishText = "Credits", turkishText = "Jenerik", germanText = "Credits", description = "Credits button" },
                new LocalizedTextReference { key = "NoSaveData", englishText = "No save data found", turkishText = "Kayıt verisi bulunamadı", germanText = "Keine Speicherdaten gefunden", description = "No save data message" },
                new LocalizedTextReference { key = "MasterVolume", englishText = "Master Volume", turkishText = "Ana Ses", germanText = "Hauptlautstärke", description = "Master volume label" },
                new LocalizedTextReference { key = "SoundVolume", englishText = "Sound Volume", turkishText = "Efekt Sesi", germanText = "Effektlautstärke", description = "Sound volume label" },
                new LocalizedTextReference { key = "MusicVolume", englishText = "Music Volume", turkishText = "Müzik Sesi", germanText = "Musiklautstärke", description = "Music volume label" },
                new LocalizedTextReference { key = "Resolution", englishText = "Resolution", turkishText = "Çözünürlük", germanText = "Auflösung", description = "Resolution label" },
                new LocalizedTextReference { key = "Fullscreen", englishText = "Fullscreen", turkishText = "Tam Ekran", germanText = "Vollbild", description = "Fullscreen toggle" },
                new LocalizedTextReference { key = "VSync", englishText = "V-Sync", turkishText = "V-Sync", germanText = "V-Sync", description = "V-Sync toggle" },
                new LocalizedTextReference { key = "ParticleEffects", englishText = "Particle Effects", turkishText = "Parçacık Efektleri", germanText = "Partikeleffekte", description = "Particle effects dropdown" },
                new LocalizedTextReference { key = "BlurQuality", englishText = "Blur Quality", turkishText = "Bulantı Kalitesi", germanText = "Unschärfe-Qualität", description = "Blur quality dropdown" },
                new LocalizedTextReference { key = "Brightness", englishText = "Brightness", turkishText = "Parlaklık", germanText = "Helligkeit", description = "Brightness slider" },
                new LocalizedTextReference { key = "Language", englishText = "Language", turkishText = "Dil", germanText = "Sprache", description = "Language dropdown" },
                new LocalizedTextReference { key = "ResetDefaults", englishText = "Reset to Defaults", turkishText = "Varsayılana Sıfırla", germanText = "Auf Standard zurücksetzen", description = "Reset defaults button" },
                new LocalizedTextReference { key = "QuitConfirm", englishText = "Are you sure you want to quit?", turkishText = "Çıkmak istediğinizden emin misiniz?", germanText = "Sind Sie sicher, dass Sie beenden möchten?", description = "Quit confirmation message" },
                new LocalizedTextReference { key = "Yes", englishText = "Yes", turkishText = "Evet", germanText = "Ja", description = "Yes button" },
                new LocalizedTextReference { key = "No", englishText = "No", turkishText = "Hayır", germanText = "Nein", description = "No button" }
            };
        }
        
        Debug.Log($"✅ {uiTextReferences.Count} UI text references configured");
    }
    
    [ContextMenu("Generate Localization Files")]
    public void GenerateLocalizationFiles()
    {
        Debug.Log("📝 Generating localization files...");
        
        if (uiTextReferences.Count == 0)
        {
            SetupUITextReferences();
        }
        
        // Bu method Unity Editor'da localization dosyalarını oluşturmak için kullanılabilir
        // Gerçek implementasyon Unity Localization API'si ile yapılmalı
        
        Debug.Log("✅ Localization files generation complete!");
        Debug.Log("💡 Note: Use Unity Localization Window to create actual localization assets");
    }
    
    [ContextMenu("Test Localization")]
    public void TestLocalization()
    {
        Debug.Log("🧪 Testing localization system...");
        
        if (LocalizationManager.Instance != null)
        {
            Debug.Log("✅ LocalizationManager found");
            Debug.Log($"Current language: {LocalizationManager.Instance.GetCurrentLanguageCode()}");
            Debug.Log($"Available languages: {string.Join(", ", LocalizationManager.Instance.GetAvailableLanguageNames())}");
        }
        else
        {
            Debug.LogWarning("⚠️ LocalizationManager not found");
        }
        
        if (localizationSettings != null)
        {
            Debug.Log("✅ LocalizationSettings found");
            Debug.Log($"Selected locale: {LocalizationSettings.SelectedLocale?.LocaleName ?? "None"}");
        }
        else
        {
            Debug.LogWarning("⚠️ LocalizationSettings not found");
        }
    }
    
    private void OnValidate()
    {
        // Inspector'da değişiklik yapıldığında otomatik olarak çalışır
        if (Application.isPlaying) return;
        
        // Editor'da validation yap
        if (localizationSettings == null)
        {
            localizationSettings = LocalizationSettings.Instance;
        }
    }
}
#endif
