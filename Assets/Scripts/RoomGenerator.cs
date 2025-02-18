﻿using System.Collections;
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
        new string[] { "vítr", "bez hlasu vrčí", "bez těla běží", "bez křídel klapá", "bez zubů kousá" },
        new string[] { "slunce", "všude mě vidíš ve dne", "nevidíš mě v noci nikdy", "lezu po nebi jak plaz", "po horách lezu jak šplhavý pták." },
        new string[] { "mnich", "Můj život je zasvěcen modlitbě a tichu.", "Nosím prostý hábit a má duše hledá klid.", "Žiji v klášteře, stranou od světských věcí.", "Píši a kopíruji knihy, aby se nezapomněla moudrost." },
        new string[] { "kat", "Nosím černou kápi, jsem služebníkem spravedlnosti.", "Má práce je těžká, mou zbraň je sekera.", "Lidé mě znají a přesto se mi vyhýbají.", "Jsem poslední tvář, kterou provinilec spatří." },
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
        availableRoomPrefabs = new List<GameObject>(roomPrefabs); // Vytvoří kopii seznamu prefabů
        GenerateDungeonLayout();
        ConnectRooms();
        LockRandomRoom();
        PlaceKeyInAccessibleRoom();
        SpawnPlayer();

        LockPasswordRoom();

        int randomIndex = Random.Range(0, puzzles.Count);
        selectedPuzzle = puzzles[randomIndex];
        correctPassword = selectedPuzzle[0];
        //PlaceHints();

        GenerateHints();
        GenerateNPCPaths();
        SpawnNPCs();
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
        int randomIndex = Random.Range(0, puzzles.Count);
        selectedPuzzle = puzzles[randomIndex];

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



    //void PlaceHints()
    //{
    //    List<Vector2Int> availablePositions = new List<Vector2Int>();

    //    // Projdi všechny pozice v gridu a přidej je do seznamu dostupných pozic, kromě zamčených místností
    //    for (int x = 0; x < gridWidth; x++)
    //    {
    //        for (int y = 0; y < gridHeight; y++)
    //        {
    //            if (roomGrid[x, y] != null && !(new Vector2Int(x, y) == lockedRoomPosition) && !(new Vector2Int(x, y) == passwordLockedRoomPosition))
    //            {
    //                availablePositions.Add(new Vector2Int(x, y));
    //            }
    //        }
    //    }

    //    // Umísti všechny 4 nápovědy
    //    for (int i = 1; i <= 4; i++)
    //    {
    //        if (availablePositions.Count == 0)
    //        {
    //            Debug.LogError("Není dostatek volných místností pro umístění nápověd.");
    //            return;
    //        }

    //        // Vyber náhodnou pozici pro nápovědu
    //        int randomPositionIndex = Random.Range(0, availablePositions.Count);
    //        Vector2Int hintPosition = availablePositions[randomPositionIndex];
    //        availablePositions.RemoveAt(randomPositionIndex);

    //        Vector2 spawnPosition = new Vector2(hintPosition.x * roomSpacingX, hintPosition.y * roomSpacingY);

    //        // Vytvoř nápovědu a nastav její text
    //        GameObject hintObject = Instantiate(hintPrefab, spawnPosition, Quaternion.identity);
    //        Hint hintComponent = hintObject.GetComponent<Hint>();
    //        if (hintComponent != null)
    //        {
    //            hintComponent.SetHintText(selectedPuzzle[i]);
    //        }
    //    }
    //}

    void LockPasswordRoom()
    {
        // Najdi místnost, která bude zamčena na heslo a má pouze jedny dveře
        Vector2Int randomPosition;
        do
        {
            randomPosition = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
        } while (randomPosition == new Vector2Int(0, 0) || randomPosition == lockedRoomPosition || roomGrid[randomPosition.x, randomPosition.y].doors.Count != 1);

        passwordLockedRoomPosition = randomPosition;
        Room lockedRoom = roomGrid[passwordLockedRoomPosition.x, passwordLockedRoomPosition.y];
        lockedRoom.LockRoom();

        // Najdi dveře v této místnosti a zamkni je heslem
        foreach (Door door in lockedRoom.doors)
        {
            bool isHorizontal = Mathf.Abs(door.transform.localScale.x) > Mathf.Abs(door.transform.localScale.y);

            // Vybereme správný prefab pro dveře na heslo
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

                    // Přidáme blokující Collider na nové dveře
                    BoxCollider2D blockingCollider = passwordDoor.AddComponent<BoxCollider2D>();
                    blockingCollider.isTrigger = false; // Tento collider blokuje průchod
                    blockingCollider.size = new Vector2(0.8f, 0.8f); // Zmenšená velikost
                    passwordDoorComponent.SetBlockingCollider(blockingCollider);
                }

                Destroy(door.gameObject); // Odstraníme staré dveře
            }
            else
            {
                Debug.LogError("Prefab pro dveře na heslo není přiřazen!");
            }
        }

        // Spawn EndNPC v zamčené místnosti
        if (endNpcPrefab != null)
        {
            Vector2 spawnPosition = new Vector2(passwordLockedRoomPosition.x * roomSpacingX, passwordLockedRoomPosition.y * roomSpacingY);
            GameObject endNpcObject = Instantiate(endNpcPrefab, spawnPosition, Quaternion.identity);

            // Najdi a nastav správného DialogueManager
            EndNPC endNpcScript = endNpcObject.GetComponent<EndNPC>();
            DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();

            if (endNpcScript != null && dialogueManager != null)
            {
                endNpcScript.SetDialogueManager(dialogueManager);
            }
            else
            {
                Debug.LogError("Chyba při přiřazování DialogueManager k EndNPC!");
            }
        }
        else
        {
            Debug.LogError("EndNPC Prefab není přiřazen v RoomGenerator!");
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

                    // Přidej dveře mezi currentRoom a neighborRoom
                    Door doorComponent = null;

                    if (direction == new Vector2Int(1, 0)) // Right
                    {
                        // Přidej dveře vpravo a odstraní příslušné stěny
                        Vector2 doorPosition = new Vector2(currentRoom.transform.position.x + roomSpacingX / 2, currentRoom.transform.position.y);
                        GameObject door = Instantiate(horizontalDoorPrefab, doorPosition, Quaternion.identity);
                        doorComponent = door.GetComponent<Door>();
                        doorComponent.SetRoomGenerator(this);
                        doorComponent.SetConnectedRoomPosition(neighborPosition);

                        currentRoom.AddDoor(doorComponent);
                        neighborRoom.AddDoor(doorComponent);

                        // Odstraň pravou stěnu aktuální místnosti a levou stěnu sousední místnosti
                        currentRoom.RemoveWallSegment("right");
                        neighborRoom.RemoveWallSegment("left");
                    }
                    else if (direction == new Vector2Int(-1, 0)) // Left
                    {
                        // Přidej dveře vlevo a odstraní příslušné stěny
                        Vector2 doorPosition = new Vector2(neighborRoom.transform.position.x + roomSpacingX / 2, neighborRoom.transform.position.y);
                        GameObject door = Instantiate(horizontalDoorPrefab, doorPosition, Quaternion.identity);
                        doorComponent = door.GetComponent<Door>();
                        doorComponent.SetRoomGenerator(this);
                        doorComponent.SetConnectedRoomPosition(currentRoomPosition);

                        currentRoom.AddDoor(doorComponent);
                        neighborRoom.AddDoor(doorComponent);

                        // Odstraň levou stěnu aktuální místnosti a pravou stěnu sousední místnosti
                        currentRoom.RemoveWallSegment("left");
                        neighborRoom.RemoveWallSegment("right");
                    }
                    else if (direction == new Vector2Int(0, 1)) // Up
                    {
                        // Přidej dveře nahoře a odstraní příslušné stěny
                        Vector2 doorPosition = new Vector2(currentRoom.transform.position.x, currentRoom.transform.position.y + roomSpacingY / 2);
                        GameObject door = Instantiate(verticalDoorPrefab, doorPosition, Quaternion.identity);
                        doorComponent = door.GetComponent<Door>();
                        doorComponent.SetRoomGenerator(this);
                        doorComponent.SetConnectedRoomPosition(neighborPosition);

                        currentRoom.AddDoor(doorComponent);
                        neighborRoom.AddDoor(doorComponent);

                        // Odstraň horní stěnu aktuální místnosti a spodní stěnu sousední místnosti
                        currentRoom.RemoveWallSegment("top");
                        neighborRoom.RemoveWallSegment("bottom");
                    }
                    else if (direction == new Vector2Int(0, -1)) // Down
                    {
                        // Přidej dveře dole a odstraní příslušné stěny
                        Vector2 doorPosition = new Vector2(neighborRoom.transform.position.x, neighborRoom.transform.position.y + roomSpacingY / 2);
                        GameObject door = Instantiate(verticalDoorPrefab, doorPosition, Quaternion.identity);
                        doorComponent = door.GetComponent<Door>();
                        doorComponent.SetRoomGenerator(this);
                        doorComponent.SetConnectedRoomPosition(currentRoomPosition);

                        currentRoom.AddDoor(doorComponent);
                        neighborRoom.AddDoor(doorComponent);

                        // Odstraň spodní stěnu aktuální místnosti a horní stěnu sousední místnosti
                        currentRoom.RemoveWallSegment("bottom");
                        neighborRoom.RemoveWallSegment("top");
                    }

                    if (doorComponent != null)
                    {
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
        } while (randomPosition == new Vector2Int(0, 0));

        lockedRoomPosition = randomPosition;
        Room lockedRoom = roomGrid[lockedRoomPosition.x, lockedRoomPosition.y];
        lockedRoom.LockRoom();

        foreach (Door door in lockedRoom.doors)
        {
            bool isHorizontal = Mathf.Abs(door.transform.localScale.x) > Mathf.Abs(door.transform.localScale.y);

            // Vybereme správný prefab podle směru dveří
            GameObject lockedDoorPrefab = isHorizontal ? horizontalLockedDoorPrefab : verticalLockedDoorPrefab;

            // Zaměníme dveře za uzamčené dveře
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

                Destroy(door.gameObject); // Odstraníme staré dveře
            }
        }
    }


    void PlaceKeyInAccessibleRoom()
    {
        // Najdi náhodnou místnost, která není zamčená a je přístupná ze startu
        Vector2Int keyPosition;
        do
        {
            keyPosition = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
        } while (keyPosition == lockedRoomPosition || keyPosition == new Vector2Int(0, 0));

        Vector2 spawnPosition = new Vector2(keyPosition.x * roomSpacingX, keyPosition.y * roomSpacingY);
        keyObject = Instantiate(keyPrefab, spawnPosition, Quaternion.identity);
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
        // Umístí hráče do první vygenerované místnosti (na pozici [0, 0])
        if (roomGrid[0, 0] != null)
        {
            Vector2 playerSpawnPosition = roomGrid[0, 0].transform.position + new Vector3(0, 1, 0);
            Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);

            // Spawn statického NPC v první místnosti
            Vector2 npcSpawnPosition = roomGrid[0, 0].transform.position + new Vector3(30, 0, 0); // Posun od hráče
            Instantiate(staticNPCPrefab, npcSpawnPosition, Quaternion.identity);
        }
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