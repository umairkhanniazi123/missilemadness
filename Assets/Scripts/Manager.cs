using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{

    public GameObject InventoryPanel;

    public void SwitchScene()
    {
        SceneManager.LoadScene(2);
    }

    public void OnClick()
    {
        InventoryPanel.SetActive(true);
    }

    public void OnClickBack()
    {
        InventoryPanel.SetActive(false);
    }
}
