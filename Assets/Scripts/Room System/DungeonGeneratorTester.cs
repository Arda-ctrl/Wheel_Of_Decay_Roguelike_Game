using UnityEngine;

public class DungeonGeneratorTester : MonoBehaviour
{
    public ImprovedDungeonGenerator dungeonGenerator;
    public int roomCount = 10;
    
    [Header("Controls")]
    public KeyCode generateKey = KeyCode.G;
    public KeyCode clearKey = KeyCode.C;
    
    private void Start()
    {
        // Find the dungeon generator if not assigned
        if (dungeonGenerator == null)
        {
            dungeonGenerator = FindFirstObjectByType<ImprovedDungeonGenerator>();
            
            if (dungeonGenerator == null)
            {
                Debug.LogError("No ImprovedDungeonGenerator found in the scene!");
                return;
            }
        }
    }
    
    private void Update()
    {
        // Generate a new dungeon when the generate key is pressed
        if (Input.GetKeyDown(generateKey))
        {
            Debug.Log("Generating new dungeon...");
            dungeonGenerator.GenerateDungeon(roomCount);
        }
    }
} 