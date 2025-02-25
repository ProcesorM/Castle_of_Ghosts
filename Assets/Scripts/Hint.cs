using UnityEngine;

public class Hint : MonoBehaviour
{
    public string hintText;
    private Color hintColor;
    public Sprite hintSprite; // Přidej tohle

    private void Start()
    {
        // Ulož barvu z SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            hintColor = spriteRenderer.color;
        }
    }

    public void SetHintText(string text)
    {
        hintText = text;
    }

    public string GetHintText()
    {
        return hintText;
    }

    public Color GetHintColor()
    {
        return hintColor;
    }

    public void CollectHint(Inventory inventory)
    {
        if (inventory != null)
        {
            inventory.AddHint(this);
            Destroy(gameObject);
        }
    }

    public void SetSprite(Sprite sprite)
    {
        hintSprite = sprite;
    }

    public Sprite GetSprite()
    {
        return hintSprite;
    }
}
