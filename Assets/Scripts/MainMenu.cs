using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // Název scény s hrou
    }

    public void QuitGame()
    {
        Debug.Log("Hra ukončena!");
        Application.Quit();
    }
}
