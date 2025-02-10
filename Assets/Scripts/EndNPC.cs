using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndNPC : MonoBehaviour
{
    public List<DialogueLine> endDialogue;  // Dialog pro ukončení hry
    private DialogueManager dialogueManager;
    private bool isPlayerInRange = false;

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartEndDialogue();
        }
    }

    void StartEndDialogue()
    {
        if (dialogueManager == null)
        {
            Debug.LogError("EndNPC: DialogueManager nebyl nalezen! Ujisti se, že je ve scéně.");
            return;
        }

        if (endDialogue == null || endDialogue.Count == 0)
        {
            Debug.LogError("EndNPC: Seznam dialogů je prázdný! Přidej dialog do EndNPC v Unity Inspectoru.");
            return;
        }

        dialogueManager.StartDialogue(endDialogue, null, EndGame);
    }

    public void EndGame()
    {
        Debug.Log("Hra ukončena!");
        Application.Quit();  // Ukončení aplikace
        // Pro testování v Unity Editoru lze použít
        // UnityEditor.EditorApplication.isPlaying = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
    public void SetDialogueManager(DialogueManager manager)
    {
        dialogueManager = manager;
    }

}
