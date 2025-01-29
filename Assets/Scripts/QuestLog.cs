using UnityEngine;
using UnityEngine.UI;

public class QuestLog : MonoBehaviour
{
    public GameObject questLogPanel;
    public Text questLogText; // Textové pole pro zobrazení questů
    private QuestManager questManager;

    void Start()
    {
        questManager = FindObjectOfType<QuestManager>();
        if (questManager == null)
        {
            Debug.LogError("QuestManager nebyl nalezen ve scéně!");
            return;
        }
        questLogPanel.SetActive(false);

        UpdateQuestLog(); // Aktualizace při spuštění
    }
    public void ToggleQuestLog()
    {
        bool isActive = questLogPanel.activeSelf;
        questLogPanel.SetActive(!isActive); // Přepíná viditelnost panelu
        if (!isActive)
        {
            UpdateQuestLog(); // Aktualizuje seznam questů při otevření
        }
    }

    public void UpdateQuestLog()
    {
        if (questManager == null)
        {
            Debug.LogError("QuestManager není dostupný!");
            return;
        }

        questLogText.text = "Aktivní úkoly:\n";
        foreach (Quest quest in questManager.activeQuests)
        {
            questLogText.text += "- " + quest.questName + "\n";
        }
    }


    private void OnDestroy()
    {
        if (questManager != null)
        {
            questManager.OnQuestUpdated -= UpdateQuestLog; // Odstraníme event listener
        }
    }
}
