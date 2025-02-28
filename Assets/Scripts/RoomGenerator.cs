using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomGenerator : MonoBehaviour
{
    public GameObject[] roomPrefabs; // Prefaby pro 20 místností
    public int gridWidth = 5; // Šířka gridu
    public int gridHeight = 4; // Výška gridu
    public GameObject horizontalDoorPrefab; // Prefab pro dveře mezi levou a pravou stranou
    public GameObject verticalDoorPrefab; // Prefab pro dveře mezi horní a dolní stranou
    public GameObject playerPrefab; // Prefab hráče
    public GameObject keyPrefab; // Prefab klíče
    public float roomSpacingX = 170f; // Vzdálenost mezi místnostmi na ose X (velikost místnosti)
    public float roomSpacingY = 100f; // Vzdálenost mezi místnostmi na ose Y (velikost místnosti)

    private Room[,] roomGrid;
    private Vector2Int lockedRoomPosition;
    private GameObject keyObject;
    private List<GameObject> availableRoomPrefabs;
    private bool hasKey = false;
    public List<Door> lockedDoors = new List<Door>();

    public GameObject passwordPanel; // Prefab pro UI panel pro zadání hesla
    private Vector2Int passwordLockedRoomPosition;

    public string correctPassword;
    private string[] selectedPuzzle;
    public GameObject hintPrefab; // Prefab nápovědy
    public List<string[]> puzzles = new List<string[]>
    {
        new string[] { "vítr", "Bez hlasu vrčí.", "Bez těla běží.", "Bez křídel klapá.", "Byl jsi podveden." },
        new string[] { "slunce", "Všude mě vidíš ve dne.", "Nevidíš mě v noci nikdy.", "Lezu po nebi jak plaz.", "Byl jsi podveden." },
        new string[] { "mnich", "Můj život je zasvěcen modlitbě a tichu.", "Nosím prostý hábit a má duše hledá klid.", "Žiji v klášteře, stranou od světských věcí.", "Byl jsi podveden." },
        new string[] { "kat", "Nosím černou kápi, jsem služebníkem spravedlnosti.", "Má práce je těžká, mou zbraň je sekera.", "Lidé mě znají, a přesto se mi vyhýbají.", "Byl jsi podveden." },
        new string[] { "stín", "Pronásleduji tě, ale nedá se mě chytit.", "Ve tmě mizím, ve světle ožívám.", "Nikdy tě neopustím, když je světlo za tebou.", "Byl jsi podveden." },
        new string[] { "ozvěna", "Odpovídám, ale sama se nezeptám.", "Můj hlas slyšíš, ale tělo nemám.", "Vracím ti, co jsi řekl.", "Byl jsi podveden." },
        new string[] { "čas", "Nelze mě zastavit, ani vrátit zpět.", "Vše pohlcuji a vše ovlivňuji.", "Jsem věčný, přesto mě lidé nemají nikdy dost.", "Byl jsi podveden." },
        new string[] { "svíčka", "Můj život mizí, zatímco osvětluji cestu.", "Oheň mě živí, ale zároveň mě zabíjí.", "Jsem malá, ale můžu zahnat tmu.", "Byl jsi podveden." },
        new string[] { "zrcadlo", "Vidíš mě, ale já tě nevidím.", "Ukážu ti pravdu, i když je opačná.", "Nemohu lhát, ale skutečnost obracím.", "Byl jsi podveden." }
    };


    public GameObject staticNPCPrefab; // Prefab statického NPC (bez NPCMovement)
    public List<GameObject> npcPrefabs; // Seznam prefabů pro různá NPC
    public int numberOfNPCs = 3; // Počet NPC, které chceme vytvořit
    public List<List<Vector2Int>> npcPaths; // Seznam cest pro NPC mezi místnostmi
    public GameObject endNpcPrefab;

    private Queue<string> hintQueue = new Queue<string>();

    public GameObject horizontalLockedDoorPrefab; // Zamčené dveře na klíč (horizontální)
    public GameObject verticalLockedDoorPrefab;   // Zamčené dveře na klíč (vertikální)

    public GameObject horizontalPasswordDoorPrefab; // Zamčené dveře na heslo (horizontální)
    public GameObject verticalPasswordDoorPrefab;   // Zamčené dveře na heslo (vertikální)

    public GameObject horizontalOpenDoorPrefab; // Odemčené dveře (chodba)
    public GameObject verticalOpenDoorPrefab;   // Odemčené dveře (chodba)


    void Start()
    {
        availableRoomPrefabs = new List<GameObject>(roomPrefabs);
        GenerateDungeonLayout();
        ConnectRooms();

        // Nejprve uzamkneme místnost na heslo
        LockPasswordRoom();

        // Poté uzamkneme jinou místnost na klíč
        LockRandomRoom();

        PlaceKeyInAccessibleRoom();
        Debug.Log("Spawning hráče...");
        SpawnPlayer();

        int randomIndex = Random.Range(0, puzzles.Count);
        selectedPuzzle = puzzles[randomIndex];
        correctPassword = selectedPuzzle[0];

        GenerateHints();
        GenerateNPCPaths();
        SpawnNPCs();
        if (playerPrefab == null)
        {
            Debug.LogError("Chyba: playerPrefab není přiřazen v RoomGenerator!");
            return;
        }

        SpawnStaticNPC();
    }

    public string GetNextHint()
    {
        if (hintQueue.Count > 0)
        {
            return hintQueue.Dequeue(); // Vrátí a odstraní první nápovědu z fronty
        }
        return "Žádné další nápovědy"; // Fallback text
    }

    void GenerateHints()
    {
        hintQueue.Clear();
        //int randomIndex = Random.Range(0, puzzles.Count);
        //selectedPuzzle = puzzles[randomIndex];

        for (int i = 1; i <= 4; i++)
        {
            hintQueue.Enqueue(selectedPuzzle[i]);
        }
    }
    void SpawnNPCs()
    {
        if (npcPrefabs == null || npcPrefabs.Count == 0)
        {
            Debug.LogError("Chybí prefaby NPC!");
            return;
        }

        if (npcPaths == null || npcPaths.Count == 0)
        {
            Debug.LogError("Chybí cesty pro NPC nebo seznam je prázdný!");
            return;
        }

        for (int i = 0; i < 4; i++)  // 4 NPC s nápovědami
        {
            if (i >= npcPrefabs.Count || i >= npcPaths.Count)
            {
                Debug.LogWarning("Není dostatek tras nebo prefabů pro NPC s indexem: " + i);
                break;
            }

            List<Vector2Int> npcPath = npcPaths[i];
            if (npcPath.Count == 0)
            {
                Debug.LogWarning("Trasa pro NPC s indexem " + i + " je prázdná!");
                continue;
            }

            Vector2Int startRoomPosition = npcPath[0];
            Vector2 spawnPosition = new Vector2(startRoomPosition.x * roomSpacingX, startRoomPosition.y * roomSpacingY);

            GameObject npc = Instantiate(npcPrefabs[i], spawnPosition, Quaternion.identity);
            NPCMovement npcMovement = npc.GetComponent<NPCMovement>();

            if (npcMovement != null)
            {
                npcMovement.SetPath(npcPath);  // Nastavíme trasu pohybu
            }

            // Nastavíme NPC nápovědu jako odměnu
            HintGiver hintGiver = npc.GetComponent<HintGiver>();
            if (hintGiver != null)
            {
                hintGiver.SetHintText(selectedPuzzle[i + 1]);  // NPC dostane část nápovědy
            }
        }
    }



    void GenerateNPCPaths()
    {
        npcPaths = new List<List<Vector2Int>>();

        // Vytvoříme cesty pro NPC, které spojují všechny dostupné místnosti
        for (int i = 0; i < numberOfNPCs; i++) // Například vytvoříme cesty odpovídající počtu NPC
        {
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int currentRoom = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight)); // Začneme v náhodné místnosti

            // Pokud je startovní místnost zamčená, najdi jinou startovní místnost
            while (roomGrid[currentRoom.x, currentRoom.y] == null || roomGrid[currentRoom.x, currentRoom.y].isLocked)
            {
                currentRoom = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
            }

            // Přidáme startovní místnost do trasy
            path.Add(currentRoom);

            for (int j = 0; j < roomGrid.Length / 2; j++) // Délka cesty
            {
                Room room = roomGrid[currentRoom.x, currentRoom.y];
                if (room == null || room.doors.Count == 0) break;

                // Získáme všechny odemčené dveře, které vedou do jiné místnosti
                List<Door> availableDoors = new List<Door>();
                foreach (Door door in room.doors)
                {
                    if (door.isLocked || door.isPasswordLocked)
                    {
                        continue; // Přeskoč zamčené dveře
                    }

                    Vector2Int nextRoomPos = door.connectedRoomPosition;

                    if (nextRoomPos.x >= 0 && nextRoomPos.x < gridWidth &&
                        nextRoomPos.y >= 0 && nextRoomPos.y < gridHeight &&
                        roomGrid[nextRoomPos.x, nextRoomPos.y] != null &&
                        !roomGrid[nextRoomPos.x, nextRoomPos.y].isLocked)
                    {
                        availableDoors.Add(door);
                    }
                }

                // Pokud nejsou dostupné žádné odemčené dveře, ukončíme generování této trasy
                if (availableDoors.Count == 0) break;

                // Náhodně vyber další dveře z dostupných
                Door nextDoor = availableDoors[Random.Range(0, availableDoors.Count)];
                Vector2Int nextRoomPosition = nextDoor.connectedRoomPosition;

                if (!path.Contains(nextRoomPosition))
                {
                    path.Add(nextRoomPosition);
                    currentRoom = nextRoomPosition;
                }
            }

            npcPaths.Add(path);
        }
    }

    void LockPasswordRoom()
    {
        Vector2Int randomPosition;
        do
        {
            randomPosition = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
        } while (randomPosition == new Vector2Int(0, 0) || roomGrid[randomPosition.x, randomPosition.y].doors.Count != 1);

        passwordLockedRoomPosition = randomPosition;
        Room lockedRoom = roomGrid[passwordLockedRoomPosition.x, passwordLockedRoomPosition.y];
        lockedRoom.LockRoom();

        foreach (Door door in lockedRoom.doors)
        {
            bool isHorizontal = Mathf.Abs(door.transform.localScale.x) > Mathf.Abs(door.transform.localScale.y);
            GameObject passwordDoorPrefab = isHorizontal ? horizontalPasswordDoorPrefab : verticalPasswordDoorPrefab;

            if (passwordDoorPrefab != null)
            {
                GameObject passwordDoor = Instantiate(passwordDoorPrefab, door.transform.position, Quaternion.identity);
                Door passwordDoorComponent = passwordDoor.GetComponent<Door>();

                if (passwordDoorComponent != null)
                {
                    passwordDoorComponent.connectedDoor = door.connectedDoor;
                    passwordDoorComponent.SetRoomGenerator(this);
                    passwordDoorComponent.SetConnectedRoomPosition(door.connectedRoomPosition);
                    passwordDoorComponent.LockWithPassword();

                    BoxCollider2D blockingCollider = passwordDoor.AddComponent<BoxCollider2D>();
                    blockingCollider.isTrigger = false; // Zamezuje průchodu
                    blockingCollider.size = new Vector2(0.8f, 0.8f); // Menší velikost

                    passwordDoorComponent.SetBlockingCollider(blockingCollider);
                }

                Destroy(door.gameObject);
            }
        }

        // Spawn EndNPC v této místnosti
        if (endNpcPrefab != null)
        {
            Vector2 spawnPosition = new Vector2(passwordLockedRoomPosition.x * roomSpacingX, passwordLockedRoomPosition.y * roomSpacingY);
            Instantiate(endNpcPrefab, spawnPosition, Quaternion.identity);
        }
    }





    void GenerateDungeonLayout()
    {
        roomGrid = new Room[gridWidth, gridHeight];
        List<Vector2Int> availablePositions = new List<Vector2Int>();

        // Přidej všechny možné pozice v gridu do seznamu
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                availablePositions.Add(new Vector2Int(x, y));
            }
        }

        // Procházej všechny buňky gridu a umísti místnost
        foreach (Vector2Int position in availablePositions)
        {
            SpawnRoom(position);
        }
    }


    void SpawnRoom(Vector2Int gridPosition)
    {
        if (availableRoomPrefabs.Count == 0)
        {
            Debug.LogError("Všechny místnosti již byly použity, ale stále jsou dostupné pozice v gridu.");
            return;
        }

        // Náhodně vyber a odeber prefab místnosti ze seznamu dostupných místností
        int randomRoomIndex = Random.Range(0, availableRoomPrefabs.Count);
        GameObject roomPrefab = availableRoomPrefabs[randomRoomIndex];
        availableRoomPrefabs.RemoveAt(randomRoomIndex);

        Vector2 spawnPosition = new Vector2(gridPosition.x * roomSpacingX, gridPosition.y * roomSpacingY);

        // Vytvoř místnost a přidej ji do gridu
        GameObject newRoomObject = Instantiate(roomPrefab, spawnPosition, Quaternion.identity);
        Room newRoom = newRoomObject.GetComponent<Room>();
        roomGrid[gridPosition.x, gridPosition.y] = newRoom;
    }

    void ConnectRooms()
    {
        List<Vector2Int> connectedRooms = new List<Vector2Int>();
        Queue<Vector2Int> roomQueue = new Queue<Vector2Int>();

        // Začněme s první místností v gridu
        Vector2Int startRoom = new Vector2Int(0, 0);
        connectedRooms.Add(startRoom);
        roomQueue.Enqueue(startRoom);

        while (roomQueue.Count > 0)
        {
            Vector2Int currentRoomPosition = roomQueue.Dequeue();
            Room currentRoom = roomGrid[currentRoomPosition.x, currentRoomPosition.y];

            List<Vector2Int> possibleDirections = new List<Vector2Int>()
        {
            new Vector2Int(1, 0),  // Right
            new Vector2Int(-1, 0), // Left
            new Vector2Int(0, 1),  // Up
            new Vector2Int(0, -1)  // Down
        };

            ShuffleList(possibleDirections);

            foreach (Vector2Int direction in possibleDirections)
            {
                Vector2Int neighborPosition = currentRoomPosition + direction;

                if (neighborPosition.x >= 0 && neighborPosition.x < gridWidth &&
                    neighborPosition.y >= 0 && neighborPosition.y < gridHeight &&
                    roomGrid[neighborPosition.x, neighborPosition.y] != null &&
                    !connectedRooms.Contains(neighborPosition))
                {
                    Room neighborRoom = roomGrid[neighborPosition.x, neighborPosition.y];
                    Door doorComponent = null;

                    // Získání velikosti dveří pro lepší umístění
                    float doorWidth = horizontalDoorPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
                    float doorHeight = verticalDoorPrefab.GetComponent<SpriteRenderer>().bounds.size.y;

                    if (direction == new Vector2Int(1, 0)) // Right (dveře mezi levou a pravou místností)
                    {
                        Vector2 doorPosition = new Vector2(
                            (currentRoom.transform.position.x + neighborRoom.transform.position.x) / 2 + 0.4f,
                            currentRoom.transform.position.y
                        );
                        GameObject door = Instantiate(horizontalDoorPrefab, doorPosition, Quaternion.identity);
                        doorComponent = door.GetComponent<Door>();
                    }
                    else if (direction == new Vector2Int(-1, 0)) // Left
                    {
                        Vector2 doorPosition = new Vector2(
                            (currentRoom.transform.position.x + neighborRoom.transform.position.x) / 2 + 0.4f,
                            currentRoom.transform.position.y
                        );
                        GameObject door = Instantiate(horizontalDoorPrefab, doorPosition, Quaternion.identity);
                        doorComponent = door.GetComponent<Door>();
                    }
                    else if (direction == new Vector2Int(0, 1)) // Up (dveře mezi horní a dolní místností)
                    {
                        Vector2 doorPosition = new Vector2(
                            currentRoom.transform.position.x + 0.4f,
                            (currentRoom.transform.position.y + neighborRoom.transform.position.y) / 2
                        );
                        GameObject door = Instantiate(verticalDoorPrefab, doorPosition, Quaternion.identity);
                        doorComponent = door.GetComponent<Door>();
                    }
                    else if (direction == new Vector2Int(0, -1)) // Down
                    {
                        Vector2 doorPosition = new Vector2(
                            currentRoom.transform.position.x + 0.4f,
                            (currentRoom.transform.position.y + neighborRoom.transform.position.y) / 2
                        );
                        GameObject door = Instantiate(verticalDoorPrefab, doorPosition, Quaternion.identity);
                        doorComponent = door.GetComponent<Door>();
                    }

                    if (doorComponent != null)
                    {
                        doorComponent.SetRoomGenerator(this);
                        doorComponent.SetConnectedRoomPosition(neighborPosition);

                        currentRoom.AddDoor(doorComponent);
                        neighborRoom.AddDoor(doorComponent);

                        // Správné odstranění stěn mezi propojenými místnostmi
                        if (direction == new Vector2Int(1, 0)) // Right
                        {
                            currentRoom.RemoveWallSegment("right");
                            neighborRoom.RemoveWallSegment("left");
                        }
                        else if (direction == new Vector2Int(-1, 0)) // Left
                        {
                            currentRoom.RemoveWallSegment("left");
                            neighborRoom.RemoveWallSegment("right");
                        }
                        else if (direction == new Vector2Int(0, 1)) // Up
                        {
                            currentRoom.RemoveWallSegment("top");
                            neighborRoom.RemoveWallSegment("bottom");
                        }
                        else if (direction == new Vector2Int(0, -1)) // Down
                        {
                            currentRoom.RemoveWallSegment("bottom");
                            neighborRoom.RemoveWallSegment("top");
                        }

                        connectedRooms.Add(neighborPosition);
                        roomQueue.Enqueue(neighborPosition);
                    }
                }
            }
        }
    }


    void LockRandomRoom()
    {
        Vector2Int randomPosition;
        do
        {
            randomPosition = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
        } while (randomPosition == new Vector2Int(0, 0) || randomPosition == passwordLockedRoomPosition);

        lockedRoomPosition = randomPosition;
        Room lockedRoom = roomGrid[lockedRoomPosition.x, lockedRoomPosition.y];
        lockedRoom.LockRoom();

        foreach (Door door in lockedRoom.doors)
        {
            bool isHorizontal = Mathf.Abs(door.transform.localScale.x) > Mathf.Abs(door.transform.localScale.y);
            GameObject lockedDoorPrefab = isHorizontal ? horizontalLockedDoorPrefab : verticalLockedDoorPrefab;

            if (lockedDoorPrefab != null)
            {
                GameObject lockedDoor = Instantiate(lockedDoorPrefab, door.transform.position, Quaternion.identity);
                Door lockedDoorComponent = lockedDoor.GetComponent<Door>();

                if (lockedDoorComponent != null)
                {
                    lockedDoorComponent.connectedDoor = door.connectedDoor;
                    lockedDoorComponent.SetRoomGenerator(this);
                    lockedDoorComponent.SetConnectedRoomPosition(door.connectedRoomPosition);
                    lockedDoorComponent.LockDoor();
                }

                Destroy(door.gameObject);
            }
        }
    }


    void PlaceKeyInAccessibleRoom()
    {
        List<Vector2Int> accessibleRooms = GetAccessibleRooms();

        if (accessibleRooms.Count == 0)
        {
            Debug.LogError("Nebyla nalezena žádná vhodná místnost pro klíč!");
            return;
        }

        // Vybereme náhodnou místnost ze seznamu dostupných místností
        Vector2Int keyPosition = accessibleRooms[Random.Range(0, accessibleRooms.Count)];

        Vector2 spawnPosition = new Vector2(keyPosition.x * roomSpacingX + 20f, keyPosition.y * roomSpacingY + 20f);
        keyObject = Instantiate(keyPrefab, spawnPosition, Quaternion.identity);
    }

    List<Vector2Int> GetAccessibleRooms()
    {
        List<Vector2Int> accessibleRooms = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(new Vector2Int(0, 0)); // Začínáme od startovní místnosti

        while (queue.Count > 0)
        {
            Vector2Int currentRoomPos = queue.Dequeue();

            if (visited.Contains(currentRoomPos))
                continue;

            visited.Add(currentRoomPos);
            Room currentRoom = roomGrid[currentRoomPos.x, currentRoomPos.y];

            // Pokud je místnost zamčená na heslo nebo klíč, ignorujeme ji
            if (currentRoomPos == passwordLockedRoomPosition || currentRoomPos == lockedRoomPosition)
                continue;

            accessibleRooms.Add(currentRoomPos);

            // Projdeme sousední místnosti
            foreach (Door door in currentRoom.doors)
            {
                if (door.isLocked || door.isPasswordLocked)
                    continue; // Pokud jsou dveře zamčené, přeskočíme je

                Vector2Int nextRoomPos = door.connectedRoomPosition;

                if (!visited.Contains(nextRoomPos) && roomGrid[nextRoomPos.x, nextRoomPos.y] != null)
                {
                    queue.Enqueue(nextRoomPos);
                }
            }
        }

        return accessibleRooms;
    }




    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void SpawnPlayer()
    {
        if (FindObjectOfType<Player>() != null)
        {
            Debug.Log("Hráč už existuje, nespawnujeme nového.");
            return;
        }

        if (roomGrid[0, 0] == null)
        {
            Debug.LogError("Startovní místnost [0,0] ještě neexistuje!");
            return;
        }

        Vector2 playerSpawnPosition = roomGrid[0, 0].transform.position + new Vector3(0, 1, 0);
        Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);

    }
    void SpawnStaticNPC()
    {
        if (staticNPCPrefab == null)
        {
            Debug.LogError("staticNPCPrefab není přiřazen v RoomGenerator!");
            return;
        }

        if (roomGrid[0, 0] == null)
        {
            Debug.LogError("Startovní místnost [0,0] ještě neexistuje!");
            return;
        }

        Vector2 npcSpawnPosition = roomGrid[0, 0].transform.position + new Vector3(30, 0, 0);
        Instantiate(staticNPCPrefab, npcSpawnPosition, Quaternion.identity);
    }




    public void CollectKey()
    {
        hasKey = true;
        UnlockLockedDoors();
    }


    public bool HasKey()
    {
        return hasKey;
    }

    void UnlockLockedDoors()
    {
        List<Door> doorsToUnlock = new List<Door>(lockedDoors); // Vytvoříme kopii seznamu

        foreach (Door door in doorsToUnlock)
        {
            if (door != null) // Zkontrolujeme, zda dveře stále existují
            {
                door.UnlockDoor(); // Odemkne dveře

                // Vyber správný prefab podle orientace dveří
                GameObject unlockedDoorPrefab = (Mathf.Abs(door.transform.localScale.x) < Mathf.Abs(door.transform.localScale.y))
                    ? horizontalOpenDoorPrefab
                    : verticalOpenDoorPrefab;

                if (unlockedDoorPrefab != null)
                {
                    Vector3 doorPosition = door.transform.position;
                    GameObject newDoor = Instantiate(unlockedDoorPrefab, doorPosition, Quaternion.identity);
                    Destroy(door.gameObject); // Smaže původní zamčené dveře
                }
                else
                {
                    Debug.LogError("Prefab pro odemčené dveře není přiřazen!");
                }
            }
        }
        lockedDoors.Clear(); // Vyčistí seznam zamčených dveří
    }


}