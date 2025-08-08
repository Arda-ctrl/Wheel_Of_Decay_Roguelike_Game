# Brightness Sistemi Açıklaması

## 🎯 Sistem Nasıl Çalışıyor?

### **Eski Sistem (Light-based):**
- ❌ **Light Component** kullanıyordu
- ❌ **Scene'deki ışıkları** kontrol ediyordu
- ❌ **World Space** Canvas gerektiriyordu

### **Yeni Sistem (UI Overlay-based):**
- ✅ **UI Image Overlay** kullanıyor
- ✅ **Screen Space - Overlay** Canvas kullanıyor
- ✅ **Tam ekran siyah Image** ile karartma yapıyor
- ✅ **Alpha değeri** ile brightness kontrolü

## 🔧 Teknik Detaylar

### **SettingsManager'da Brightness Kontrolü:**
```csharp
public void SetBrightness(float brightness)
{
    settingsData.brightness = Mathf.Clamp01(brightness);
    
    if (brightnessOverlay != null)
    {
        // Invert brightness: 0 = dark, 1 = bright
        float alpha = 1f - settingsData.brightness;
        Color color = brightnessOverlay.color;
        color.a = alpha;
        brightnessOverlay.color = color;
    }
    
    // Update UI text
    if (brightnessValueText != null)
    {
        int percentage = Mathf.RoundToInt(settingsData.brightness * 100f);
        brightnessValueText.text = $"{percentage}%";
    }
    
    SaveSettings();
}
```

### **Brightness Overlay Oluşturma:**
```csharp
private void CreateBrightnessOverlay()
{
    // Find or create Canvas
    Canvas canvas = FindFirstObjectByType<Canvas>();
    if (canvas == null)
    {
        GameObject canvasGO = new GameObject("Settings Canvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Ensure it's on top
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
    }
    
    // Create brightness overlay
    GameObject overlayGO = new GameObject("Brightness Overlay");
    overlayGO.transform.SetParent(canvas.transform, false);
    
    brightnessOverlay = overlayGO.AddComponent<Image>();
    brightnessOverlay.color = new Color(0, 0, 0, 0); // Transparent black
    brightnessOverlay.raycastTarget = false; // Don't block UI interactions
    
    // Set RectTransform to fill screen
    RectTransform rectTransform = brightnessOverlay.GetComponent<RectTransform>();
    rectTransform.anchorMin = Vector2.zero;
    rectTransform.anchorMax = Vector2.one;
    rectTransform.offsetMin = Vector2.zero;
    rectTransform.offsetMax = Vector2.zero;
}
```

## 🎨 UI Yapısı

### **Brightness Panel:**
```
┌─────────────────────────┐
│      BRIGHTNESS         │
│                         │
│    [██████████] 80%    │ ← Slider + Text
│                         │
│        [BACK]           │ ← Back Button
└─────────────────────────┘
```

### **MainMenuManager'da Gerekli Elementler:**
```csharp
[Header("Brightness Panel")]
[SerializeField] private Slider brightnessSlider;        // 0-1 arası slider
[SerializeField] private Button backToBrightnessButton;  // Geri butonu
[SerializeField] private TextMeshProUGUI brightnessValueText; // Yüzde gösterimi
```

## 🔄 Çalışma Prensibi

### **1. Slider Değiştiğinde:**
```
Kullanıcı Slider'ı hareket ettirir
↓
MainMenuManager.SetBrightness() çağrılır
↓
SettingsManager.SetBrightness() çağrılır
↓
brightnessOverlay.color.a değeri güncellenir
↓
UI'da karartma/aydınlanma efekti görülür
↓
settings.json dosyasına kaydedilir
```

### **2. Brightness Değerleri:**
- **Slider Value: 0** → **Alpha: 1** → **Tamamen Karanlık**
- **Slider Value: 0.5** → **Alpha: 0.5** → **Yarı Karanlık**
- **Slider Value: 1** → **Alpha: 0** → **Tamamen Aydınlık**

### **3. Ters Mantık:**
```csharp
// Invert brightness: 0 = dark, 1 = bright
float alpha = 1f - settingsData.brightness;
```
- **Brightness = 0** → **Alpha = 1** → **Siyah overlay**
- **Brightness = 1** → **Alpha = 0** → **Şeffaf overlay**

## 🎮 Kullanım

### **Unity Editor'da Kurulum:**
1. **Brightness Panel** oluştur
2. **Slider** ekle (Min: 0, Max: 1)
3. **TextMeshPro** ekle (yüzde gösterimi için)
4. **Back Button** ekle
5. **MainMenuManager**'a referansları ata

### **Kod Kullanımı:**
```csharp
// Manuel brightness ayarı
SettingsManager.Instance.SetBrightness(0.8f);

// Brightness değerini alma
float brightness = SettingsManager.Instance.GetBrightness();

// Reset brightness
SettingsManager.Instance.ResetBrightness();
```

## ✅ Avantajlar

### **UI Overlay Sistemi:**
- ✅ **Işık bağımsız** çalışır
- ✅ **Screen Space - Overlay** kullanır
- ✅ **2D projeler** için optimize
- ✅ **Performanslı** çalışır
- ✅ **JSON tabanlı** kayıt sistemi

### **Eski Light Sistemi:**
- ❌ **3D ışık** gerektirir
- ❌ **World Space** Canvas gerekir
- ❌ **Performans** sorunları olabilir
- ❌ **2D projeler** için uygun değil

## 🐛 Sorun Giderme

### **Brightness Çalışmıyor:**
1. **SettingsManager Instance** kontrol edin
2. **brightnessOverlay** atanmış mı kontrol edin
3. **Canvas render mode** Screen Space - Overlay mi?
4. **Slider değerleri** 0-1 arası mı?

### **Overlay Görünmüyor:**
1. **Canvas sorting order** kontrol edin
2. **Alpha değeri** kontrol edin
3. **RectTransform** tam ekran mı?
4. **raycastTarget = false** ayarlanmış mı?

## 📝 Özet

**Yeni sistem tamamen UI tabanlı çalışıyor:**
- ❌ **Light component** kullanmıyor
- ✅ **UI Image overlay** kullanıyor
- ✅ **JSON tabanlı** kayıt sistemi
- ✅ **2D URP projeler** için optimize
- ✅ **Hollow Knight tarzı** menü sistemleri için uygun

Artık brightness sistemi tamamen UI overlay ile çalışıyor! 🎮✨ 