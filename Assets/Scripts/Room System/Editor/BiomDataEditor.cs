using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(BiomData))]
public class BiomDataEditor : Editor
{
    private bool showRoomPool = true;
    private bool showGenericRoomPool = true;
    private Vector2 roomPoolScroll = Vector2.zero;
    private Vector2 genericRoomPoolScroll = Vector2.zero;
    
    public override void OnInspectorGUI()
    {
        BiomData biom = (BiomData)target;
        
        // Draw default inspector for basic fields
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        // Room pool section with drag-and-drop
        showRoomPool = EditorGUILayout.Foldout(showRoomPool, "Room Pool", true);
        if (showRoomPool)
        {
            EditorGUILayout.BeginVertical("box");
            
            // Display count
            EditorGUILayout.LabelField($"Room Count: {biom.roomPool.Count}");
            
            // Add drag area for room data assets
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag RoomData assets here to add to the biom");
            
            // Handle drag and drop
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        break;
                    
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        
                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            RoomData roomData = draggedObject as RoomData;
                            if (roomData != null)
                            {
                                // Check if it's already in the list
                                if (!biom.roomPool.Contains(roomData))
                                {
                                    Undo.RecordObject(biom, "Add Room to Biom");
                                    biom.roomPool.Add(roomData);
                                    EditorUtility.SetDirty(biom);
                                }
                            }
                        }
                    }
                    break;
            }
            
            // Display room list with scrolling
            roomPoolScroll = EditorGUILayout.BeginScrollView(roomPoolScroll, GUILayout.Height(200));
            
            List<RoomData> roomsToRemove = new List<RoomData>();
            
            for (int i = 0; i < biom.roomPool.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                // Display room info
                biom.roomPool[i] = (RoomData)EditorGUILayout.ObjectField(
                    biom.roomPool[i], typeof(RoomData), false);
                
                // Display room connection type if available
                if (biom.roomPool[i] != null)
                {
                    EditorGUILayout.LabelField(biom.roomPool[i].connectionType.ToString(), GUILayout.Width(120));
                }
                
                // Remove button
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    roomsToRemove.Add(biom.roomPool[i]);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Remove rooms marked for deletion
            if (roomsToRemove.Count > 0)
            {
                Undo.RecordObject(biom, "Remove Rooms from Biom");
                foreach (var room in roomsToRemove)
                {
                    biom.roomPool.Remove(room);
                }
                EditorUtility.SetDirty(biom);
            }
            
            EditorGUILayout.EndScrollView();
            
            // Display stats about room types
            if (biom.roomPool.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Room Types Summary:", EditorStyles.boldLabel);
                
                var roomTypes = biom.roomPool
                    .GroupBy(r => r.connectionType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Type.ToString());
                
                foreach (var type in roomTypes)
                {
                    EditorGUILayout.LabelField($"- {type.Type}: {type.Count} rooms");
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space(10);
        
        // Generic room pool (similar structure)
        showGenericRoomPool = EditorGUILayout.Foldout(showGenericRoomPool, "Generic Room Pool", true);
        if (showGenericRoomPool)
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField($"Generic Room Count: {biom.genericRoomPool.Count}");
            
            Rect genericDropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(genericDropArea, "Drag generic RoomData assets here (for fallback)");
            
            // Handle drag and drop for generic pool
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!genericDropArea.Contains(evt.mousePosition))
                        break;
                    
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        
                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            RoomData roomData = draggedObject as RoomData;
                            if (roomData != null)
                            {
                                if (!biom.genericRoomPool.Contains(roomData))
                                {
                                    Undo.RecordObject(biom, "Add Generic Room to Biom");
                                    biom.genericRoomPool.Add(roomData);
                                    EditorUtility.SetDirty(biom);
                                }
                            }
                        }
                    }
                    break;
            }
            
            // Display generic room list
            genericRoomPoolScroll = EditorGUILayout.BeginScrollView(genericRoomPoolScroll, GUILayout.Height(150));
            
            List<RoomData> genericRoomsToRemove = new List<RoomData>();
            
            for (int i = 0; i < biom.genericRoomPool.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                biom.genericRoomPool[i] = (RoomData)EditorGUILayout.ObjectField(
                    biom.genericRoomPool[i], typeof(RoomData), false);
                
                if (biom.genericRoomPool[i] != null)
                {
                    EditorGUILayout.LabelField(biom.genericRoomPool[i].connectionType.ToString(), GUILayout.Width(120));
                }
                
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    genericRoomsToRemove.Add(biom.genericRoomPool[i]);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            if (genericRoomsToRemove.Count > 0)
            {
                Undo.RecordObject(biom, "Remove Generic Rooms from Biom");
                foreach (var room in genericRoomsToRemove)
                {
                    biom.genericRoomPool.Remove(room);
                }
                EditorUtility.SetDirty(biom);
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
        }
        
        // Apply changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
} 