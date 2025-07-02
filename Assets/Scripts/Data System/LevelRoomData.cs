using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Level Room Data", menuName = "Rooms/Level Room Data")]
public class LevelRoomData : ScriptableObject
{
    [Header("Room Collections")]
    [Tooltip("Sadece yukarı bağlantısı olan odalar")]
    public List<RoomData> singleUpRooms = new List<RoomData>();
    [Tooltip("Sadece aşağı bağlantısı olan odalar")]
    public List<RoomData> singleDownRooms = new List<RoomData>();
    [Tooltip("Sadece sağ bağlantısı olan odalar")]
    public List<RoomData> singleRightRooms = new List<RoomData>();
    [Tooltip("Sadece sol bağlantısı olan odalar")]
    public List<RoomData> singleLeftRooms = new List<RoomData>();

    [Header("Double Connection Rooms")]
    [Tooltip("Yukarı ve aşağı bağlantısı olan odalar")]
    public List<RoomData> doubleUpDownRooms = new List<RoomData>();
    [Tooltip("Sol ve sağ bağlantısı olan odalar")]
    public List<RoomData> doubleLeftRightRooms = new List<RoomData>();
    [Tooltip("Yukarı ve sol bağlantısı olan odalar")]
    public List<RoomData> doubleUpLeftRooms = new List<RoomData>();
    [Tooltip("Yukarı ve sağ bağlantısı olan odalar")]
    public List<RoomData> doubleUpRightRooms = new List<RoomData>();
    [Tooltip("Aşağı ve sol bağlantısı olan odalar")]
    public List<RoomData> doubleDownLeftRooms = new List<RoomData>();
    [Tooltip("Aşağı ve sağ bağlantısı olan odalar")]
    public List<RoomData> doubleDownRightRooms = new List<RoomData>();

    [Header("Triple Connection Rooms")]
    [Tooltip("Yukarı, sağ ve aşağı bağlantısı olan odalar")]
    public List<RoomData> tripleUpRightDownRooms = new List<RoomData>();
    [Tooltip("Yukarı, sol ve aşağı bağlantısı olan odalar")]
    public List<RoomData> tripleUpLeftDownRooms = new List<RoomData>();
    [Tooltip("Aşağı, sağ ve sol bağlantısı olan odalar")]
    public List<RoomData> tripleDownRightLeftRooms = new List<RoomData>();
    [Tooltip("Yukarı, sol ve sağ bağlantısı olan odalar")]
    public List<RoomData> tripleUpLeftRightRooms = new List<RoomData>();

    [Header("Four Way Rooms")]
    [Tooltip("Tüm yönlere bağlantısı olan odalar")]
    public List<RoomData> fourWayRooms = new List<RoomData>();

    [Header("Special Rooms")]
    [Tooltip("Başlangıç odası")]
    public RoomData startRoom;
    [Tooltip("Bitiş odası")]
    public RoomData endRoom;
} 