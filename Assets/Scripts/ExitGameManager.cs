using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitGameManager : MonoBehaviour
{
    public GameObject confirmExitPanel; // Referenci nastavíme v Unity Inspectoru

    void Start()
    {
        confirmExitPanel.SetActive(false); // Na začátku skryjeme potvrzovací panel
    }

    public void OpenConfirmExitPanel()
    {
        confirmExitPanel.SetActive(true); // Zobrazí potvrzovací dialog
    }

    public void CloseConfirmExitPanel()
    {
        confirmExitPanel.SetActive(false); // Skryje potvrzovací dialog
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Načte hlavní menu
    }
}
