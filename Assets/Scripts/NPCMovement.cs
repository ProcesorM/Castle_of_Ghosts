﻿using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 5f; // Rychlost pohybu NPC
    private List<Vector2Int> npcPath; // Aktuální cesta, kterou NPC bude sledovat
    private int currentWaypointIndex = 0;
    private RoomGenerator roomGenerator;
    private bool movingForward = true; // Určuje směr pohybu (vpřed nebo zpět)

    private bool isMoving = true; // Určuje, zda se NPC pohybuje
    private float originalSpeed; // Původní rychlost NPC

    void Start()
    {
        roomGenerator = FindObjectOfType<RoomGenerator>();

        if (roomGenerator != null && roomGenerator.npcPaths.Count > 0)
        {
            // Náhodně vyber jednu z vygenerovaných cest pro NPC
            npcPath = roomGenerator.npcPaths[Random.Range(0, roomGenerator.npcPaths.Count)];
            currentWaypointIndex = 0;
        }
        originalSpeed = speed;
    }

    void Update()
    {
        if (npcPath != null && npcPath.Count > 0)
        {
            MoveToWaypoint();
        }
    }

    public void StopMovement()
    {
        isMoving = false;
        originalSpeed = speed; // Uložíme aktuální rychlost
        speed = 0f; // Zastavíme NPC
    }

    public void ResumeMovement()
    {
        isMoving = true;
        speed = originalSpeed; // Vrátíme rychlost zpět na původní hodnotu
    }

    public void SetPath(List<Vector2Int> path)
    {
        npcPath = path;
        currentWaypointIndex = 0;
        movingForward = true;

        // **Kontrola, zda `roomGenerator` není null**
        if (roomGenerator == null)
        {
            roomGenerator = FindObjectOfType<RoomGenerator>(); // Pokusíme se ho najít v hierarchii
            if (roomGenerator == null)
            {
                Debug.LogError("roomGenerator je stále null v NPCMovement!");
                return;
            }
        }

        // **Kontrola, zda `npcPath` není null nebo prázdná**
        if (npcPath == null || npcPath.Count == 0)
        {
            Debug.LogError("NPC nemá přidělenou cestu! Spawn proběhl špatně.");
            return;
        }

        // 🚀 Oprava: NPC se rovnou přesune na svůj první waypoint
        Vector3 startPosition = new Vector3(npcPath[0].x * roomGenerator.roomSpacingX, npcPath[0].y * roomGenerator.roomSpacingY, 0);
        transform.position = startPosition;
    }



    private void MoveToWaypoint()
    {
        if (npcPath == null || npcPath.Count == 0)
        {
            return;
        }

        if (currentWaypointIndex >= 0 && currentWaypointIndex < npcPath.Count)
        {
            Vector2Int targetPosition = npcPath[currentWaypointIndex];
            Vector3 targetWorldPosition = new Vector3(targetPosition.x * roomGenerator.roomSpacingX, targetPosition.y * roomGenerator.roomSpacingY, 0);
            Vector3 direction = (targetWorldPosition - transform.position).normalized;
            if (direction.x > 0)
            {
                transform.localScale = new Vector3(20, 20, 1); // NPC směřuje doprava
            }
            else if (direction.x < 0)
            {
                transform.localScale = new Vector3(-20, 20, 1); // NPC směřuje doleva
            }

            if (isMoving)
            {
                transform.position += direction * speed * Time.deltaTime;
            }

            // Pokud NPC dosáhne aktuálního waypointu, přejde na další waypoint nebo změní směr
            if (Vector3.Distance(transform.position, targetWorldPosition) < 0.1f)
            {
                if (movingForward)
                {
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= npcPath.Count)
                    {
                        movingForward = false; // Došli jsme na konec cesty, otočíme směr
                        currentWaypointIndex -= 2; // Přesuneme se zpět na předchozí waypoint
                    }
                }
                else
                {
                    currentWaypointIndex--;
                    if (currentWaypointIndex < 0)
                    {
                        movingForward = true; // Došli jsme na začátek cesty, otočíme směr zpět
                        currentWaypointIndex = 1; // Přesuneme se na druhý waypoint
                    }
                }
            }
        }
    }
}
