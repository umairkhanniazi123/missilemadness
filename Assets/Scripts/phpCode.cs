using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System;

public class phpCode : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI feedbackText; // TextMeshPro text field to display feedback to the user

    void Start()
    {
        
    }

    IEnumerator GetRequest()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("http://localhost/unity/getData.php"))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("HTTP Error: " + webRequest.error);
            }
            else if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
            }
        }
    }

    public IEnumerator RegisterUser(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/unity/insertData.php", form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error registering user: " + webRequest.error);
                feedbackText.text = "Error registering user: " + webRequest.error;
            }
            else
            {
                string responseText = webRequest.downloadHandler.text;
                if (responseText.Contains("Username already exists"))
                {
                    Debug.LogWarning("Username already exists");
                    feedbackText.text = "Username already exists. Please choose a different username.";
                }
                else
                {
                    Debug.Log("User registered successfully");
                    feedbackText.text = "User registered successfully";
                }
            }
        }
    }

    // Method to be called when registering a new user using input fields
    public void RegisterUser()
    {
        // Get username and password from TextMeshPro input fields
        string username = usernameInput.text;
        string password = passwordInput.text;

        StartCoroutine(RegisterUser(username, password));
    }
}