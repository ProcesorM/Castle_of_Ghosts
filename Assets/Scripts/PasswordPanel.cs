using UnityEngine;
using UnityEngine.UI;

public class PasswordPanel : MonoBehaviour
{
    public InputField passwordInput; // UI InputField pro zadání hesla
    private Door doorToUnlock;
    private string correctPassword = "heslo"; // Správné heslo (může být nahrazeno generováným heslem)

    void Start()
    {
        // Nastavení správného hesla z vybrané hádanky
        RoomGenerator roomGenerator = FindObjectOfType<RoomGenerator>();
        if (roomGenerator != null)
        {
            correctPassword = roomGenerator.correctPassword;
        }

    }
    public void SetDoorToUnlock(Door door)
    {
        doorToUnlock = door;

    }

    public void OnSubmitPassword()
    {
        Debug.Log("Submit tlačítko bylo stisknuto"); // Pro kontrolu, zda metoda běží
        if (passwordInput.text == correctPassword)
        {
            Debug.Log("Správné heslo! Dveře odemčeny.");
            doorToUnlock.UnlockDoor();
            doorToUnlock.SetGreenColor(); // Změň barvu dveří na zelenou
            gameObject.SetActive(false); // Skryj panel po úspěšném zadání hesla


        }
        else
        {
            Debug.Log("Špatné heslo, zkuste to znovu.");
        }
    }

    public void OnCancel()
    {
        gameObject.SetActive(false); // Skryje panel, pokud se hráč rozhodne zrušit zadání hesla


    }
}