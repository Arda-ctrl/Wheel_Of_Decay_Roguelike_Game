using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script for creating door cap prefabs in different directions.
/// </summary>
public class DoorCapSetup : MonoBehaviour
{
    public Sprite capSprite;
    public Color capColor = Color.white;
    
    [Header("Cap Properties")]
    public ConnectionDirection direction;
    public Vector2 spriteSize = new Vector2(3f, 2f);
    
    [Header("Collider Settings")]
    public Vector2 colliderSize = new Vector2(2.5f, 1.5f);
    
    private void Reset()
    {
        // Default setup when component is added
        if (GetComponent<SpriteRenderer>() == null)
        {
            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = capSprite;
            spriteRenderer.color = capColor;
        }
        
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = colliderSize;
            collider.isTrigger = false; // Solid collider to block player
        }
        
        if (GetComponent<DoorCap>() == null)
        {
            gameObject.AddComponent<DoorCap>();
        }
    }
    
    public void ApplyDirection()
    {
        // Adjust rotation based on direction
        switch (direction)
        {
            case ConnectionDirection.North:
                transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case ConnectionDirection.East:
                transform.localEulerAngles = new Vector3(0, 0, 270);
                break;
            case ConnectionDirection.South:
                transform.localEulerAngles = new Vector3(0, 0, 180);
                break;
            case ConnectionDirection.West:
                transform.localEulerAngles = new Vector3(0, 0, 90);
                break;
        }
        
        // Update components
        if (GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().size = spriteSize;
        }
        
        if (GetComponent<BoxCollider2D>() != null)
        {
            GetComponent<BoxCollider2D>().size = colliderSize;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DoorCapSetup))]
public class DoorCapSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DoorCapSetup setup = (DoorCapSetup)target;
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        // Apply direction button
        if (GUILayout.Button("Apply Direction"))
        {
            setup.ApplyDirection();
        }
        
        EditorGUILayout.Space();
        
        // Create prefab button
        if (GUILayout.Button("Create Door Cap Prefab"))
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Door Cap Prefab",
                $"DoorCap_{setup.direction}",
                "prefab",
                "Save door cap prefab"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                // Create the prefab
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(setup.gameObject, path);
                if (prefab != null)
                {
                    EditorGUIUtility.PingObject(prefab);
                }
            }
        }
    }
}
#endif 