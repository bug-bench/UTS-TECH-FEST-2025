using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMode : MonoBehaviour
{
    public string modeName;

    public void loadGameMode() {
        SceneManager.LoadScene(modeName);
    }
}
