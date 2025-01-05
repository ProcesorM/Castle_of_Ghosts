using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public string npcName = "Neznámé NPC"; // Jméno NPC
    public Quest assignedQuest;           // Přiřazený úkol
    public List<DialogueLine> defaultDialogue; // První dialog
    public List<DialogueLine> questDialogue;   // Druhý dialog
    public List<DialogueLine> completedDialogue; // Třetí dialog

    private bool isPlayerInRange = false; // Je hráč v dosahu?
    private DialogueManager dialogueManager;
    private QuestManager questManager;
    private NPCMovement npcMovement;

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        questManager = FindObjectOfType<QuestManager>();
        npcMovement = GetComponent<NPCMovement>();
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Kontrola stavu úkolu a spuštění odpovídajícího dialogu
            if (assignedQuest != null)
            {
                if (assignedQuest.isCompleted)
                {
                    StartCompletedDialogue();
                }
                else if (questManager.activeQuests.Contains(assignedQuest))
                {
                    StartQuestDialogue();
                }
                else
                {
                    StartDefaultDialogue();
                }
            }
            else
            {
                StartDefaultDialogue();
            }
        }
    }

    void StartDefaultDialogue()
    {
        dialogueManager.StartDialogue(defaultDialogue, npcMovement);
        if (assignedQuest != null)
        {
            questManager.AddQuest(assignedQuest); // Přidá quest při potvrzení
        }
    }

    void StartQuestDialogue()
    {
        dialogueManager.StartDialogue(questDialogue, npcMovement, () =>
        {
            if (assignedQuest != null && questManager.activeQuests.Contains(assignedQuest))
            {
                Inventory playerInventory = FindObjectOfType<Player>().GetComponent<Inventory>();
                if (playerInventory.HasItem(assignedQuest.requiredItemName))
                {
                    // Předmět je v inventáři
                    playerInventory.RemoveItemByName(assignedQuest.requiredItemName);
                    assignedQuest.isCompleted = true;
                    questManager.CompleteQuest(assignedQuest);

                    Debug.Log($"Úkol dokončen: {assignedQuest.questName}");
                    dialogueManager.StartDialogue(completedDialogue, npcMovement);
                }
                else
                {
                    // Předmět není v inventáři
                    dialogueManager.DisplayDynamicResponse(npcName, "Nemáš požadovaný předmět.");
                }
            }
        });
    }

    void StartCompletedDialogue()
    {
        dialogueManager.StartDialogue(completedDialogue, npcMovement);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = false;
    }
}
