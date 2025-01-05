using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;  // Jméno mluvčího
    [TextArea(3, 10)]
    public string dialogueText; // Text dialogu
    public List<PlayerChoice> playerChoices; // Volby hráče
}

[System.Serializable]
public class PlayerChoice
{
    public string choiceText; // Text odpovědi hráče
    public int nextLineIndex; // Index následující repliky v dialogu (pokud je -1, dialog končí)
    public bool startsQuest;      // Určuje, zda tato volba spustí quest
    public bool checkForItem;     // Určuje, zda tato volba zkontroluje předmět v inventáři
}
