using UnityEngine;
using UnityEngine.UI;

public class HintDisplayPanel : MonoBehaviour
{
    public Text hintText; // Textové pole pro zobrazení textu nápovědy
    public Button closeButton; // Tlačítko pro zavření panelu

    void Start()
    {
        // Skryj panel na začátku hry
        gameObject.SetActive(false);

        // Připoj událost k tlačítku pro zavření panelu
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    public void ShowHint(string text)
    {
        if (hintText != null)
        {
            hintText.text = text; // Nastav text nápovědy
        }
        gameObject.SetActive(true); // Zobraz panel
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false); // Skryj panel
    }
}
