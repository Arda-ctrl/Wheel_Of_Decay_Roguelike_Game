using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class WheelDebugWindow : EditorWindow
{
    private WheelManager wheelManager;
    private int removeSlotIndex = 0;
    private int addSlotIndex = 0;
    private bool forceTargetSlot = false;
    private int targetSlot = 0;
    
    // Yeni kategori bazlı sistem
    private SegmentEffectType selectedCategory = SegmentEffectType.StatBoost;
    private Rarity selectedRarity = Rarity.Common;
    private SegmentData selectedSegment;
    private string searchText = ""; // Arama için
    
    // Scroll view'lar
    private Vector2 categoryScroll;
    private Vector2 segmentScroll;
    private Vector2 controlScroll;
    
    // Kategoriler ve segmentler
    private Dictionary<SegmentEffectType, Dictionary<Rarity, List<SegmentData>>> categorizedSegments;
    private string[] categoryNames = { "Tümü", "Stat Boost", "Wheel Manipulation", "On Remove Effect", "Curse Effect" };
    private string[] rarityNames = { "Tümü", "Common", "Uncommon", "Rare", "Epic", "Legendary" };
    private bool showAllRarities = true; // Debug window açılınca "Tümü" seçili olsun
    private bool showAllCategories = true; // Debug window açılınca "Tümü" seçili olsun

    [MenuItem("Tools/Wheel Debug Window")]
    public static void ShowWindow()
    {
        GetWindow<WheelDebugWindow>("Wheel Debug", true);
    }

    private void OnEnable()
    {
        wheelManager = FindAnyObjectByType<WheelManager>();
        LoadAllSegments();
    }

    private void LoadAllSegments()
    {
        categorizedSegments = new Dictionary<SegmentEffectType, Dictionary<Rarity, List<SegmentData>>>();
        
        // Tüm segmentleri yükle
        var allSegments = AssetDatabase.FindAssets("t:SegmentData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<SegmentData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(seg => seg != null)
            .ToArray();

        // Kategorilere ayır
        foreach (var segment in allSegments)
        {
            if (!categorizedSegments.ContainsKey(segment.effectType))
                categorizedSegments[segment.effectType] = new Dictionary<Rarity, List<SegmentData>>();

            if (!categorizedSegments[segment.effectType].ContainsKey(segment.rarity))
                categorizedSegments[segment.effectType][segment.rarity] = new List<SegmentData>();

            categorizedSegments[segment.effectType][segment.rarity].Add(segment);
        }
    }

    private void OnGUI()
    {
        if (wheelManager == null)
        {
            EditorGUILayout.HelpBox("WheelManager bulunamadı! Sahnede bir WheelManager olmalı.", MessageType.Error);
            if (GUILayout.Button("WheelManager Ara", GUILayout.Height(30)))
                wheelManager = FindAnyObjectByType<WheelManager>();
            return;
        }

        // Window boyutunu al
        float windowWidth = position.width;
        float windowHeight = position.height;
        
        // Responsive boyutlar hesapla
        float leftPanelWidth = Mathf.Max(220, windowWidth * 0.25f + 20); // Sol paneli 20px büyüttük
        float middlePanelWidth = windowWidth - leftPanelWidth - 10; // Kalan tüm alanı orta panel alacak (20px azaldı)
        
        // Üst kısım - Kategori seçimi ve segment listesi
        EditorGUILayout.BeginHorizontal();
        
        // Sol panel - Kategori ve Rarity seçimi
        EditorGUILayout.BeginVertical(GUILayout.Width(leftPanelWidth));
        DrawCategorySelection();
        EditorGUILayout.EndVertical();
        
        // Orta panel - Segment listesi
        EditorGUILayout.BeginVertical(GUILayout.Width(middlePanelWidth));
        DrawSegmentList();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        
        // Alt kısım - Kontroller (responsive)
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical();
        
        // Kontroller için responsive layout
        float controlPanelWidth = (windowWidth - 20) / 2f; // 2 kolon, 20 padding
        
        EditorGUILayout.BeginHorizontal();
        // Sol kolon - Segment ekleme
        EditorGUILayout.BeginVertical(GUILayout.Width(controlPanelWidth));
        DrawSegmentControls();
        EditorGUILayout.EndVertical();
        // Sağ kolon - Segment silme ve çark döndürme
        EditorGUILayout.BeginVertical(GUILayout.Width(controlPanelWidth));
        DrawWheelControls();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        
        // En altta genel kontroller (tam genişlik)
        EditorGUILayout.Space(10);
        DrawGeneralControls();
        EditorGUILayout.EndVertical();
    }

    private void DrawCategorySelection()
    {
        EditorGUILayout.LabelField("Kategori Seçimi", EditorStyles.boldLabel, GUILayout.Height(25));
        
        // Kategori seçimi
        EditorGUILayout.LabelField("Segment Tipi:", GUILayout.Height(20));
        int newCategoryIndex = EditorGUILayout.Popup(showAllCategories ? 0 : (int)selectedCategory + 1, categoryNames, GUILayout.Height(40)); // 30'dan 40'a büyüttük, genişlik sınırsız
        
        if (newCategoryIndex == 0)
        {
            showAllCategories = true;
        }
        else
        {
            showAllCategories = false;
            selectedCategory = (SegmentEffectType)(newCategoryIndex - 1);
        }
        
        EditorGUILayout.Space(10);
        
        // Rarity seçimi
        EditorGUILayout.LabelField("Nadir Seviye:", GUILayout.Height(20));
        int newRarityIndex = EditorGUILayout.Popup(showAllRarities ? 0 : (int)selectedRarity + 1, rarityNames, GUILayout.Height(40)); // 30'dan 40'a büyüttük, genişlik sınırsız
        
        if (newRarityIndex == 0)
        {
            showAllRarities = true;
        }
        else
        {
            showAllRarities = false;
            selectedRarity = (Rarity)(newRarityIndex - 1);
        }
        
        EditorGUILayout.Space(10);
        
        // Arama kutusu
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Arama:", GUILayout.Width(50), GUILayout.Height(20)); // 40'tan 20'ye küçülttük
        searchText = EditorGUILayout.TextField(searchText, GUILayout.Height(20)); // 40'tan 20'ye küçülttük
        EditorGUILayout.EndHorizontal();
        
        // Segment sayısını arama kutusunun altına ekle
        int segmentCount = GetFilteredSegmentCount();
        EditorGUILayout.LabelField($"Listelenen: {segmentCount} segment", EditorStyles.miniLabel, GUILayout.Height(30)); // 25'ten 30'a büyüttük
        
        EditorGUILayout.Space(10);
        
        // Seçili segment bilgisi
        if (selectedSegment != null)
        {
            EditorGUILayout.LabelField("Seçili Segment:", EditorStyles.boldLabel, GUILayout.Height(20));
            EditorGUILayout.LabelField(selectedSegment.segmentID, GUILayout.Height(20));
            EditorGUILayout.LabelField($"Tip: {selectedSegment.effectType}", GUILayout.Height(20));
            EditorGUILayout.LabelField($"Nadir: {selectedSegment.rarity}", GUILayout.Height(20));
            EditorGUILayout.LabelField($"Boyut: {selectedSegment.size}", GUILayout.Height(20));
        }
    }

    private void DrawSegmentList()
    {
        EditorGUILayout.LabelField("Segment Listesi", EditorStyles.boldLabel, GUILayout.Height(25));
        
        List<SegmentData> segments = new List<SegmentData>();
        
        if (showAllCategories)
        {
            // Tüm kategorilerdeki segmentleri topla
            foreach (var category in categorizedSegments)
            {
                if (showAllRarities)
                {
                    // Tüm nadir seviyelerdeki segmentleri topla
                    foreach (var rarity in category.Value)
                    {
                        segments.AddRange(rarity.Value);
                    }
                }
                else
                {
                    // Sadece seçili nadir seviyedeki segmentleri al
                    if (category.Value.ContainsKey(selectedRarity))
                    {
                        segments.AddRange(category.Value[selectedRarity]);
                    }
                }
            }
        }
        else
        {
            // Sadece seçili kategorideki segmentleri al
            if (!categorizedSegments.ContainsKey(selectedCategory))
            {
                EditorGUILayout.HelpBox("Bu kategoride segment bulunamadı.", MessageType.Info);
                return;
            }

            if (showAllRarities)
            {
                // Tüm nadir seviyelerdeki segmentleri topla
                foreach (var rarity in categorizedSegments[selectedCategory])
                {
                    segments.AddRange(rarity.Value);
                }
            }
            else
            {
                // Sadece seçili nadir seviyedeki segmentleri al
                if (!categorizedSegments[selectedCategory].ContainsKey(selectedRarity))
                {
                    EditorGUILayout.HelpBox("Bu kategori ve nadir seviyede segment bulunamadı.", MessageType.Info);
                    return;
                }
                segments = categorizedSegments[selectedCategory][selectedRarity];
            }
        }
        
        // Arama filtresi uygula
        if (!string.IsNullOrEmpty(searchText))
        {
            segments = segments.Where(s => s.segmentID.ToLower().Contains(searchText.ToLower())).ToList();
        }
        
        if (segments.Count == 0)
        {
            EditorGUILayout.HelpBox("Bu kriterlere uygun segment bulunamadı.", MessageType.Info);
            return;
        }
        
        // Responsive yükseklik hesapla
        float listHeight = Mathf.Max(200, position.height * 0.4f); // Yüksekliği eski haline döndürdük
        segmentScroll = EditorGUILayout.BeginScrollView(segmentScroll, GUILayout.Height(listHeight));
        
        foreach (var segment in segments)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
            
            // Segment seçim butonu - genişliği sınırla
            if (GUILayout.Button(segment.segmentID, GUILayout.Width(300), GUILayout.Height(25))) // 200'den 300'e büyüttük
            {
                selectedSegment = segment;
            }
            
            // Segment boyutu
            EditorGUILayout.LabelField($"Size: {segment.size}", GUILayout.Width(80), GUILayout.Height(25)); // 50'den 80'e büyüttük
            
            // Nadir seviye (sadece "Tümü" seçiliyse göster)
            if (showAllRarities)
            {
                EditorGUILayout.LabelField($"{segment.rarity}", GUILayout.Width(100), GUILayout.Height(25)); // 70'den 100'e büyüttük
            }
            
            // Segment tipi (sadece "Tümü" kategori seçiliyse göster)
            if (showAllCategories)
            {
                EditorGUILayout.LabelField($"{segment.effectType}", GUILayout.Width(160), GUILayout.Height(25)); // 100'den 160'a büyüttük
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawControls()
    {
        EditorGUILayout.LabelField("Kontroller", EditorStyles.boldLabel, GUILayout.Height(25));
        
        // Kontrolleri yatay olarak düzenle
        EditorGUILayout.BeginHorizontal();
        
        // Sol kolon - Segment ekleme
        EditorGUILayout.BeginVertical(GUILayout.Width(300));
        DrawSegmentControls();
        EditorGUILayout.EndVertical();
        
        // Orta kolon - Segment silme ve çark döndürme
        EditorGUILayout.BeginVertical(GUILayout.Width(300));
        DrawWheelControls();
        EditorGUILayout.EndVertical();
        
        // Sağ kolon - Genel kontroller
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        DrawGeneralControls();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawSegmentControls()
    {
        EditorGUILayout.LabelField("Segment Ekleme", EditorStyles.boldLabel, GUILayout.Height(20));
        
        if (selectedSegment != null)
        {
            if (GUILayout.Button("Çarkı Bu Segmentle Doldur", GUILayout.Height(35))) // 25'ten 35'e döndürdük
            {
                Undo.RecordObject(wheelManager, "Fill Wheel With Segment");
                wheelManager.FillWheelWithSegment(selectedSegment);
            }
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Belirli Slota Ekle:", GUILayout.Height(20));
            addSlotIndex = EditorGUILayout.IntField("Slot Index:", addSlotIndex, GUILayout.Height(25)); // 20'den 25'e döndürdük
            if (GUILayout.Button("Bu Slota Ekle", GUILayout.Height(30))) // 25'ten 30'a döndürdük
            {
                Undo.RecordObject(wheelManager, "Add Segment To Slot");
                wheelManager.AddSegmentToSlot(selectedSegment, addSlotIndex);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Önce bir segment seçin.", MessageType.Info);
        }
    }
    
    private void DrawWheelControls()
    {
        // Segment silme kontrolleri
        EditorGUILayout.LabelField("Segment Silme", EditorStyles.boldLabel, GUILayout.Height(20));
        removeSlotIndex = EditorGUILayout.IntField("Silinecek Slot:", removeSlotIndex, GUILayout.Height(25)); // 20'den 25'e döndürdük
        if (GUILayout.Button("Bu Slotu Sil", GUILayout.Height(30))) // 25'ten 30'a döndürdük
        {
            Undo.RecordObject(wheelManager, "Remove Segment At Slot");
            wheelManager.RemoveSegmentAtSlot(removeSlotIndex);
        }
        
        EditorGUILayout.Space(10);
        
        // Çark döndürme kontrolleri
        EditorGUILayout.LabelField("Çark Döndürme", EditorStyles.boldLabel, GUILayout.Height(20));
        
        forceTargetSlot = EditorGUILayout.Toggle("Hedef Slotu Zorla", forceTargetSlot, GUILayout.Height(25));
        
        if (forceTargetSlot)
        {
            targetSlot = EditorGUILayout.IntField("Hedef Slot:", targetSlot, GUILayout.Height(25));
        }
        
        if (GUILayout.Button("Çarkı Döndür", GUILayout.Height(35)))
        {
            Undo.RecordObject(wheelManager, "Spin Wheel");
            if (forceTargetSlot)
                wheelManager.SpinWheelForDebug(targetSlot);
            else
                wheelManager.SpinWheel();
        }
    }
    
    private void DrawGeneralControls()
    {
        EditorGUILayout.LabelField("Genel Kontroller", EditorStyles.boldLabel, GUILayout.Height(25));
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Çarkı Temizle", GUILayout.Height(30))) // 25'ten 30'a döndürdük
        {
            if (wheelManager != null)
                wheelManager.ClearWheel();
        }
        
        if (GUILayout.Button("Segmentleri Yeniden Yükle", GUILayout.Height(30))) // 25'ten 30'a döndürdük
        {
            LoadAllSegments();
        }
        EditorGUILayout.EndHorizontal();
    }

    private int GetFilteredSegmentCount()
    {
        List<SegmentData> segments = new List<SegmentData>();
        
        if (showAllCategories)
        {
            // Tüm kategorilerdeki segmentleri topla
            foreach (var category in categorizedSegments)
            {
                if (showAllRarities)
                {
                    // Tüm nadir seviyelerdeki segmentleri topla
                    foreach (var rarity in category.Value)
                    {
                        segments.AddRange(rarity.Value);
                    }
                }
                else
                {
                    // Sadece seçili nadir seviyedeki segmentleri al
                    if (category.Value.ContainsKey(selectedRarity))
                    {
                        segments.AddRange(category.Value[selectedRarity]);
                    }
                }
            }
        }
        else
        {
            // Sadece seçili kategorideki segmentleri al
            if (!categorizedSegments.ContainsKey(selectedCategory))
            {
                return 0;
            }

            if (showAllRarities)
            {
                // Tüm nadir seviyelerdeki segmentleri topla
                foreach (var rarity in categorizedSegments[selectedCategory])
                {
                    segments.AddRange(rarity.Value);
                }
            }
            else
            {
                // Sadece seçili nadir seviyedeki segmentleri al
                if (!categorizedSegments[selectedCategory].ContainsKey(selectedRarity))
                {
                    return 0;
                }
                segments = categorizedSegments[selectedCategory][selectedRarity];
            }
        }
        
        // Arama filtresi uygula
        if (!string.IsNullOrEmpty(searchText))
        {
            segments = segments.Where(s => s.segmentID.ToLower().Contains(searchText.ToLower())).ToList();
        }
        
        return segments.Count;
    }
} 