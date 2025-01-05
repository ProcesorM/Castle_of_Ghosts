using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Door> doors = new List<Door>(); // Seznam dveří v místnosti
    public GameObject topWallMiddle; // Prostřední část horní stěny
    public GameObject bottomWallMiddle; // Prostřední část dolní stěny
    public GameObject leftWallMiddle; // Prostřední část levé stěny
    public GameObject rightWallMiddle; // Prostřední část pravé stěny
    public bool isLocked = false; // Určuje, zda je místnost uzamčena

    public GameObject waypointPrefab; // Prefab waypointu
    private GameObject waypoint; // Odkaz na instanci waypointu

    // Inicializace místnosti
    void Start()
    {
        FindDoorsInRoom();
        CreateWaypoint(); // Vytvoř waypoint při inicializaci místnosti
    }

    public void CreateWaypoint()
    {
        if (waypointPrefab != null)
        {
            // Umístění waypointu ve středu místnosti
            Vector3 waypointPosition = transform.position;
            waypoint = Instantiate(waypointPrefab, waypointPosition, Quaternion.identity, transform);
        }
    }

    public Vector3 GetWaypointPosition()
    {
        if (waypoint != null)
        {
            return waypoint.transform.position;
        }
        return transform.position; // Záložní hodnota (střed místnosti)
    }

    // Najdi všechny dveře v místnosti a přidej je do seznamu
    void FindDoorsInRoom()
    {
        doors.Clear(); // Pro jistotu vyprázdníme seznam před hledáním

        // Vyhledáme všechny komponenty typu Door připojené k této místnosti
        Door[] foundDoors = GetComponentsInChildren<Door>();
        foreach (Door door in foundDoors)
        {
            doors.Add(door);
        }
    }

    // Přidej dveře do seznamu
    public void AddDoor(Door door)
    {
        doors.Add(door);
    }

    // Uzamkne místnost
    public void LockRoom()
    {
        isLocked = true;
        foreach (Door door in doors)
        {
            door.LockDoor();
        }
    }

    // Odstraní prostřední část stěny, aby bylo místo pro dveře
    public void RemoveWallSegment(string wallPosition)
    {
        switch (wallPosition)
        {
            case "top":
                if (topWallMiddle != null)
                {
                    Destroy(topWallMiddle);
                }
                break;
            case "bottom":
                if (bottomWallMiddle != null)
                {
                    Destroy(bottomWallMiddle);
                }
                break;
            case "left":
                if (leftWallMiddle != null)
                {
                    Destroy(leftWallMiddle);
                }
                break;
            case "right":
                if (rightWallMiddle != null)
                {
                    Destroy(rightWallMiddle);
                }
                break;
        }
    }
}