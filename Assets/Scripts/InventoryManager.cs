using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public TMP_Text coinText;
    public TMP_Text goldText;
    public TMP_Text diamondsText;

    void Start()
    {
        GetInventoryData(LoginManager.Username);
    }

    void GetInventoryData(string username)
    {
        StartCoroutine(FetchInventoryData(username));
    }

    IEnumerator FetchInventoryData(string username)
    {
        // Create form data with the username
        WWWForm form = new WWWForm();
        form.AddField("username", username);

        // Send request to the server
        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://localhost/unity/getInventory.php", form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching inventory data: " + webRequest.error);
            }
            else
            {
                // Parse the response to extract inventory data
                string responseText = webRequest.downloadHandler.text;
                string[] inventoryData = responseText.Split(',');

                if (inventoryData.Length >= 3)
                {
                    // Update UI with inventory data
                    coinText.text = inventoryData[0];
                    goldText.text = inventoryData[1];
                    diamondsText.text = inventoryData[2];
                }
                else
                {
                    Debug.LogError("Invalid inventory data format");
                }
            }
        }
    }
}