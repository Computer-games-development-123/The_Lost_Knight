using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string registerSceneName = "RegisterMenu";
    [SerializeField] private string loginSceneName = "LoginMenu";

    public void OnNewGame()
    {
        SceneManager.LoadScene(registerSceneName);
    }

    public void OnLoadGame()
    {
        SceneManager.LoadScene(loginSceneName);
    }
}
