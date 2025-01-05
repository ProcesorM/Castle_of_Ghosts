using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public List<Quest> activeQuests = new List<Quest>();  // Seznam aktivních úkolů
    public List<Quest> completedQuests = new List<Quest>();  // Seznam dokončených úkolů

    public void AddQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest) && !quest.isCompleted)
        {
            activeQuests.Add(quest);
            Debug.Log("Přidán nový úkol: " + quest.questName);

            FindObjectOfType<QuestLog>()?.UpdateQuestLog();
        }
    }

    public void CompleteQuest(Quest quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            completedQuests.Add(quest);
            quest.isCompleted = true;
            Debug.Log("Úkol dokončen: " + quest.questName);

            QuestLog questLog = FindObjectOfType<QuestLog>();
            if (questLog != null)
            {
                questLog.UpdateQuestLog();
            }
        }

    }
    public void TryCompleteQuest(Quest quest, Inventory inventory)
    {
        if (quest == null)
        {
            Debug.LogError("Úkol je null!");
            return;
        }

        if (inventory.HasItem(quest.requiredItemName))
        {
            inventory.RemoveItemByName(quest.requiredItemName);
            CompleteQuest(quest);
            Debug.Log("Úkol dokončen: " + quest.questName);
        }
        else
        {
            Debug.Log("Hráč nemá požadovaný předmět: " + quest.requiredItemName);
        }
    }

}
