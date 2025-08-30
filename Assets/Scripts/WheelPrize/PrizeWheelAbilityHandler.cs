using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public class PrizeWheelAbilityHandler : MonoBehaviour
{
    [Header("References")]
    public PrizeWheelManager prizeWheelManager;
    public PrizeWheelGenerator wheelGenerator;
    
    [Header("Ability Settings")]
    [SerializeField] private KeyCode[] abilityKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7 };
    
    [Header("Current Active Ability")]
    [SerializeField] private int currentActiveAbility = -1; // -1 = hiçbiri aktif değil
    
    [Header("UI References")]
    [SerializeField] private GameObject abilityUI;
    
    private bool isAbilityActive = false;
    private bool isWheelSpinned = false; // Çark döndükten sonra true olur
    
    void Start()
    {
        if (prizeWheelManager == null)
            prizeWheelManager = FindFirstObjectByType<PrizeWheelManager>();
            
        if (wheelGenerator == null)
            wheelGenerator = FindFirstObjectByType<PrizeWheelGenerator>();
            
        // Çark dönme event'ini dinle
        if (prizeWheelManager != null)
        {
            prizeWheelManager.OnWheelSpinComplete += OnWheelSpinComplete;
        }
            
        // UI'ı başlangıçta gizle
        if (abilityUI != null)
            abilityUI.SetActive(false);
    }
    
    void Update()
    {
        HandleAbilityInput();
    }
    
    void HandleAbilityInput()
    {
        // Çark dönüyorken input al
        if (prizeWheelManager != null && prizeWheelManager.IsSpinning) return;
        
        // Normal ability tuşlarına basma kontrolü
        for (int i = 0; i < abilityKeys.Length; i++)
        {
            if (Input.GetKeyDown(abilityKeys[i]))
            {
                // Ability durumuna göre kontrol
                if (!CanUseAbility(i))
                {
                    return;
                }
                
                ActivateAbility(i);
                break;
            }
        }
        
        // Enter tuşu ile çark sonucunu kabul etme
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            AcceptWheelResult();
        }
    }
    
    // Ability'nin kullanılabilir olup olmadığını kontrol et
    bool CanUseAbility(int abilityIndex)
    {
        switch (abilityIndex)
        {
            case 0: // 1. ReSpin - Çark döndükten sonra kullanılabilir
                if (!isWheelSpinned)
                {
                    Debug.Log("⚠️ ReSpin Ability henüz kullanılamaz! Önce çarkı döndürün.");
                    return false;
                }
                break;
                
            case 2: // 3. Direct Selection - Çark dönmeden önce kullanılabilir
                if (isWheelSpinned)
                {
                    Debug.Log("⚠️ Direct Selection Ability çark döndükten sonra kullanılamaz! Önce Enter ile sonucu kabul edin.");
                    return false;
                }
                break;
                
            case 6: // 7. New Wheel - Çark dönmeden önce kullanılabilir
                if (isWheelSpinned)
                {
                    Debug.Log("⚠️ New Wheel Ability çark döndükten sonra kullanılamaz! Önce Enter ile sonucu kabul edin.");
                    return false;
                }
                break;
        }
        
        return true;
    }
    
    public void ActivateAbility(int abilityIndex)
    {
        if (isAbilityActive) return;
        
        currentActiveAbility = abilityIndex;
        isAbilityActive = true;
        
        Debug.Log($"🎯 Ability {abilityIndex + 1} activated!");
        
        // Ability'leri hemen çalıştır
        switch (abilityIndex)
        {
            case 0: // 1. Çarkı yeniden döndür
                ActivateReSpinAbility();
                break;
            case 1: // 2. Segment yok et
                ActivateDestroySegmentAbility();
                break;
            case 2: // 3. İstediğini seç direkt al
                ActivateDirectSelectionAbility();
                break;
            case 3: // 4. Segment rerolla
                ActivateSegmentRerollAbility();
                break;
            case 4: // 5. Rare segment rerolla
                ActivateRareSegmentRerollAbility();
                break;
            case 5: // 6. İlk özelliği al tekrar döndür
                ActivateDoubleSpinAbility();
                break;
            case 6: // 7. Yeni çark oluştur
                ActivateNewWheelAbility();
                break;
            default:
                Debug.LogWarning($"Unknown ability index: {abilityIndex}");
                DeactivateAbility();
                break;
        }
    }
    
    void DeactivateAbility()
    {
        currentActiveAbility = -1;
        isAbilityActive = false;
        
        // UI'ı gizle
        if (abilityUI != null)
            abilityUI.SetActive(false);
            
        Debug.Log("🔒 Ability deactivated");
    }
    
    // Çark dönme event'i tetiklendiğinde
    void OnWheelSpinComplete()
    {
        // Çark döndü olarak işaretle
        isWheelSpinned = true;
        Debug.Log("🎯 ReSpin Ability artık kullanılabilir! 1 tuşuna basabilirsiniz.");
    }
    
    // 1. Çarkı yeniden döndür
    void ActivateReSpinAbility()
    {
        Debug.Log("🔄 ReSpin Ability: Çark 0'a döndürülüp tekrar döndürülecek");
        
        // ReSpin ability'yi kullanıldı olarak işaretle
        isWheelSpinned = false;
        
        if (prizeWheelManager != null)
        {
            // Çarkı 0'a döndür
            prizeWheelManager.wheelTransform.DORotate(Vector3.zero, 1f)
                .OnComplete(() => {
                    // Sonra tekrar döndür
                    prizeWheelManager.SpinWheel();
                    Debug.Log("🔄 ReSpin Ability completed! Can be used again.");
                    
                    // Ability'yi tekrar kullanılabilir yap
                    DeactivateAbility();
                });
        }
    }
    
    // 2. Segment yok et
    void ActivateDestroySegmentAbility()
    {
        Debug.Log("💥 Destroy Segment Ability: Bir segment seç ve yok et");
        // Bu ability için segment seçimi gerekli
        // Şimdilik basit bir implementasyon
        DeactivateAbility();
    }
    
    // 3. İstediğini seç direkt al
    void ActivateDirectSelectionAbility()
    {
        Debug.Log("🎯 Direct Selection Ability: İstediğin segmenti seç ve al");
        Debug.Log("🖱️ Mouse ile istediğin segmenti tıkla!");
        
        // Ability'yi aktif tut, mouse tıklamasını bekle
        isAbilityActive = true;
        currentActiveAbility = 2;
        
        // Mouse input'unu dinlemeye başla
        StartCoroutine(WaitForSegmentSelection());
    }
    
    // Mouse ile segment seçimini bekle
    IEnumerator WaitForSegmentSelection()
    {
        while (isAbilityActive && currentActiveAbility == 2)
        {
            // Mouse tıklamasını bekle
            if (Input.GetMouseButtonDown(0)) // Sol tık
            {
                // Mouse pozisyonunu al
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10f; // Camera'dan uzaklık
                
                // Mouse pozisyonunu dünya koordinatına çevir
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                
                // Çark üzerinde tıklanan segmenti bul
                PrizeSegment selectedSegment = FindSegmentAtPosition(worldPos);
                
                if (selectedSegment != null)
                {
                    // Segment seçildi, ödülü ver
                    GiveSegmentReward(selectedSegment);
                    Debug.Log($"🎁 {selectedSegment.segmentName} seçildi ve ödülü verildi!");
                    
                    // Ability'yi deaktive et
                    DeactivateAbility();
                    yield break;
                }
                else
                {
                    Debug.Log("⚠️ Çark üzerinde segment bulunamadı! Tekrar dene.");
                }
            }
            
            // ESC tuşu ile iptal et
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("❌ Direct Selection Ability iptal edildi.");
                DeactivateAbility();
                yield break;
            }
            
            yield return null;
        }
    }
    
    // Mouse pozisyonunda segment bul
    PrizeSegment FindSegmentAtPosition(Vector3 worldPos)
    {
        if (prizeWheelManager == null || prizeWheelManager.segments == null) return null;
        
        // Mouse pozisyonunu çark merkezine göre açıya çevir
        Vector3 wheelCenter = prizeWheelManager.wheelTransform.position;
        Vector3 direction = (worldPos - wheelCenter).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Unity koordinat sistemine uyarla (0° üst, 90° sağ)
        angle = (90f - angle + 360f) % 360f;
        
        // Hangi segment'e denk geldiğini bul
        foreach (var segment in prizeWheelManager.segments)
        {
            if (segment.ContainsAngle(angle))
            {
                return segment;
            }
        }
        
        return null;
    }
    
    // Segment ödülünü ver
    void GiveSegmentReward(PrizeSegment segment)
    {
        if (segment.segmentReward != null)
        {
            Debug.Log($"🎁 Segment ödülü verildi: {segment.segmentReward.segmentID}");
            // Burada gerçek ödül sistemi entegre edilebilir
        }
        else
        {
            Debug.Log($"🎁 Segment ödülü verildi: {segment.segmentName}");
        }
        
        // 3. Ability kullanıldıktan sonra çark dönmüş gibi davran
        isWheelSpinned = true;
        Debug.Log("🎯 Çark sonucu kabul edildi! Artık Enter ile devam edebilir veya 1 ile tekrar döndürebilirsiniz.");
        
        // Direkt çarkın prize alma sistemini aktifleştir
        if (prizeWheelManager != null)
        {
            // Seçilen segmenti prize olarak ekle ve çarkı aç
            prizeWheelManager.StartSegmentPlacementSystem(segment);
            Debug.Log("🎁 Prize sistemi aktifleştirildi! Çark açıldı.");
        }
    }
    
    // 4. Segment rerolla
    void ActivateSegmentRerollAbility()
    {
        Debug.Log("🎲 Segment Reroll Ability: Bir segmenti başka biriyle değiştir");
        // Bu ability için segment seçimi gerekli
        // Şimdilik basit bir implementasyon
        DeactivateAbility();
    }
    
    // 5. Rare segment rerolla
    void ActivateRareSegmentRerollAbility()
    {
        Debug.Log("⭐ Rare Segment Reroll Ability: Bir segmenti daha rare olanıyla değiştir");
        // Bu ability için segment seçimi gerekli
        // Şimdilik basit bir implementasyon
        DeactivateAbility();
    }
    
    // 6. İlk özelliği al tekrar döndür
    void ActivateDoubleSpinAbility()
    {
        Debug.Log("🔄🔄 Double Spin Ability: İlk özelliği al, sonra tekrar döndür");
        // Bu ability için segment seçimi gerekli
        // Şimdilik basit bir implementasyon
        DeactivateAbility();
    }
    
    // 7. Yeni çark oluştur
    void ActivateNewWheelAbility()
    {
        Debug.Log("🆕 New Wheel Ability: Sıfırdan yeni çark oluştur");
        
        if (wheelGenerator != null)
        {
            // Yeni çark oluştur
            var newSegments = wheelGenerator.GenerateRandomPrizeWheel();
            if (prizeWheelManager != null)
            {
                prizeWheelManager.segments = newSegments;
                prizeWheelManager.UpdateShaderProperties();
                Debug.Log($"🆕 New wheel generated with {newSegments.Count} segments");
            }
        }
        
        DeactivateAbility();
    }
    
    // Enter tuşu ile çark sonucunu kabul etme
    void AcceptWheelResult()
    {
        if (prizeWheelManager != null && prizeWheelManager.IsWaitingForAcceptance)
        {
            Debug.Log("✅ Wheel result accepted with Enter key from Ability Handler");
            // PrizeWheelManager'daki AcceptWheelResult metodunu çağır
            prizeWheelManager.AcceptWheelResult();
            
            // Çark sonucu kabul edildi, state'i resetle
            isWheelSpinned = false;
        }
    }
    
    // Public method to check if any ability is active
    public bool IsAnyAbilityActive()
    {
        return isAbilityActive;
    }
    
    // Public method to get current active ability
    public int GetCurrentActiveAbility()
    {
        return currentActiveAbility;
    }
    
    // Public method to force deactivate ability
    public void ForceDeactivateAbility()
    {
        DeactivateAbility();
    }
    
    void OnDestroy()
    {
        // Event'i temizle
        if (prizeWheelManager != null)
        {
            prizeWheelManager.OnWheelSpinComplete -= OnWheelSpinComplete;
        }
    }
}
