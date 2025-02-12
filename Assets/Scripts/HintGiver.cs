using UnityEngine;

public class HintGiver : MonoBehaviour
{
    private bool questCompleted = false;
    public string hintText;
    public GameObject hintItemPrefab; // Prefab předmětu nápovědy
    private bool hintDropped = false;
    private bool hintGiven = false;

    public void SetHintText(string text)
    {
        hintText = text;
    }

    public void CompleteQuest()
    {
        questCompleted = true;
        GiveHint(hintText); // Zavolá metodu, aby se nápověda spawnula
    }
    public void GiveHint(string hintText)
    {
        if (!hintGiven)
        {
            hintGiven = true;

            // Spawn hintu jako fyzického objektu
            Vector2 spawnPosition = new Vector2(transform.position.x + 2, transform.position.y); // Posun vedle NPC
            GameObject hintItem = Instantiate(hintItemPrefab, spawnPosition, Quaternion.identity);

            // Nastav text nápovědy
            Hint hintComponent = hintItem.GetComponent<Hint>();
            if (hintComponent != null)
            {
                hintComponent.SetHintText(hintText);
                Debug.Log("Spawnut hint s textem: " + hintText);
            }
            else
            {
                Debug.LogError("HintItem prefab nemá komponentu HintItem!");
            }
        }
    }
}
