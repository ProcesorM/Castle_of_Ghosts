using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 5f; // Rychlost pohybu NPC
    private List<Vector2Int> npcPath; // Aktuální cesta, kterou NPC bude sledovat
    private int currentWaypointIndex = 0;
    private RoomGenerator roomGenerator;
    private bool movingForward = true; // Určuje směr pohybu (vpřed nebo zpět)

    private bool isMoving = true; // Určuje, zda se NPC pohybuje

    void Start()
    {
        roomGenerator = FindObjectOfType<RoomGenerator>();

        if (roomGenerator != null && roomGenerator.npcPaths.Count > 0)
        {
            // Náhodně vyber jednu z vygenerovaných cest pro NPC
            npcPath = roomGenerator.npcPaths[Random.Range(0, roomGenerator.npcPaths.Count)];
            currentWaypointIndex = 0;
        }
    }

    void Update()
    {
        if (npcPath != null && npcPath.Count > 0)
        {
            MoveToWaypoint();
        }
    }

    public void ToggleMovement()
    {
        isMoving = !isMoving; // Přepnutí stavu pohybu
        Debug.Log("NPC pohyb: " + (isMoving ? "Pohyb povolen" : "Pohyb zastaven"));

        // Pokud NPC má být zastaveno, nastavíme rychlost na 0, aby se pohyb zastavil úplně
        if (!isMoving)
        {
            speed = 0f;
        }
        else
        {
            speed = 5f;
        }
    }

    public void SetPath(List<Vector2Int> path)
    {
        npcPath = path;
        currentWaypointIndex = 0;
        movingForward = true;
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

            // Debugování stavu isMoving
            //Debug.Log("isMoving: " + isMoving);

            // Pohyb pouze pokud je isMoving true
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
