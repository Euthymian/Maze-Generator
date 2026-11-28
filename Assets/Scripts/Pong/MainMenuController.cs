using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";

    public void StartOneVsOne()
    {
        GameModeSettings.VsComputer = false;
        SceneManager.LoadScene(gameSceneName);
    }

    public void StartVsComputer()
    {
        GameModeSettings.VsComputer = true;
        SceneManager.LoadScene(gameSceneName);
    }

}
