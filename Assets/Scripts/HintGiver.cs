using UnityEngine;

public class HintGiver : MonoBehaviour
{
    private bool questCompleted = false;
    public string hintText;
    public GameObject hintItemPrefab;
    private bool hintDropped = false;
    private bool hintGiven = false;

    public void SetHintText(string text)
    {
        hintText = text;
    }

    public void CompleteQuest()
    {
        questCompleted = true;
        GiveHint(hintText);
    }

    public void GiveHint(string hintText)
    {
        if (!hintGiven)
        {
            hintGiven = true;

            Vector2 spawnPosition = new Vector2(transform.position.x, transform.position.y);
            GameObject hintItem = Instantiate(hintItemPrefab, spawnPosition, Quaternion.identity);

            // Nastavení textu nápovědy
            Hint hintComponent = hintItem.GetComponent<Hint>();
            if (hintComponent != null)
            {
                hintComponent.SetHintText(hintText);
                Debug.Log("Spawnut hint s textem: " + hintText);
            }
            else
            {
                Debug.LogError("Prefab hintItem nemá komponentu Hint!");
            }

            // Ověř a nastav sprite (volitelně, pokud chybí)
            SpriteRenderer renderer = hintItem.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = hintItem.AddComponent<SpriteRenderer>();
            }

            if (renderer.sprite == null)
            {
                Debug.LogWarning("HintItem prefab nemá nastavený sprite! Nastav jej v editoru.");
            }
        }
    }
}
