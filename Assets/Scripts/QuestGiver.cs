using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    public Quest quest; // Přiřazený quest
    public string hintReward; // Nápověda jako odměna za quest

    private bool isCompleted = false;
    private QuestManager questManager;
    private Inventory inventory;

    void Start()
    {
        questManager = FindObjectOfType<QuestManager>();
        inventory = FindObjectOfType<Inventory>();
    }

    public void GiveQuest()
    {
        if (!isCompleted && questManager != null)
        {
            questManager.AddQuest(quest);
        }
    }

    public void CompleteQuest()
    {
        if (!isCompleted && questManager != null && inventory != null)
        {
            if (questManager.activeQuests.Contains(quest) && inventory.HasItem(quest.requiredItemName))

            {
                Debug.Log("Quest splněn: " + quest.questName);
                questManager.CompleteQuest(quest);
                inventory.RemoveItemByName(quest.requiredItemName);
                // Odebrání předmětu

                // Přidání hintu do inventáře
                if (!string.IsNullOrEmpty(hintReward))
                {
                    inventory.AddHint(hintReward, Color.yellow); // Nápověda má žlutou barvu
                }

                isCompleted = true;
            }
        }
    }
}

