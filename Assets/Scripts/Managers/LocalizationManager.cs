using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class LocalizationManager : MonoBehaviour
{
    [Header("Localization Settings")]
    [SerializeField] private LocalizationSettings localizationSettings;
    
    [Header("UI Text References")]
    [SerializeField] private List<LocalizedText> localizedTexts = new List<LocalizedText>();
    
    [Header("Language Options")]
    [SerializeField] private List<Locale> availableLocales = new List<Locale>();
    
    public static LocalizationManager Instance { get; private set; }
    
    private Locale currentLocale;
    private StringTable currentStringTable;
    
    [System.Serializable]
    public class LocalizedText
    {
        public string key;
        public TextMeshProUGUI textComponent;
        public Button buttonComponent;
        public TMP_InputField inputFieldComponent;
        public string fallbackText;
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLocalization();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        StartCoroutine(InitializeLocalizationAsync());
    }
    
    private void InitializeLocalization()
    {
        if (localizationSettings == null)
        {
            localizationSettings = LocalizationSettings.Instance;
        }
        
        // Available locales'ları al
        if (localizationSettings != null)
        {
            availableLocales = localizationSettings.GetAvailableLocales().Locales;
            Debug.Log($"🌍 Available locales bulundu: {availableLocales.Count}");
            for (int i = 0; i < availableLocales.Count; i++)
            {
                Debug.Log($"   Locale {i}: {availableLocales[i].LocaleName} ({availableLocales[i].Identifier})");
            }
        }
        else
        {
            Debug.LogError("❌ LocalizationSettings bulunamadı!");
        }
    }
    
    private IEnumerator InitializeLocalizationAsync()
    {
        // Localization sisteminin hazır olmasını bekle
        yield return LocalizationSettings.InitializationOperation;
        
        // String Table'ı otomatik ata
        if (currentStringTable == null)
        {
            try
            {
                // String Table'ı doğrudan mevcut locale ile al
                try
                {
                    var localizedString = LocalizationSettings.StringDatabase.GetLocalizedString("UI", "StartGame");
                    if (!string.IsNullOrEmpty(localizedString))
                    {
                        Debug.Log($"✅ String Table erişimi başarılı: {localizedString}");
                        // String Table erişimi çalışıyor, currentStringTable'ı null bırak
                        // GetLocalizedString zaten doğru String Table'ı kullanıyor
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ String Table erişimi başarısız, fallback kullanılacak");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ String Table erişim hatası: {e.Message}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ String Table atama hatası: {e.Message}");
            }
        }
        
        // Default locale'i ayarla
        int savedLanguage = PlayerPrefs.GetInt("Language", 0);
        SetLanguage(savedLanguage);
        
        // Tüm localized text'leri güncelle
        UpdateAllLocalizedTexts();
    }
    
    public void SetLanguage(int languageIndex)
    {
        if (localizationSettings == null || availableLocales.Count == 0) return;
        
        languageIndex = Mathf.Clamp(languageIndex, 0, availableLocales.Count - 1);
        
        // Locale'i değiştir
        currentLocale = availableLocales[languageIndex];
        LocalizationSettings.SelectedLocale = currentLocale;
        
        // String Table referansını temizle (yeni locale için yeniden alınacak)
        currentStringTable = null;
        
        // PlayerPrefs'e kaydet
        PlayerPrefs.SetInt("Language", languageIndex);
        PlayerPrefs.Save();
        
        // Tüm localized text'leri güncelle
        UpdateAllLocalizedTexts();
        
        Debug.Log($"Language changed to: {currentLocale.LocaleName} (Index: {languageIndex})");
    }
    
    public int GetCurrentLanguageIndex()
    {
        if (currentLocale == null) return 0;
        return availableLocales.IndexOf(currentLocale);
    }
    
    public string GetCurrentLanguageCode()
    {
        if (currentLocale == null) return "en";
        return currentLocale.Identifier.CultureInfo.TwoLetterISOLanguageName;
    }
    
    public List<string> GetAvailableLanguageNames()
    {
        List<string> languageNames = new List<string>();
        Debug.Log($"🔍 GetAvailableLanguageNames çağrıldı. Available locales count: {availableLocales.Count}");
        
        foreach (var locale in availableLocales)
        {
            languageNames.Add(locale.LocaleName);
            Debug.Log($"   Adding locale: {locale.LocaleName}");
        }
        
        Debug.Log($"🎯 Final language names: {string.Join(", ", languageNames)}");
        return languageNames;
    }
    
    public void AddLocalizedText(string key, TextMeshProUGUI textComponent, string fallbackText = "")
    {
        var localizedText = new LocalizedText
        {
            key = key,
            textComponent = textComponent,
            fallbackText = fallbackText
        };
        
        localizedTexts.Add(localizedText);
        
        // Hemen güncelle
        UpdateLocalizedText(localizedText);
    }
    
    public void AddLocalizedButton(string key, Button buttonComponent, string fallbackText = "")
    {
        var localizedText = new LocalizedText
        {
            key = key,
            buttonComponent = buttonComponent,
            fallbackText = fallbackText
        };
        
        localizedTexts.Add(localizedText);
        
        // Hemen güncelle
        UpdateLocalizedText(localizedText);
    }
    
    public void UpdateAllLocalizedTexts()
    {
        foreach (var localizedText in localizedTexts)
        {
            UpdateLocalizedText(localizedText);
        }
    }
    
    private void UpdateLocalizedText(LocalizedText localizedText)
    {
        if (string.IsNullOrEmpty(localizedText.key)) return;
        
        string localizedString = GetLocalizedString(localizedText.key, localizedText.fallbackText);
        
        // Text component varsa güncelle
        if (localizedText.textComponent != null)
        {
            localizedText.textComponent.text = localizedString;
        }
        
        // Button component varsa, altındaki TextMeshPro'yu bul ve güncelle
        if (localizedText.buttonComponent != null)
        {
            var buttonText = localizedText.buttonComponent.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = localizedString;
                
                // Eğer textComponent atanmamışsa otomatik ata
                if (localizedText.textComponent == null)
                {
                    localizedText.textComponent = buttonText;
                    Debug.Log($"🔄 {localizedText.key} için Text component otomatik atandı: {buttonText.name}");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ {localizedText.key} button'ında TextMeshPro component bulunamadı!");
            }
        }
        
        // InputField component varsa güncelle
        if (localizedText.inputFieldComponent != null)
        {
            localizedText.inputFieldComponent.text = localizedString;
            
            // Placeholder text'i de güncelle
            var placeholderText = localizedText.inputFieldComponent.placeholder as TextMeshProUGUI;
            if (placeholderText != null)
            {
                placeholderText.text = localizedString;
            }
        }
    }
    
    public string GetLocalizedString(string key, string fallbackText = "")
    {
        if (string.IsNullOrEmpty(key)) return fallbackText;
        
        try
        {
            // Localization sisteminden string'i al
            var localizedString = LocalizationSettings.StringDatabase.GetLocalizedString("UI", key);
            
            if (!string.IsNullOrEmpty(localizedString))
            {
                return localizedString;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to get localized string for key '{key}': {e.Message}");
        }
        
        // Fallback text'i döndür
        return fallbackText;
    }
    
    public void RefreshLocalization()
    {
        UpdateAllLocalizedTexts();
    }
    
    // Editor'da test için
    [ContextMenu("Refresh Localization")]
    private void RefreshLocalizationEditor()
    {
        UpdateAllLocalizedTexts();
    }
    
    // Language dropdown için helper method
    public void OnLanguageDropdownChanged(int index)
    {
        SetLanguage(index);
    }
    
    // Otomatik Text Component atama sistemi
    [ContextMenu("Auto Assign Text Components")]
    public void AutoAssignTextComponents()
    {
        Debug.Log("🔧 Otomatik Text Component atama başlatılıyor...");
        
        int assignedCount = 0;
        
        foreach (var localizedText in localizedTexts)
        {
            if (localizedText.buttonComponent != null && localizedText.textComponent == null)
            {
                // Button'ın altındaki TextMeshPro component'ini bul
                var textComponent = localizedText.buttonComponent.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    localizedText.textComponent = textComponent;
                    assignedCount++;
                    Debug.Log($"✅ {localizedText.key} için Text component atandı: {textComponent.name}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ {localizedText.key} için Text component bulunamadı!");
                }
            }
            
            if (localizedText.inputFieldComponent != null)
            {
                // InputField'ın placeholder text'ini bul
                var placeholderText = localizedText.inputFieldComponent.placeholder as TextMeshProUGUI;
                if (placeholderText != null && localizedText.textComponent == null)
                {
                    localizedText.textComponent = placeholderText;
                    assignedCount++;
                    Debug.Log($"✅ {localizedText.key} için Placeholder Text component atandı");
                }
            }
        }
        
        Debug.Log($"🎯 Toplam {assignedCount} Text component otomatik atandı!");
        
        // Atamalardan sonra tüm metinleri güncelle
        UpdateAllLocalizedTexts();
    }
    
    // Button'lardaki Text component'leri otomatik bul ve ata
    [ContextMenu("Find Missing Text Components")]
    public void FindMissingTextComponents()
    {
        Debug.Log("🔍 Eksik Text component'ler aranıyor...");
        
        foreach (var localizedText in localizedTexts)
        {
            if (localizedText.buttonComponent != null)
            {
                var textComponent = localizedText.buttonComponent.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    Debug.Log($"📝 {localizedText.key}: {textComponent.name} bulundu");
                }
                else
                {
                    Debug.LogWarning($"❌ {localizedText.key}: Text component bulunamadı!");
                }
            }
        }
    }
    
    // Tüm button'larda Text component var mı kontrol et
    [ContextMenu("Validate Text Components")]
    public void ValidateTextComponents()
    {
        Debug.Log("✅ Text component validasyonu başlatılıyor...");
        
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (var localizedText in localizedTexts)
        {
            if (localizedText.buttonComponent != null)
            {
                var textComponent = localizedText.buttonComponent.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    validCount++;
                    Debug.Log($"✅ {localizedText.key}: Geçerli");
                }
                else
                {
                    invalidCount++;
                    Debug.LogError($"❌ {localizedText.key}: Text component eksik!");
                }
            }
        }
        
        Debug.Log($"📊 Sonuç: {validCount} geçerli, {invalidCount} eksik");
    }
}
