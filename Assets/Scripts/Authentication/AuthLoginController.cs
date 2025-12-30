using System;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthLoginController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_Text statusText;

    [Header("Scenes")]
    [SerializeField] private string nextScene = "Forest_Hub";
    [SerializeField] private string previusScene = "MainMenu";

    private const string FIXED_PASSWORD = "LostKnight1!";

    public async void OnLoginClicked()
    {
        if (!UGSInitializer.IsReady)
        {
            SetStatus("Services not ready yet...");
            return;
        }

        string username = Sanitize(usernameInput);
        if (string.IsNullOrEmpty(username))
        {
            SetStatus("Enter a username");
            return;
        }

        SetStatus("Logging in...");

        try
        {
            await AuthenticationService.Instance
                .SignInWithUsernamePasswordAsync(username, FIXED_PASSWORD);

            Debug.Log("Logged in PlayerId: " +
                      AuthenticationService.Instance.PlayerId);

            SetStatus("Login success!");
            SceneManager.LoadScene(nextScene);
        }
        catch (Exception e)
        {
            SetStatus($"Login failed: {e.Message}");
        }
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene(previusScene);
    }

    private static string Sanitize(TMP_InputField field)
    {
        return field == null ? "" : field.text.Trim().Replace(" ", "");
    }

    private void SetStatus(string msg)
    {
        if (msg == "Login failed: Invalid username or password") msg = "Login failed: Invalid username";
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }
}
