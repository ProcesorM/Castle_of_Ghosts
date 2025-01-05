using UnityEngine;
using UnityEngine.UI;

public class QuestLog : MonoBehaviour
{
    public Text questLogText; // Textové pole pro zobrazení questů
    private QuestManager questManager;

    void Start()
    {
        questManager = FindObjectOfType<QuestManager>();
        UpdateQuestLog();
    }

    public void UpdateQuestLog()
    {
        questLogText.text = "Aktivní úkoly:\n";
        foreach (Quest quest in questManager.activeQuests)
        {
            questLogText.text += "- " + quest.questName + "\n";
        }
    }
}
