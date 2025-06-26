using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMode : MonoBehaviour
{
    public string modeName;

    public void LoadGameMode()
    {
        SceneManager.LoadScene(modeName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
