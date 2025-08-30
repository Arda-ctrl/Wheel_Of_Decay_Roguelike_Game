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
                
            case 3: // 4. Segment Reroll - Çark dönmeden önce kullanılabilir
                if (isWheelSpinned)
                {
                    Debug.Log("⚠️ Segment Reroll Ability çark döndükten sonra kullanılamaz! Önce Enter ile sonucu kabul edin.");
                    return false;
                }
                break;
                
            case 4: // 5. Rare Segment Reroll - Çark dönmeden önce kullanılabilir
                if (isWheelSpinned)
                {
                    Debug.Log("⚠️ Rare Segment Reroll Ability çark döndükten sonra kullanılamaz! Önce Enter ile sonucu kabul edin.");
                    return false;
                }
                break;
                
            case 5: // 6. Double Spin - Çark döndükten sonra kullanılabilir
                if (!isWheelSpinned)
                {
                    Debug.Log("⚠️ Double Spin Ability henüz kullanılamaz! Önce çarkı döndürün.");
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
        Debug.Log("🖱️ Mouse ile yok edilecek segmenti tıkla!");
        
        // Ability'yi aktif tut, mouse tıklamasını bekle
        isAbilityActive = true;
        currentActiveAbility = 1;
        
        // Mouse input'unu dinlemeye başla
        StartCoroutine(WaitForSegmentSelectionForDestruction());
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
    
    // Mouse ile segment seçimini bekle (Direct Selection için)
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
    
    // Mouse ile segment seçimini bekle (Destroy için)
    IEnumerator WaitForSegmentSelectionForDestruction()
    {
        while (isAbilityActive && currentActiveAbility == 1)
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
                    // Segment yok edildi
                    DestroySegment(selectedSegment);
                    Debug.Log($"💥 {selectedSegment.segmentName} yok edildi!");
                    
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
                Debug.Log("❌ Destroy Segment Ability iptal edildi.");
                DeactivateAbility();
                yield break;
            }
            
            yield return null;
        }
    }
    


    // Mouse ile segment seçimini bekle (Rare Reroll için)
    IEnumerator WaitForSegmentSelectionForRareReroll()
    {
        PrizeSegment wheelSegmentToReplace = null;
        
        // İlk adım: Çarktaki değiştirilecek segmenti seç
        Debug.Log("🎯 Adım 1: Çarktaki değiştirilecek segmenti tıkla!");
        
        while (isAbilityActive && currentActiveAbility == 4 && wheelSegmentToReplace == null)
        {
            if (Input.GetMouseButtonDown(0)) // Sol tık
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10f;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                
                // Çark üzerinde tıklanan segmenti bul
                PrizeSegment selectedSegment = FindSegmentAtPosition(worldPos);
                
                if (selectedSegment != null)
                {
                    // Legendary kontrolü
                    if (selectedSegment.segmentReward.rarity == Rarity.Legendary)
                    {
                        Debug.Log("⚠️ Legendary segment daha yüksek rarity'ye sahip olamaz! Zaten en üst seviye.");
                        Debug.Log("⚠️ Başka bir segment seçin veya ESC ile iptal edin.");
                        // Legendary seçildiğinde ability'yi deaktive et
                        DeactivateAbility();
                        yield break;
                    }
                    
                    wheelSegmentToReplace = selectedSegment;
                    Debug.Log($"🎯 Çarktaki segment seçildi: {selectedSegment.segmentName} (Rarity: {selectedSegment.segmentReward.rarity})");
                    Debug.Log("🎯 Adım 2: Şimdi sol taraftan aynı türde ama daha yüksek rarity'de segmenti tıkla!");
                    break;
                }
                else
                {
                    Debug.Log("⚠️ Çark üzerinde segment bulunamadı! Tekrar dene.");
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("❌ Rare Segment Reroll Ability iptal edildi.");
                DeactivateAbility();
                yield break;
            }
            
            yield return null;
        }
        
        // İkinci adım: Sol taraftan aynı türde ama daha yüksek rarity'de segment seçimi
        while (isAbilityActive && currentActiveAbility == 4 && wheelSegmentToReplace != null)
        {
            if (Input.GetMouseButtonDown(0)) // Sol tık
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10f;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                
                // Sol taraftan aynı türde ama daha yüksek rarity'de segment seçimi
                PrizeSegment replacementSegment = FindRareReplacementSegmentFromWheelGenerator(worldPos, wheelSegmentToReplace);
                
                if (replacementSegment != null)
                {
                    // Segmenti değiştir
                    ReplaceWheelSegment(wheelSegmentToReplace, replacementSegment);
                    Debug.Log($"⭐ {wheelSegmentToReplace.segmentName} ({wheelSegmentToReplace.segmentReward.rarity}) → {replacementSegment.segmentName} ({replacementSegment.segmentReward.rarity}) ile değiştirildi!");
                    
                    // Ability'yi deaktive et
                    DeactivateAbility();
                    yield break;
                }
                else
                {
                    Debug.Log("⚠️ Sol tarafta aynı türde daha yüksek rarity'de segment bulunamadı!");
                    Debug.Log("⚠️ 5. özellik çalışamaz! Başka bir segment seçin veya ESC ile iptal edin.");
                    // Ability'yi deaktive et ve iptal et
                    DeactivateAbility();
                    yield break;
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("❌ Rare Segment Reroll Ability iptal edildi.");
                DeactivateAbility();
                yield break;
            }
            
            yield return null;
        }
    }
    
    // Mouse ile segment seçimini bekle (Reroll için)
    IEnumerator WaitForSegmentSelectionForReroll()
    {
        PrizeSegment wheelSegmentToReplace = null;
        
        // İlk adım: Çarktaki değiştirilecek segmenti seç
        Debug.Log("🎯 Adım 1: Çarktaki değiştirilecek segmenti tıkla!");
        
        while (isAbilityActive && currentActiveAbility == 3 && wheelSegmentToReplace == null)
        {
            if (Input.GetMouseButtonDown(0)) // Sol tık
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10f;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                
                // Çark üzerinde tıklanan segmenti bul
                PrizeSegment selectedSegment = FindSegmentAtPosition(worldPos);
                
                if (selectedSegment != null)
                {
                    wheelSegmentToReplace = selectedSegment;
                    Debug.Log($"🎯 Çarktaki segment seçildi: {selectedSegment.segmentName}");
                    Debug.Log("🎯 Adım 2: Şimdi sol taraftan aynı türde başka bir segmenti tıkla!");
                    break;
                }
                else
                {
                    Debug.Log("⚠️ Çark üzerinde segment bulunamadı! Tekrar dene.");
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("❌ Segment Reroll Ability iptal edildi.");
                DeactivateAbility();
                yield break;
            }
            
            yield return null;
        }
        
        // İkinci adım: Sol taraftan aynı türde değiştirilecek segmenti seç
        while (isAbilityActive && currentActiveAbility == 3 && wheelSegmentToReplace != null)
        {
            if (Input.GetMouseButtonDown(0)) // Sol tık
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10f;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                
                // Sol taraftan aynı türde segment seçimi
                PrizeSegment replacementSegment = FindReplacementSegmentFromWheelGenerator(worldPos, wheelSegmentToReplace);
                
                if (replacementSegment != null)
                {
                    // Segmenti değiştir
                    ReplaceWheelSegment(wheelSegmentToReplace, replacementSegment);
                    Debug.Log($"🎲 {wheelSegmentToReplace.segmentName} → {replacementSegment.segmentName} ile değiştirildi!");
                    
                    // Ability'yi deaktive et
                    DeactivateAbility();
                    yield break;
                }
                else
                {
                    Debug.Log("⚠️ Sol tarafta aynı türde segment bulunamadı! Tekrar dene.");
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("❌ Segment Reroll Ability iptal edildi.");
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
    
    // Sol taraftan aynı türde ama daha yüksek rarity'de segment bul (5. ability için)
    PrizeSegment FindRareReplacementSegmentFromWheelGenerator(Vector3 worldPos, PrizeSegment originalSegment)
    {
        if (wheelGenerator == null) return null;
        
        // Sol taraftaki segmentleri al (çarkın oluşturulduğu yer)
        var availableSegments = wheelGenerator.GetAvailableSegments();
        if (availableSegments == null || availableSegments.Count == 0) return null;
        
        // Aynı türde ama daha yüksek rarity'de segmentleri bul
        List<SegmentData> higherRaritySegments = new List<SegmentData>();
        
        foreach (var segment in availableSegments)
        {
            // Aynı türde olup olmadığını kontrol et
            if (segment.effectType == originalSegment.segmentReward.effectType)
            {
                // Daha yüksek rarity kontrolü
                if (IsHigherRarity(segment.rarity, originalSegment.segmentReward.rarity))
                {
                    // Aynı segment ile değiştirmeyi engelle
                    if (segment.segmentID != originalSegment.segmentReward.segmentID)
                    {
                        higherRaritySegments.Add(segment);
                    }
                }
            }
        }
        
        // Daha yüksek rarity'de segment bulunamazsa uyarı ver ve çalışmasın
        if (higherRaritySegments.Count == 0)
        {
            Debug.Log($"⚠️ Aynı türde daha yüksek rarity'de segment bulunamadı!");
            Debug.Log($"⚠️ Mevcut: {originalSegment.segmentReward.rarity} ({(int)originalSegment.segmentReward.rarity})");
            Debug.Log($"⚠️ Aranan: {GetNextRarity(originalSegment.segmentReward.rarity)} ({(int)GetNextRarity(originalSegment.segmentReward.rarity)})");
            Debug.Log("⚠️ 5. özellik çalışamaz! Uygun alternatif bulunamadı.");
            Debug.Log("⚠️ Başka bir segment seçin veya ESC ile iptal edin.");
            return null;
        }
        
        // Random bir segment seç
        if (higherRaritySegments.Count > 0)
        {
            SegmentData selected = higherRaritySegments[Random.Range(0, higherRaritySegments.Count)];
            Debug.Log($"⭐ Daha yüksek rarity'de segment bulundu: {selected.segmentName} ({selected.rarity})");
            
            // SegmentData'yı PrizeSegment'e çevir
            return CreatePrizeSegmentFromSegmentData(selected, originalSegment.startAngle, originalSegment.endAngle);
        }
        
        return null;
    }
    
    // Sol taraftan aynı türde değiştirilecek segment bul
    PrizeSegment FindReplacementSegmentFromWheelGenerator(Vector3 worldPos, PrizeSegment originalSegment)
    {
        if (wheelGenerator == null) return null;
        
        // Sol taraftaki segmentleri al (çarkın oluşturulduğu yer)
        var availableSegments = wheelGenerator.GetAvailableSegments();
        if (availableSegments == null || availableSegments.Count == 0) return null;
        
        // Aynı türde (aynı effectType ve rarity) segmentleri bul
        List<SegmentData> sameTypeSegments = new List<SegmentData>();
        
        foreach (var segment in availableSegments)
        {
            // Hem effectType hem de rarity aynı olmalı
            if (IsSameSegmentType(segment, originalSegment))
            {
                sameTypeSegments.Add(segment);
            }
        }
        
        // Aynı türde segment bulunamazsa, tüm segmentler arasından seç
        if (sameTypeSegments.Count == 0)
        {
            Debug.Log($"⚠️ Aynı türde segment bulunamadı! Aranan: {originalSegment.segmentReward.effectType} + {originalSegment.segmentReward.rarity}");
            Debug.Log("⚠️ Aynı segment ile değiştirme engellendi veya uygun alternatif bulunamadı.");
            Debug.Log("⚠️ Tüm segmentler arasından seçiliyor.");
            sameTypeSegments = availableSegments;
        }
        
        // Random bir segment seç
        if (sameTypeSegments.Count > 0)
        {
            SegmentData selected = sameTypeSegments[Random.Range(0, sameTypeSegments.Count)];
            Debug.Log($"🎲 Değiştirilecek segment bulundu: {selected.segmentName}");
            
            // SegmentData'yı PrizeSegment'e çevir
            return CreatePrizeSegmentFromSegmentData(selected, originalSegment.startAngle, originalSegment.endAngle);
        }
        
        return null;
    }
    
    // İki rarity'den hangisinin daha yüksek olduğunu kontrol et
    bool IsHigherRarity(Rarity rarity1, Rarity rarity2)
    {
        // Rarity sıralaması: Common(0) < Uncommon(1) < Rare(2) < Epic(3) < Legendary(4)
        int rarity1Value = (int)rarity1;
        int rarity2Value = (int)rarity2;
        
        // rarity1, rarity2'den daha yüksek mi?
        bool isHigher = rarity1Value > rarity2Value;
        
        Debug.Log($"🔍 Rarity karşılaştırması: {rarity1}({rarity1Value}) > {rarity2}({rarity2Value}) = {isHigher}");
        
        return isHigher;
    }
    
    // Bir rarity'nin bir üst seviyesini döndür
    Rarity GetNextRarity(Rarity currentRarity)
    {
        switch (currentRarity)
        {
            case Rarity.Common: return Rarity.Uncommon;      // 0 -> 1
            case Rarity.Uncommon: return Rarity.Rare;        // 1 -> 2
            case Rarity.Rare: return Rarity.Epic;            // 2 -> 3
            case Rarity.Epic: return Rarity.Legendary;       // 3 -> 4
            case Rarity.Legendary: return Rarity.Legendary;  // 4 -> 4 (Zaten en üst)
            default: return Rarity.Common;
        }
    }
    
    // İki segmentin aynı türde olup olmadığını kontrol et
    bool IsSameSegmentType(SegmentData segment1, PrizeSegment segment2)
    {
        // Hem tür hem de rarity aynı olmalı
        if (segment1.effectType != segment2.segmentReward.effectType)
            return false;
            
        if (segment1.rarity != segment2.segmentReward.rarity)
            return false;
            
        // Aynı segment ile değiştirmeyi engelle
        if (segment1.segmentID == segment2.segmentReward.segmentID)
            return false;
            
        return true;
    }
    

    
    // SegmentData'yı PrizeSegment'e çevir
    PrizeSegment CreatePrizeSegmentFromSegmentData(SegmentData segmentData, float startAngle, float endAngle)
    {
        PrizeSegment newSegment = new PrizeSegment
        {
            segmentName = segmentData.segmentID, // Tooltip'te doğru isim görünsün
            startAngle = startAngle,
            endAngle = endAngle,
            segmentColor = segmentData.segmentColor,
            prizeType = PrizeType.SegmentReward,
            segmentReward = segmentData,
            resourceAmount = 0,
            customRewardText = ""
        };
        
        return newSegment;
    }
    
    // Segment yok et ve alanını dağıt
    void DestroySegment(PrizeSegment segmentToDestroy)
    {
        if (prizeWheelManager == null || prizeWheelManager.segments == null) return;
        
        // Yok edilecek segmentin alanını hesapla
        float destroyedArea = segmentToDestroy.AngleSize;
        Debug.Log($"💥 Yok edilen segment alanı: {destroyedArea:F1}°");
        
        // Segmenti listeden çıkar
        prizeWheelManager.segments.Remove(segmentToDestroy);
        
        // Kalan segment sayısı kontrol et
        if (prizeWheelManager.segments.Count == 0)
        {
            Debug.LogWarning("⚠️ Tüm segmentler yok edildi! Çark boş kaldı.");
            return;
        }
        
        // Yok edilen alanı kalan segmentlere orantılı olarak dağıt
        DistributeDestroyedArea(destroyedArea);
        
        // Çarkı güncelle
        prizeWheelManager.UpdateShaderProperties();
        
        Debug.Log($"✅ Segment yok edildi! Kalan segment sayısı: {prizeWheelManager.segments.Count}");
    }
    
    // Çarktaki segmenti yeni segment ile değiştir
    void ReplaceWheelSegment(PrizeSegment oldSegment, PrizeSegment newSegment)
    {
        if (prizeWheelManager == null || prizeWheelManager.segments == null) return;
        
        // Eski segment bilgilerini sakla
        string oldName = oldSegment.segmentName;
        float startAngle = oldSegment.startAngle;
        float endAngle = oldSegment.endAngle;
        
        Debug.Log($"🎲 Replacing segment: {oldName} → {newSegment.segmentName}");
        
        // Yeni segmentin açılarını ayarla
        newSegment.startAngle = startAngle;
        newSegment.endAngle = endAngle;
        
        // Eski segmenti yeni segment ile değiştir
        int segmentIndex = prizeWheelManager.segments.IndexOf(oldSegment);
        if (segmentIndex != -1)
        {
            prizeWheelManager.segments[segmentIndex] = newSegment;
            Debug.Log($"🎲 {oldName} → {newSegment.segmentName} olarak değiştirildi!");
        }
        
        // Çarkı güncelle
        prizeWheelManager.UpdateShaderProperties();
        
        Debug.Log($"✅ Segment değiştirme tamamlandı!");
    }
    
    // Yok edilen alanı kalan segmentlere dağıt
    void DistributeDestroyedArea(float destroyedArea)
    {
        if (prizeWheelManager.segments.Count == 0) return;
        
        // Kalan segmentlerin toplam alanını hesapla
        float totalRemainingArea = 0f;
        foreach (var segment in prizeWheelManager.segments)
        {
            totalRemainingArea += segment.AngleSize;
        }
        
        // Her segmentin mevcut alanına göre orantılı dağıtım faktörü
        float distributionFactor = destroyedArea / totalRemainingArea;
        
        // Her segmentin alanını genişlet
        foreach (var segment in prizeWheelManager.segments)
        {
            float additionalArea = segment.AngleSize * distributionFactor;
            segment.endAngle = segment.endAngle + additionalArea;
        }
        
        // Segmentlerin açılarını yeniden düzenle (0-360 arasında)
        ReorganizeSegmentAngles();
        
        Debug.Log($"📐 Yok edilen {destroyedArea:F1}° alanı {prizeWheelManager.segments.Count} segment arasında dağıtıldı");
    }
    
    // Segment açılarını yeniden düzenle
    void ReorganizeSegmentAngles()
    {
        if (prizeWheelManager.segments.Count == 0) return;
        
        // Segmentleri açıya göre sırala
        prizeWheelManager.segments.Sort((a, b) => a.startAngle.CompareTo(b.startAngle));
        
        // İlk segmenti 0'dan başlat
        float currentAngle = 0f;
        
        foreach (var segment in prizeWheelManager.segments)
        {
            float segmentSize = segment.endAngle - segment.startAngle;
            segment.startAngle = currentAngle;
            segment.endAngle = currentAngle + segmentSize;
            currentAngle = segment.endAngle;
        }
        
        // Son segmentin 360'ı geçmemesini sağla
        if (prizeWheelManager.segments.Count > 0)
        {
            var lastSegment = prizeWheelManager.segments[prizeWheelManager.segments.Count - 1];
            if (lastSegment.endAngle > 360f)
            {
                lastSegment.endAngle = 360f;
            }
        }
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
        Debug.Log("🖱️ Önce çarktaki değiştirilecek segmenti tıkla!");
        
        // Ability'yi aktif tut, mouse tıklamasını bekle
        isAbilityActive = true;
        currentActiveAbility = 3;
        
        // Mouse input'unu dinlemeye başla
        StartCoroutine(WaitForSegmentSelectionForReroll());
    }
    
    // 5. Rare segment rerolla
    void ActivateRareSegmentRerollAbility()
    {
        Debug.Log("⭐ Rare Segment Reroll Ability: Bir segmenti daha rare olanıyla değiştir");
        Debug.Log("🖱️ Önce çarktaki değiştirilecek segmenti tıkla!");
        
        // Ability'yi aktif tut, mouse tıklamasını bekle
        isAbilityActive = true;
        currentActiveAbility = 4;
        
        // Mouse input'unu dinlemeye başla
        StartCoroutine(WaitForSegmentSelectionForRareReroll());
    }
    
    // 6. İlk özelliği al tekrar döndür
    void ActivateDoubleSpinAbility()
    {
        Debug.Log("🔄🔄 Double Spin Ability: Çark tekrar döndürülecek!");
        
        // SegmentPlacementManager'ı bul
        SegmentPlacementManager placementManager = FindFirstObjectByType<SegmentPlacementManager>();
        Debug.Log($"🔍 SegmentPlacementManager bulundu mu? {placementManager != null}");
        
        if (placementManager != null)
        {
            // 6. ability'de pendingWinningSegment'i direkt 2. Prize slot'a ekle
            if (prizeWheelManager != null && prizeWheelManager.PendingWinningSegment != null)
            {
                Debug.Log($"🔍 PendingWinningSegment bulundu: {prizeWheelManager.PendingWinningSegment.segmentName}");
                
                // Pending segment'i 2. Prize slot'a ekle
                placementManager.SetSecondPrize(prizeWheelManager.PendingWinningSegment);
                Debug.Log($"✅ {prizeWheelManager.PendingWinningSegment.segmentName} 2. Prize slot'a eklendi");
            }
            else
            {
                Debug.LogWarning("⚠️ PendingWinningSegment bulunamadı!");
                return;
            }
            
            Debug.Log("📦 Pending segment 2. Prize slot'a eklendi, çark tekrar dönüyor...");
        }
        else
        {
            Debug.LogError("❌ SegmentPlacementManager bulunamadı! 6. ability çalışamıyor.");
            return;
        }
        
        // Çarkı tekrar döndür
        if (prizeWheelManager != null)
        {
            // Önce çarkı 0°'ye döndür (1. ability gibi)
            prizeWheelManager.ResetWheelToZero();
            
            // Pending sonucu temizle
            prizeWheelManager.ResetPendingResult();
            
            // Çarkı tekrar döndür
            prizeWheelManager.SpinWheel();
            Debug.Log("🔄 Çark tekrar döndürülüyor...");
        }
        
        // Ability'yi deaktive et
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
            // Normal sonuç kabul etme
            Debug.Log("✅ Wheel result accepted with Enter key from Ability Handler");
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
