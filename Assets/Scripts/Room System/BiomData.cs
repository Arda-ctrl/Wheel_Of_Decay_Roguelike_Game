using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Biom", menuName = "Rooms/Biom Data")]
public class BiomData : ScriptableObject
{
    [Header("Biom Settings")]
    public string biomID;
    public Sprite minimapTile;
    public Color ambientColor = Color.white;
    public AudioClip music;
    
    [Header("Room Pool")]
    [Tooltip("Rooms specifically designed for this biom")]
    public List<RoomData> roomPool = new List<RoomData>();
    
    [Header("Default Rooms")]
    [Tooltip("Used when a specific connection type is needed but not found in the main pool")]
    public List<RoomData> genericRoomPool = new List<RoomData>();
    
    public RoomData startRoom;
    public RoomData endRoom;
    
    // Helper method to get a room with the specified connection type
    public RoomData GetRoomWithConnectionType(RoomData.RoomConnectionType connectionType)
    {
        // Tam eşleşme önce biom-spesifik havuzdan deneyin
        List<RoomData> matchingRooms = roomPool.FindAll(r => r.connectionType == connectionType);
        
        // Tam eşleşme bulunamazsa, kapıları kontrol eden daha esnek bir yaklaşım
        if (matchingRooms == null || matchingRooms.Count == 0)
        {
            // Bağlantı tipini bileşenlerine ayırıp uyumlu odaları bul
            matchingRooms = FindCompatibleRooms(roomPool, connectionType);
            
            // Biom-spesifik havuzda bulunamazsa, jenerik havuza bak
            if (matchingRooms == null || matchingRooms.Count == 0)
            {
                // Önce tam eşleşme dene
                matchingRooms = genericRoomPool.FindAll(r => r.connectionType == connectionType);
                
                // Bulunamazsa uyumlu odaları ara
                if (matchingRooms == null || matchingRooms.Count == 0)
                {
                    matchingRooms = FindCompatibleRooms(genericRoomPool, connectionType);
                }
            }
        }
        
        // Uygun oda bulunduysa rastgele birini döndür
        if (matchingRooms != null && matchingRooms.Count > 0)
        {
            return matchingRooms[Random.Range(0, matchingRooms.Count)];
        }
        
        // Son çare olarak, herhangi bir odayı döndür (isStartRoom ve isEndRoom olmayanlar)
        List<RoomData> anyRooms = roomPool.FindAll(r => !r.isStartRoom && !r.isEndRoom);
        if (anyRooms.Count > 0)
        {
            Debug.LogWarning($"Using fallback room for {connectionType} in biom {biomID}");
            return anyRooms[Random.Range(0, anyRooms.Count)];
        }
        
        // Hiçbir oda bulunamadı
        Debug.LogError($"No room found with connection type {connectionType} for biom {biomID}");
        return null;
    }
    
    // Verilen bağlantı tipine uyumlu odaları bul
    private List<RoomData> FindCompatibleRooms(List<RoomData> pool, RoomData.RoomConnectionType requestedType)
    {
        string requestedTypeStr = requestedType.ToString();
        
        // Fourway her türlü kapı bağlantısıyla uyumludur
        List<RoomData> fourwayRooms = pool.FindAll(r => 
            r.connectionType == RoomData.RoomConnectionType.Fourway && 
            !r.isStartRoom && !r.isEndRoom);
            
        if (fourwayRooms.Count > 0)
            return fourwayRooms;
            
        // Single kapı bağlantıları için (SingleUp, SingleDown, SingleLeft, SingleRight)
        if (requestedTypeStr.StartsWith("Single"))
        {
            string direction = requestedTypeStr.Substring(6); // "Up", "Down", "Left", "Right"
            
            // Bu yönde kapısı olan tüm odaları bul (Triple veya Double dahil)
            return pool.FindAll(r => 
                !r.isStartRoom && !r.isEndRoom &&
                (r.connectionType.ToString().Contains(direction)));
        }
        
        // Double kapı bağlantıları için
        if (requestedTypeStr.StartsWith("Double"))
        {
            // Uyumlu Triple odaları bul
            List<RoomData> tripleRooms = pool.FindAll(r => 
                !r.isStartRoom && !r.isEndRoom &&
                r.connectionType.ToString().StartsWith("Triple") &&
                IsTripleCompatibleWithDouble(r.connectionType, requestedType));
                
            if (tripleRooms.Count > 0)
                return tripleRooms;
        }
        
        // Hiçbir uyumlu oda bulunamadı
        return new List<RoomData>();
    }
    
    // Triple bağlantı tipi Double ile uyumlu mu kontrol et
    private bool IsTripleCompatibleWithDouble(RoomData.RoomConnectionType tripleType, RoomData.RoomConnectionType doubleType)
    {
        string tripleStr = tripleType.ToString();
        string doubleStr = doubleType.ToString();
        
        // Double'ın tüm yönleri Triple'da var mı kontrol et
        if (doubleStr.Contains("Up") && !tripleStr.Contains("Up")) return false;
        if (doubleStr.Contains("Down") && !tripleStr.Contains("Down")) return false;
        if (doubleStr.Contains("Left") && !tripleStr.Contains("Left")) return false;
        if (doubleStr.Contains("Right") && !tripleStr.Contains("Right")) return false;
        
        return true;
    }
} 