using UnityEngine;

public class Hint : MonoBehaviour
{
    private string hintText;
    private Color hintColor;

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
            inventory.AddHint(hintText, hintColor);
            Destroy(gameObject);
        }
    }
}
