using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public TMP_Text timerText;

    private float gameTime = 60f; // 1 minute game time
    private bool gameEnded = false;

    private PhotonView photonView;
    private double startTime;
    private bool startTimeSet = false;

    public TMP_Text player1CoinText;
    public TMP_Text player2CoinText;

    public GameObject winnerPanel;
    public GameObject loserPanel;
    public TMP_Text winnerText;
    public TMP_Text loserText;

    private string player1Name;
    private string player2Name;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        if (photonView == null)
        {
            Debug.LogError("PhotonView component not found in GameManager!");
        }

        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time;
            startTimeSet = true;
            photonView.RPC("SyncStartTime", RpcTarget.AllBuffered, startTime);
        }
        else
        {
            RequestStartTime();
        }

        player1CoinText = GameObject.Find("Player1Coins").GetComponent<TMP_Text>();
        player2CoinText = GameObject.Find("Player2Coins").GetComponent<TMP_Text>();

        if (player1CoinText == null || player2CoinText == null)
        {
            Debug.LogError("Player1Coins or Player2Coins TMP_Text not found!");
        }

        UpdatePlayerNames();
        UpdateCoinUI();
    }

    void Update()
    {
        if (!gameEnded && startTimeSet)
        {
            double elapsedTime = PhotonNetwork.Time - startTime;
            double remainingTime = gameTime - elapsedTime;

            if (remainingTime <= 0)
            {
                EndGame();
                remainingTime = 0;
            }

            timerText.text = "" + remainingTime.ToString("F2");
        }
    }

    public void LeaveRoomAndGoToLobby()
    {
        // Leave the current room
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        // Load the lobby scene after leaving the room
        SceneManager.LoadScene(0);
    }

    public void UpdateCoinUI()
    {
        if (player1CoinText != null && player2CoinText != null)
        {
            player1CoinText.text = player1Name + ": " + PlayerStats.Instance.myCoins;
            player2CoinText.text = player2Name + ": " + PlayerStats.Instance.opponentCoins;
        }
    }

    [PunRPC]
    void SyncStartTime(double networkStartTime)
    {
        startTime = networkStartTime;
        startTimeSet = true;
    }

    void RequestStartTime()
    {
        if (!startTimeSet)
        {
            photonView.RPC("RequestStartTimeFromMaster", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void RequestStartTimeFromMaster()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncStartTime", RpcTarget.OthersBuffered, startTime);
        }
    }

    void UpdatePlayerNames()
    {
        if (PhotonNetwork.PlayerList.Length > 1)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == PhotonNetwork.PlayerList[0].ActorNumber)
            {
                player1Name = PhotonNetwork.PlayerList[0].NickName;
                player2Name = PhotonNetwork.PlayerList[1].NickName;
            }
            else
            {
                player1Name = PhotonNetwork.PlayerList[1].NickName;
                player2Name = PhotonNetwork.PlayerList[0].NickName;
            }
        }
        else
        {
            player1Name = PhotonNetwork.LocalPlayer.NickName;
            player2Name = "Waiting for Player 2...";
        }
    }

    public void PlayerDied(string loserName)
    {
        if (!gameEnded)
        {
            gameEnded = true;
            string winnerName = loserName == PhotonNetwork.NickName ? PhotonNetwork.PlayerListOthers[0].NickName : PhotonNetwork.NickName;
            int winnerCoins = winnerName == PhotonNetwork.NickName ? PlayerStats.Instance.myCoins : PlayerStats.Instance.opponentCoins;
            int loserCoins = loserName == PhotonNetwork.NickName ? PlayerStats.Instance.myCoins : PlayerStats.Instance.opponentCoins;
            photonView.RPC("DisplayWinnerLoser", RpcTarget.All, winnerName, loserName, winnerCoins, loserCoins);
            photonView.RPC("EndGameRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    void DisplayWinnerLoser(string winnerName, string loserName, int winnerCoins, int loserCoins)
    {
        if (PhotonNetwork.NickName == winnerName)
        {
            winnerPanel.SetActive(true);
            winnerText.text = "You got " + winnerCoins + " coins.";
        }
        else if (PhotonNetwork.NickName == loserName)
        {
            loserPanel.SetActive(true);
            loserText.text = "You got " + loserCoins + " coins.";
        }
    }

    [PunRPC]
    void EndGameRPC()
    {
        gameEnded = true;
        MissileSpawner.Instance.StopSpawning();
        CoinSpawner.Instance.StopSpawning();
    }

    void EndGame()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            int myCoins = PlayerStats.Instance.myCoins;
            int opponentCoins = PlayerStats.Instance.opponentCoins;
            string winnerName;
            string loserName;

            if (myCoins > opponentCoins)
            {
                winnerName = PhotonNetwork.LocalPlayer.NickName;
                loserName = PhotonNetwork.PlayerListOthers[0].NickName;
            }
            else if (myCoins < opponentCoins)
            {
                winnerName = PhotonNetwork.PlayerListOthers[0].NickName;
                loserName = PhotonNetwork.LocalPlayer.NickName;
            }
            else
            {
                winnerName = null;
                loserName = null;
                winnerPanel.SetActive(true);
                loserPanel.SetActive(true);
                winnerText.text = "It's a Tie! Coins: " + myCoins;
                loserText.text = "It's a Tie! Coins: " + opponentCoins;
            }

            if (winnerName != null && loserName != null)
            {
                photonView.RPC("DisplayWinnerLoser", RpcTarget.All, winnerName, loserName, myCoins, opponentCoins);
            }
            photonView.RPC("EndGameRPC", RpcTarget.All);
        }
    }
}