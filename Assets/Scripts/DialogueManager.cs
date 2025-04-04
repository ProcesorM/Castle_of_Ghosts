﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public Text speakerNameText;
    public Text dialogueText;
    public GameObject dialoguePanel;
    public Transform choicesPanel; // Panel pro hráčovy volby
    public Button choiceButtonPrefab; // Prefab pro tlačítka voleb

    private List<DialogueLine> dialogueLines;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private NPCMovement currentNPC; // Pohyb NPC
    private Player playerScript;

    public GameObject secondaryPanel; // Druhý panel, který chceme zobrazit

    private System.Action onDialogueEndCallback;
    private int lastChoiceIndex = 0;  // Ukládá poslední volbu hráče

    void Start()
    {
        dialoguePanel.SetActive(false);
        playerScript = FindObjectOfType<Player>(); // Najdeme hráče
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (dialogueLines[currentLineIndex].playerChoices.Count > 0)
            {
                return; // Čekáme na volbu
            }
            DisplayNextLine();
        }
    }

    public void StartDialogue(List<DialogueLine> lines, NPCMovement npc, System.Action onEndCallback = null)
    {
        dialogueLines = lines;
        currentLineIndex = 0;
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        secondaryPanel.SetActive(true);
        currentNPC = npc;
        onDialogueEndCallback = onEndCallback; // Uložení callbacku

        if (currentNPC != null)
        {
            currentNPC.StopMovement();
        }

        DisplayNextLine();
    }

    void DisplayNextLine()
    {
        if (currentLineIndex < dialogueLines.Count)
        {
            DialogueLine line = dialogueLines[currentLineIndex];
            speakerNameText.text = line.speakerName;
            dialogueText.text = line.dialogueText;

            if (line.playerChoices.Count > 0)
            {
                DisplayChoices(line.playerChoices);
            }
            else
            {
                currentLineIndex++;
            }
        }
        else
        {
            EndDialogue();
        }
    }

    void DisplayChoices(List<PlayerChoice> choices)
    {
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (PlayerChoice choice in choices)
        {
            Button choiceButton = Instantiate(choiceButtonPrefab, choicesPanel);
            Text choiceText = choiceButton.GetComponentInChildren<Text>();
            choiceText.text = choice.choiceText;

            choiceButton.onClick.AddListener(() =>
            {
                MakeChoice(choice);
            });
        }
    }

    void MakeChoice(PlayerChoice choice)
    {
        NPC npcComponent = currentNPC?.GetComponent<NPC>();

        if (choice.startsQuest)
        {
            QuestManager questManager = FindObjectOfType<QuestManager>();
            questManager.AddQuest(npcComponent.assignedQuest);
        }
        else if (choice.checkForItem)
        {
            Inventory inventory = FindObjectOfType<Player>().GetComponent<Inventory>();
            if (inventory.HasItem(npcComponent.assignedQuest.requiredItemName))
            {
                inventory.RemoveItemByName(npcComponent.assignedQuest.requiredItemName);
                npcComponent.assignedQuest.isCompleted = true;
                FindObjectOfType<QuestManager>().CompleteQuest(npcComponent.assignedQuest);
                StartDialogue(npcComponent.completedDialogue, currentNPC);
            }
            else
            {
                dialogueText.text = "Nemáš požadovaný předmět.";
            }
        }

        lastChoiceIndex = choice.nextLineIndex;

        if (choice.nextLineIndex == -1)
        {
            if (currentNPC != null && currentNPC.GetComponent<EndNPC>() != null)
            {
                currentNPC.GetComponent<EndNPC>().EndGame();
            }
            else
            {
                EndDialogue();
            }
        }
        else
        {
            currentLineIndex = choice.nextLineIndex;
            DisplayNextLine();
        }
    }

    IEnumerator EndDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndDialogue();
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        secondaryPanel.SetActive(false);

        if (currentNPC != null)
        {
            currentNPC.ResumeMovement();
        }

        currentNPC = null;

        onDialogueEndCallback?.Invoke();
        onDialogueEndCallback = null;
    }

    public int LastChoiceIndex()
    {
        return lastChoiceIndex;
    }
    public void DisplayDynamicResponse(string npcName, string responseText)
    {
        speakerNameText.text = npcName;
        dialogueText.text = responseText;

        // Skryj tlačítka hráčových voleb
        foreach (Transform child in choicesPanel)
        {
            Destroy(child.gameObject);
        }
        choicesPanel.gameObject.SetActive(false);

        // Automaticky ukončí dialog po 2 sekundách
        StartCoroutine(EndDialogueAfterDelay(2f));
    }
}

