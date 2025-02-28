using System.Globalization;
using System.Text;
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

        // Odebrání diakritiky a převedení na malá písmena
        string inputPassword = RemoveDiacritics(passwordInput.text).ToLower();
        string correctNormalizedPassword = RemoveDiacritics(correctPassword).ToLower();

        if (inputPassword == correctNormalizedPassword)
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
    private string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        string normalizedText = text.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalizedText)
        {
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
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