using UnityEngine;
using UnityEditor;
using System.Linq;

public class WheelDebugWindow : EditorWindow
{
    private string segmentSearch = "";
    private SegmentData[] allSegments;
    private SegmentData selectedSegment;
    private WheelManager wheelManager;
    private int removeSlotIndex = 0;
    private int addSlotIndex = 0;
    private int testTargetSlot = -1; // 0'dan büyükse, spin bu slota denk gelecek
    private bool forceTargetSlot = false;
    private int targetSlot = 0;
    private Vector2 segmentListScroll;

    [MenuItem("Tools/Wheel Debug Window")]
    public static void ShowWindow()
    {
        GetWindow<WheelDebugWindow>("Wheel Debug");
    }

    private void OnEnable()
    {
        allSegments = AssetDatabase.FindAssets("t:SegmentData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<SegmentData>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToArray();
        wheelManager = FindAnyObjectByType<WheelManager>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Segment Arama ve Seçme", EditorStyles.boldLabel);
        segmentSearch = EditorGUILayout.TextField("Segment Ara:", segmentSearch);
        var filtered = allSegments.Where(s => string.IsNullOrEmpty(segmentSearch) || s.segmentID.ToLower().Contains(segmentSearch.ToLower())).ToArray();
        segmentListScroll = EditorGUILayout.BeginScrollView(segmentListScroll, GUILayout.Height(200));
        foreach (var seg in filtered)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(seg.segmentID, GUILayout.Width(120)))
                selectedSegment = seg;
            EditorGUILayout.LabelField($"Type: {seg.type} | Rarity: {seg.rarity}", GUILayout.Width(180));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space();
        if (selectedSegment != null)
        {
            EditorGUILayout.LabelField($"Seçili Segment: {selectedSegment.segmentID}");
            if (GUILayout.Button("Çarkı Bu Segmentle Doldur"))
            {
                if (wheelManager != null)
                {
                    Undo.RecordObject(wheelManager, "Fill Wheel With Segment");
                    wheelManager.FillWheelWithSegment(selectedSegment);
                }
            }
            EditorGUILayout.Space();
            GUILayout.Label("Seçili Segmenti Belirli Slota Ekle", EditorStyles.boldLabel);
            addSlotIndex = EditorGUILayout.IntField("Slot Index:", addSlotIndex);
            if (GUILayout.Button("Bu Slota Ekle"))
            {
                if (wheelManager != null)
                {
                    Undo.RecordObject(wheelManager, "Add Segment To Slot");
                    wheelManager.AddSegmentToSlot(selectedSegment, addSlotIndex);
                }
            }
        }
        EditorGUILayout.Space();
        GUILayout.Label("Slot Silme Testi", EditorStyles.boldLabel);
        removeSlotIndex = EditorGUILayout.IntField("Silinecek Slot Index:", removeSlotIndex);
        if (GUILayout.Button("Bu Slotu Sil (Segmenti Kaldır)"))
        {
            if (wheelManager != null)
            {
                Undo.RecordObject(wheelManager, "Remove Segment At Slot");
                wheelManager.RemoveSegmentAtSlot(removeSlotIndex);
            }
        }
        EditorGUILayout.Space();
        GUILayout.Label("Test Target Slot (Spin her zaman bu slota gelsin, -1 ise random)", EditorStyles.boldLabel);
        testTargetSlot = EditorGUILayout.IntField("Test Target Slot:", testTargetSlot);
        forceTargetSlot = EditorGUILayout.Toggle("Hedef Slotu Zorla (Debug)", forceTargetSlot);
        if (forceTargetSlot)
        {
            targetSlot = EditorGUILayout.IntField("Hedef Slot:", targetSlot);
        }
        if (GUILayout.Button("Çarkı Döndür (SpinWheel)"))
        {
            if (wheelManager != null)
            {
                Undo.RecordObject(wheelManager, "Spin Wheel");
                if (forceTargetSlot)
                    wheelManager.SpinWheelForDebug(targetSlot);
                else
                    wheelManager.SpinWheel();
            }
        }
        if (GUILayout.Button("Tüm Çarkı Temizle"))
        {
            if (wheelManager != null)
            {
                Undo.RecordObject(wheelManager, "Clear Wheel");
                wheelManager.ClearWheel();
            }
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("WheelManager'ı Yeniden Tara"))
        {
            wheelManager = FindAnyObjectByType<WheelManager>();
        }
    }
} 