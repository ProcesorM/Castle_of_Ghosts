using UnityEngine;

public class HintGiver : MonoBehaviour
{
    private bool questCompleted = false;
    public string hintText;
    public GameObject hintItemPrefab; // Prefab předmětu nápovědy
    private bool hintDropped = false;

    public void SetHintText(string text)
    {
        hintText = text;
    }

    public void CompleteQuest()
    {
        questCompleted = true;
        GiveHint(); // Zavolá metodu, aby se nápověda spawnula
    }
    public void GiveHint()
    {
        if (!questCompleted || hintDropped)
        {
            return; // Pokud quest není dokončen nebo už hint spadl, nic se nestane
        }

        // Vytvoření hintu na místě NPC
        Vector2 spawnPosition = new Vector2(transform.position.x + 1, transform.position.y);
        GameObject hintItem = Instantiate(hintItemPrefab, spawnPosition, Quaternion.identity);

        // Nastavení textu nápovědy podle NPC
        Hint hintComponent = hintItem.GetComponent<Hint>();
        if (hintComponent != null)
        {
            hintComponent.SetHintText("Toto je nápověda od ducha!");
        }

        hintDropped = true; // Zabráníme dalšímu spawnování
    }
}
