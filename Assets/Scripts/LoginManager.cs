using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;
    public TextMeshProUGUI loginFeedbackText; // TextMeshPro text field to display feedback for login


    public static string Username { get; private set; }

    public static string GetUsername(TMP_InputField inputField)
    {
        if (inputField != null)
        {
            return inputField.text;
        }
        else
        {
            Debug.LogError("InputField is null");
            return null;
        }
    }

    public void LoginUser()
    {
        // Get username and password from TextMeshPro input fields
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;

        StartCoroutine(LoginUserRoutine(username, password));
    }

    IEnumerator LoginUserRoutine(string username, string password)
    {
        // Send login request to the PHP script
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/unity/login.php", form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error logging in: " + webRequest.error);
                loginFeedbackText.text = "Error logging in: " + webRequest.error;
            }
            else
            {
                string responseText = webRequest.downloadHandler.text;
                if (responseText.Contains("User authenticated"))
                {
                    Debug.Log("User authenticated. Logging in...");
                    loginFeedbackText.text = "User authenticated. Logging in...";

                    // Store the username
                    Username = username;

                    // Load the menu scene
                    SceneManager.LoadScene(1);
                }
                else if (responseText.Contains("No user found"))
                {
                    Debug.LogWarning("No user with such name exists");
                    loginFeedbackText.text = "No user with such name exists";
                }
                else
                {
                    Debug.LogError("Unknown error occurred during login");
                    loginFeedbackText.text = "Unknown error occurred during login";
                }
            }
        }
    }

    //IEnumerator SendUsernameForInventory(string username)
    //{
    //    // Send username to insertInventory.php
    //    WWWForm inventoryForm = new WWWForm();
    //    inventoryForm.AddField("username", username);

    //    using (UnityWebRequest inventoryRequest = UnityWebRequest.Post("http://localhost/unity/insertInventory.php", inventoryForm))
    //    {
    //        yield return inventoryRequest.SendWebRequest();

    //        if (inventoryRequest.result != UnityWebRequest.Result.Success)
    //        {
    //            Debug.LogError("Error sending username for inventory: " + inventoryRequest.error);
    //        }
    //        else
    //        {
    //            Debug.Log("Username sent for inventory successfully");
    //        }
    //    }
    //}

}