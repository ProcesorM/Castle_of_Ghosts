using UnityEngine;
using UnityEngine.UI;

public class PasswordPanel : MonoBehaviour
{
    public InputField passwordInput; // UI InputField pro zadání hesla
    private Door doorToUnlock;
    private string correctPassword = "heslo"; // Správné heslo (může být nahrazeno generováným heslem)
    private Player player;

    void Start()
    {
        // Nastavení správného hesla z vybrané hádanky
        RoomGenerator roomGenerator = FindObjectOfType<RoomGenerator>();
        if (roomGenerator != null)
        {
            correctPassword = roomGenerator.correctPassword;
        }

        player = FindObjectOfType<Player>();

    }
    public void SetDoorToUnlock(Door door)
    {
        doorToUnlock = door;

        DisablePlayerMovement();
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

            EnablePlayerMovement();
        }
        else
        {
            Debug.Log("Špatné heslo, zkuste to znovu.");
        }
    }

    public void OnCancel()
    {
        gameObject.SetActive(false); // Skryje panel, pokud se hráč rozhodne zrušit zadání hesla

        EnablePlayerMovement();
    }
    private void DisablePlayerMovement()
    {
        if (player != null)
        {
            player.SetMovementEnabled(false); // Vypne pohyb hráče
        }
    }

    private void EnablePlayerMovement()
    {
        if (player != null)
        {
            player.SetMovementEnabled(true); // Obnoví pohyb hráče
        }
    }
}