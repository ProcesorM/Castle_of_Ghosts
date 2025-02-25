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

        // Nastavení stylu nadpisu
        questLogText.text = "<b><size=25><color=yellow>Questy</color></size></b>\n\n";

        foreach (Quest quest in questManager.activeQuests)
        {
            // Nastavení stylu názvu questu
            questLogText.text += $"<b><size=20><color=orange>{quest.questName}</color></size></b>\n";
            // Nastavení stylu popisu questu
            questLogText.text += $"<size=18><color=white>- {quest.questDescription}</color></size>\n\n";
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
