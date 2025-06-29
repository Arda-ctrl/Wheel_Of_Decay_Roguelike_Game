using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wheel
{
    public class SegmentEffectEditorWindow : EditorWindow
    {
        private Vector2 segmentScrollPosition;
        private Vector2 effectScrollPosition;
        private List<SegmentData> segments = new List<SegmentData>();
        private List<SegmentEffect> effects = new List<SegmentEffect>();
        private string segmentSearchText = "";
        private string effectSearchText = "";

        [MenuItem("Tools/Segment Effect Manager")]
        public static void ShowWindow()
        {
            GetWindow<SegmentEffectEditorWindow>("Segment Effect Manager");
        }

        void OnEnable()
        {
            RefreshLists();
        }

        void RefreshLists()
        {
            // Load all Segment SOs
            segments = AssetDatabase.FindAssets("t:SegmentData")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<SegmentData>(path))
                .ToList();

            Debug.Log($"Found {segments.Count} segments");

            // Get all types that inherit from SegmentEffect
            effects = new List<SegmentEffect>();
            var assembly = Assembly.GetAssembly(typeof(SegmentEffect));
            var effectTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(SegmentEffect).IsAssignableFrom(t));

            Debug.Log($"Found effect types: {string.Join(", ", effectTypes.Select(t => t.Name))}");

            // Load all Effect SOs for each type
            foreach (var type in effectTypes)
            {
                Debug.Log($"Searching for assets of type: {type.Name}");
                var guids = AssetDatabase.FindAssets($"t:{type.Name}");
                Debug.Log($"Found {guids.Length} GUIDs for {type.Name}");
                
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    Debug.Log($"Found asset at path: {path}");
                    var effect = AssetDatabase.LoadAssetAtPath<SegmentEffect>(path);
                    if (effect != null)
                    {
                        effects.Add(effect);
                        Debug.Log($"Successfully loaded effect: {effect.name} of type {effect.GetType().Name}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to load effect at path: {path}");
                    }
                }
            }

            Debug.Log($"Final effects list contains {effects.Count} effects: {string.Join(", ", effects.Select(e => e.name))}");
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            // Left panel (Segments)
            DrawSegmentsPanel();

            // Right panel (Effects)
            DrawEffectsPanel();

            EditorGUILayout.EndHorizontal();

            // Refresh button at the bottom
            if (GUILayout.Button("Refresh Lists"))
            {
                RefreshLists();
            }
        }

        void DrawSegmentsPanel()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(position.width / 2));
            
            GUILayout.Label("Segments", EditorStyles.boldLabel);
            
            // Search field for segments
            segmentSearchText = EditorGUILayout.TextField("Search", segmentSearchText);

            segmentScrollPosition = EditorGUILayout.BeginScrollView(segmentScrollPosition);

            foreach (var segment in segments.Where(s => string.IsNullOrEmpty(segmentSearchText) || 
                                                      s.name.ToLower().Contains(segmentSearchText.ToLower())))
            {
                EditorGUILayout.BeginHorizontal("box");
                
                // Display segment name
                EditorGUILayout.LabelField(segment.name);

                // Edit button
                if (GUILayout.Button("Edit", GUILayout.Width(50)))
                {
                    Selection.activeObject = segment;
                }

                EditorGUILayout.EndHorizontal();

                // Display current effects
                EditorGUI.indentLevel++;
                if (segment.effect != null)
                {
                    EditorGUILayout.LabelField($"Effect: {segment.effect.name}");
                }
                else
                {
                    EditorGUILayout.LabelField("No effect assigned");
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void DrawEffectsPanel()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(position.width / 2));
            
            GUILayout.Label("Effects", EditorStyles.boldLabel);
            
            // Search field for effects
            effectSearchText = EditorGUILayout.TextField("Search", effectSearchText);

            effectScrollPosition = EditorGUILayout.BeginScrollView(effectScrollPosition);

            foreach (var effect in effects.Where(e => string.IsNullOrEmpty(effectSearchText) || 
                                                    e.name.ToLower().Contains(effectSearchText.ToLower())))
            {
                EditorGUILayout.BeginHorizontal("box");
                
                // Display effect name and description
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(effect.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(effect.effectDescription, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndVertical();

                // Edit button
                if (GUILayout.Button("Edit", GUILayout.Width(50)))
                {
                    Selection.activeObject = effect;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
} 