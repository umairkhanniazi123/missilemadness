using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject LoadingScreen;
    public GameObject MenuButtons;
    public TMP_Text LoadingText;

    public GameObject CreateRoomScreen;
    public TMP_InputField RoomNameInput;

    public GameObject RoomScreen;
    public TMP_Text RoomNameText, PlayerNamelabel;
    private List<TMP_Text> AllPlayerNames = new List<TMP_Text>();

    public GameObject ErrorScreen;
    public TMP_Text ErrorText;

    public GameObject RoomBrowserScreen;
    public RoomButton TheRoomButton;
    private List<RoomButton> AllRoomButtons = new List<RoomButton>();

    public GameObject NameInputScreen;
    public TMP_InputField NameInput;
    private bool HasSetNick;

    public string LevelToPlay;
    public GameObject StartButton;

    private GameObject RoomTestButton;

    // Reference to the loading animation prefab
    public GameObject LoadingAnimationPrefab;
    private GameObject loadingAnimationInstance;

    // Start is called before the first frame update
    void Start()
    {
        CloseMenu();
        ShowLoadingScreen("Connecting to Network...");

        PhotonNetwork.ConnectUsingSettings();
    }

    void CloseMenu()
    {
        LoadingScreen.SetActive(false);
        MenuButtons.SetActive(false);
        CreateRoomScreen.SetActive(false);
        RoomScreen.SetActive(false);
        ErrorScreen.SetActive(false);
        RoomBrowserScreen.SetActive(false);
        NameInputScreen.SetActive(false);

        // Destroy the loading animation instance if it exists
        if (loadingAnimationInstance != null)
        {
            Destroy(loadingAnimationInstance);
            loadingAnimationInstance = null;
        }
    }

    void ShowLoadingScreen(string message)
    {
        CloseMenu();
        LoadingScreen.SetActive(true);
        LoadingText.text = message;

        // Instantiate the loading animation prefab
        if (LoadingAnimationPrefab != null)
        {
            loadingAnimationInstance = Instantiate(LoadingAnimationPrefab, LoadingScreen.transform);
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;

        ShowLoadingScreen("Joining Lobby...");
    }

    public override void OnJoinedLobby()
    {
        CloseMenu();
        MenuButtons.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if (!HasSetNick)
        {
            CloseMenu();
            NameInputScreen.SetActive(true);

            if (PlayerPrefs.HasKey("PlayerName"))
            {
                NameInput.text = PlayerPrefs.GetString("PlayerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void OpenRoomCreate()
    {
        CloseMenu();
        CreateRoomScreen.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(RoomNameInput.text))
        {
            RoomOptions Options = new RoomOptions();
            Options.MaxPlayers = 8;

            PhotonNetwork.CreateRoom(RoomNameInput.text, Options);

            ShowLoadingScreen("Creating room...");
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenu();
        RoomScreen.SetActive(true);
        RoomNameText.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    private void ListAllPlayers()
    {
        foreach (TMP_Text Player in AllPlayerNames)
        {
            Destroy(Player.gameObject);
        }
        AllPlayerNames.Clear();

        Player[] Players = PhotonNetwork.PlayerList;
        for (int i = 0; i < Players.Length; i++)
        {
            TMP_Text NewPlayerLabel = Instantiate(PlayerNamelabel, PlayerNamelabel.transform.parent);
            NewPlayerLabel.text = Players[i].NickName;
            NewPlayerLabel.gameObject.SetActive(true);

            AllPlayerNames.Add(NewPlayerLabel);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text NewPlayerLabel = Instantiate(PlayerNamelabel, PlayerNamelabel.transform.parent);
        NewPlayerLabel.text = newPlayer.NickName;
        NewPlayerLabel.gameObject.SetActive(true);

        AllPlayerNames.Add(NewPlayerLabel);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        ErrorText.text = "Failed to Create Room: " + message;
        CloseMenu();
        ErrorScreen.SetActive(true);
    }

    public void CloseErrorScreen()
    {
        CloseMenu();
        MenuButtons.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        ShowLoadingScreen("Leaving Room...");
    }

    public override void OnConnected()
    {
        base.OnConnected();
        OnLeftRoom();
        {
            CloseMenu();
            MenuButtons.SetActive(true);
        }
    }

    public void OpenRoomBrowser()
    {
        CloseMenu();
        RoomBrowserScreen.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenu();
        MenuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in AllRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        AllRoomButtons.Clear();

        TheRoomButton.gameObject.SetActive(false);

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(TheRoomButton, TheRoomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);

                AllRoomButtons.Add(newButton);
            }
        }
    }

    public void JoinRoom(RoomInfo InputInfo)
    {
        PhotonNetwork.JoinRoom(InputInfo.Name);

        ShowLoadingScreen("Joining Room...");
    }

    public void SetNickName()
    {
        if (!string.IsNullOrEmpty(NameInput.text))
        {
            PhotonNetwork.NickName = NameInput.text;

            PlayerPrefs.SetString("PlayerName", NameInput.text);

            CloseMenu();
            MenuButtons.SetActive(true);
            HasSetNick = true;
        }
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(LevelToPlay);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    public void QuickJoin()
    {
        RoomOptions Options = new RoomOptions();
        Options.MaxPlayers = 8;

        PhotonNetwork.CreateRoom("Test", Options);
        ShowLoadingScreen("Creating Room");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}