using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public List<Quest> activeQuests = new List<Quest>();  // Seznam aktivních úkolů
    public List<Quest> completedQuests = new List<Quest>();  // Seznam dokončených úkolů

    public delegate void QuestUpdateHandler();
    public event QuestUpdateHandler OnQuestUpdated; // Event pro aktualizaci quest logu

    public void AddQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest) && !quest.isCompleted)
        {
            activeQuests.Add(quest);
            Debug.Log("Přidán nový úkol: " + quest.questName);

            OnQuestUpdated?.Invoke();
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

            OnQuestUpdated?.Invoke();

            // Hledáme NPC, které zadalo quest a zkontrolujeme, zda má HintGiver
            HintGiver hintGiver = quest.questGiver.GetComponent<HintGiver>();
            if (hintGiver != null)
            {
                Debug.Log("NPC má HintGiver, spawnuji nápovědu...");
                hintGiver.CompleteQuest(); // Zavolá metodu, která spawne hint
            }
            else
            {
                Debug.Log("NPC nemá HintGiver, žádná nápověda nebude spawnuta.");
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
