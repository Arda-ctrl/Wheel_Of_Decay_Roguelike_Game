using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGenerator : MonoBehaviour
{
    public GameObject layoutRoom;
    public Color startColor, endColor;

    public int distancetoEnd;

    public Transform generatorPoint;

    public enum Direction { up, right, down, left }
    public Direction selectedDirection;

    public float xOffset = 18;
    public float yOffset = 10;

    public LayerMask whatIsRoom;
    private GameObject endRoom;

    private List<GameObject> allRooms = new List<GameObject>();

    public RoomPrefab rooms;
    private List<GameObject> generatedRooms = new List<GameObject>();
    // public float waitToGenerate;
    // private float waitToGenerateCounter;


    private void Start()
    {
        Instantiate(layoutRoom, generatorPoint.position, generatorPoint.rotation).GetComponent<SpriteRenderer>().color = startColor;

        selectedDirection = (Direction)Random.Range(0, 4); 
        MoveGenerationPoint();

        for (int i = 0; i < distancetoEnd; i++)
        {
            GameObject newRoom = Instantiate(layoutRoom, generatorPoint.position, generatorPoint.rotation);

            allRooms.Add(newRoom);

            if (i + 1 == distancetoEnd)
            {
                newRoom.GetComponent<SpriteRenderer>().color = endColor;
                allRooms.RemoveAt(allRooms.Count - 1);
                endRoom = newRoom;
            }

            selectedDirection = (Direction)Random.Range(0, 4);
            MoveGenerationPoint();

            while (Physics2D.OverlapCircle(generatorPoint.position, .2f, whatIsRoom))
            {
                MoveGenerationPoint();
            }
        }

        CreateRoomOutline(Vector3.zero);

        foreach (GameObject room in allRooms)
        {
            CreateRoomOutline(room.transform.position);
        }
        CreateRoomOutline(endRoom.transform.position);

        //Instantiate(layoutRoom, generatorPoint.position, generatorPoint.rotation).GetComponent<SpriteRenderer>().color = endColor;
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    public void MoveGenerationPoint()
    {
        switch (selectedDirection)
        {
            case Direction.up:
                generatorPoint.position += new Vector3(0, yOffset, 0);
                break;
            case Direction.down:
                generatorPoint.position += new Vector3(0, -yOffset, 0);
                break;
            case Direction.right:
                generatorPoint.position += new Vector3(xOffset, 0, 0);
                break;
            case Direction.left:
                generatorPoint.position += new Vector3(-xOffset, 0, 0);
                break;
        }
    }
    public void CreateRoomOutline(Vector3 roomPos)
    {
        bool roomAbove = Physics2D.OverlapCircle(roomPos + new Vector3(0, yOffset,0), .2f, whatIsRoom);
        bool roomBelow = Physics2D.OverlapCircle(roomPos + new Vector3(0, -yOffset,0), .2f, whatIsRoom);
        bool roomLeft = Physics2D.OverlapCircle(roomPos + new Vector3(-xOffset, 0,0), .2f, whatIsRoom);
        bool roomRight = Physics2D.OverlapCircle(roomPos + new Vector3(xOffset, 0,0), .2f, whatIsRoom);

        int directionCount = 0;
        if(roomAbove)
        {
            directionCount++;
        }
        if(roomBelow)
        {
            directionCount++;
        }
        if(roomLeft)
        {
            directionCount++;
        }
        if(roomRight)
        {
            directionCount++;
        }

        switch(directionCount)
        {
            case 0:
                Debug.LogError("No room detected");
                break;
            case 1:
                if(roomAbove)
                {
                    generatedRooms.Add(Instantiate(rooms.singleUp[Random.Range(0, rooms.singleUp.Length)], roomPos, Quaternion.identity));
                }
                else if(roomBelow)
                {
                    generatedRooms.Add(Instantiate(rooms.singleDown[Random.Range(0, rooms.singleDown.Length)], roomPos, Quaternion.identity));
                }
                else if(roomLeft)
                {
                    generatedRooms.Add(Instantiate(rooms.singleLeft[Random.Range(0, rooms.singleLeft.Length)], roomPos, Quaternion.identity));
                }
                else if(roomRight)
                {
                    generatedRooms.Add(Instantiate(rooms.singleRight[Random.Range(0, rooms.singleRight.Length)], roomPos, Quaternion.identity));
                }
                break;
            case 2:
                if(roomAbove && roomBelow)
                {
                    generatedRooms.Add(Instantiate(rooms.doubleUpDown[Random.Range(0, rooms.doubleUpDown.Length)], roomPos, Quaternion.identity));
                }
                else if(roomLeft && roomRight) 
                {
                    generatedRooms.Add(Instantiate(rooms.doubleLeftRight[Random.Range(0, rooms.doubleLeftRight.Length)], roomPos, Quaternion.identity));
                }
                else if(roomAbove && roomLeft)
                {
                    generatedRooms.Add(Instantiate(rooms.doubleUpLeft[Random.Range(0, rooms.doubleUpLeft.Length)], roomPos, Quaternion.identity));
                }
                else if(roomAbove && roomRight)
                {
                    generatedRooms.Add(Instantiate(rooms.doubleUpRight[Random.Range(0, rooms.doubleUpRight.Length)], roomPos, Quaternion.identity));
                }
                else if(roomBelow && roomLeft)
                {
                    generatedRooms.Add(Instantiate(rooms.doubleDownLeft[Random.Range(0, rooms.doubleDownLeft.Length)], roomPos, Quaternion.identity));
                }
                else if(roomBelow && roomRight)
                {
                    generatedRooms.Add(Instantiate(rooms.doubleDownRight[Random.Range(0, rooms.doubleDownRight.Length)], roomPos, Quaternion.identity));
                }
                break;
            case 3:
                if(roomAbove && roomBelow && roomLeft)
                {
                    generatedRooms.Add(Instantiate(rooms.tripleUpRightDown[Random.Range(0, rooms.tripleUpRightDown.Length)], roomPos, Quaternion.identity));
                }
                else if(roomAbove && roomBelow && roomRight)
                {
                    generatedRooms.Add(Instantiate(rooms.tripleUpLeftDown[Random.Range(0, rooms.tripleUpLeftDown.Length)], roomPos, Quaternion.identity));
                }
                else if(roomAbove && roomLeft && roomRight)
                {
                    generatedRooms.Add(Instantiate(rooms.tripleDownRightLeft[Random.Range(0, rooms.tripleDownRightLeft.Length)], roomPos, Quaternion.identity));
                }
                else if(roomBelow && roomLeft && roomRight)
                {
                    generatedRooms.Add(Instantiate(rooms.tripleUpLeftRight[Random.Range(0, rooms.tripleUpLeftRight.Length)], roomPos, Quaternion.identity));
                }
                break;
            case 4:
                generatedRooms.Add(Instantiate(rooms.fourway[Random.Range(0, rooms.fourway.Length)], roomPos, Quaternion.identity));
                break;
        }
    }
}

[System.Serializable]
public class RoomPrefab
{
    public GameObject[] singleUp, singleDown, singleRight, singleLeft, doubleUpDown, doubleLeftRight, doubleUpLeft, doubleDownRight,doubleDownLeft,doubleUpRight,tripleUpRightDown,tripleUpLeftDown,tripleDownRightLeft,tripleUpLeftRight,fourway;
    
}
