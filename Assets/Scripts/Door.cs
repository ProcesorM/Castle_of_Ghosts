﻿

using UnityEngine;

public class Door : MonoBehaviour
{
    public Door connectedDoor; // Dveře, na které jsou tyto dveře propojeny
    public bool isLocked = false; // Určuje, zda jsou dveře uzamčeny
    private RoomGenerator roomGenerator;

    public bool isPasswordLocked = false; // Určuje, zda jsou dveře zamčeny na heslo
    private bool isPlayerNearby = false;

    public Vector2Int connectedRoomPosition; // Pozice místnosti, na kterou tyto dveře vedou

    private BoxCollider2D blockingCollider;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (isPasswordLocked && roomGenerator.passwordPanel != null)
            {
                roomGenerator.passwordPanel.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (isPasswordLocked)
            {
                // Zobraz UI panel pro zadání hesla
                roomGenerator.passwordPanel.SetActive(true);
                roomGenerator.passwordPanel.GetComponent<PasswordPanel>().SetDoorToUnlock(this);
            }
            else if (isLocked && roomGenerator != null && !roomGenerator.HasKey())
            {
                Debug.Log("Dveře jsou zamčené!");
            }
            else if (connectedDoor != null && !isLocked && !isPasswordLocked)
            {
                // Teleport hráče ke spojeným dveřím
                other.transform.position = connectedDoor.transform.position + new Vector3(0, 1, 0);
            }
        }
    }

    public void LockWithPassword()
    {
        isPasswordLocked = true;
        GetComponent<BoxCollider2D>().isTrigger = true; // Dveře jsou uzamčeny na heslo, ale hráč s nimi může interagovat
        GetComponent<SpriteRenderer>().color = Color.blue; // Nastaví barvu dveří na modrou
    }

    public void LockDoor()
    {
        isLocked = true;
        GetComponent<BoxCollider2D>().isTrigger = false;
        GetComponent<SpriteRenderer>().color = Color.yellow;

        if (!roomGenerator.lockedDoors.Contains(this))
        {
            roomGenerator.lockedDoors.Add(this);
        }
    }

    public void UnlockDoor()
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("Pokus o odemčení dveří, které už neexistují!");
            return;
        }

        isLocked = false;
        isPasswordLocked = false;
        GetComponent<BoxCollider2D>().isTrigger = true;

        if (blockingCollider != null)
        {
            Destroy(blockingCollider);
            blockingCollider = null;
        }
        Debug.Log("Dveře odemčeny: " + gameObject.name);
    }



    public void SetRoomGenerator(RoomGenerator generator)
    {
        roomGenerator = generator;
    }
    public void SetGreenColor()
    {
        GetComponent<SpriteRenderer>().color = Color.green;
    }
    public void SetConnectedRoomPosition(Vector2Int position)
    {
        connectedRoomPosition = position;
    }
    // Přidána metoda pro získání propojené místnosti
    public Room GetConnectedRoom(Room currentRoom)
    {
        if (connectedDoor != null)
        {
            return connectedDoor.transform.parent.GetComponent<Room>();
        }
        return null;
    }
    public void SetBlockingCollider(BoxCollider2D collider)
    {
        blockingCollider = collider;
    }
}