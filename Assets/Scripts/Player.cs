using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f; // Rychlost pohybu hráče
    private Rigidbody2D rb;
    private Vector2 movement;
    private Inventory inventory;

    private NPCMovement nearbyNPC; // Odkaz na blízké NPC

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        if (FindObjectsOfType<Player>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventář není připojen k hráči!");
        }
    }

    void Update()
    {
        movement.x = 0;
        movement.y = 0;

        if (Input.GetKey(KeyCode.W))
        {
            movement.y = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            movement.y = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            movement.x = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            movement.x = 1;
        }

        if (Input.GetKeyDown(KeyCode.Space) && nearbyNPC != null)
        {
            nearbyNPC.ToggleMovement(); // Zastaví nebo obnoví pohyb NPC
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            if (item != null)
            {
                // Vytvoří kopii objektu pouze jako data
                GameObject itemCopy = new GameObject(item.itemName);
                itemCopy.AddComponent<Item>().itemName = item.itemName;

                // Přidání a nastavení SpriteRenderer
                SpriteRenderer originalSpriteRenderer = collision.GetComponent<SpriteRenderer>();
                if (originalSpriteRenderer != null)
                {
                    SpriteRenderer newSpriteRenderer = itemCopy.AddComponent<SpriteRenderer>();
                    newSpriteRenderer.sprite = originalSpriteRenderer.sprite;
                    newSpriteRenderer.color = originalSpriteRenderer.color;
                }

                inventory.AddItem(itemCopy); // Přidá kopii do inventáře
                Destroy(collision.gameObject); // Zničí původní objekt
            }
        }
        else if (collision.CompareTag("Hint"))
        {
            Hint hint = collision.GetComponent<Hint>();
            if (hint != null)
            {
                hint.CollectHint(inventory);
            }
        }
        else if (collision.CompareTag("NPC"))
        {
            NPCMovement npcMovement = collision.GetComponent<NPCMovement>();
            if (npcMovement != null)
            {
                nearbyNPC = npcMovement; // Uloží referenci na blízké NPC
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("NPC"))
        {
            nearbyNPC = null;
        }
    }
}
