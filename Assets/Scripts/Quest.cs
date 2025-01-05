[System.Serializable]
public class Quest
{
    public string questName;          // Název úkolu
    public string questDescription;   // Popis úkolu
    public bool isCompleted = false;  // Stav dokončení úkolu
    public string requiredItemName;   // Název požadovaného předmětu
}
