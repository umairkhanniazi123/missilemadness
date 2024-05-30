using UnityEngine;
using Photon.Pun;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public float spawnInterval = 2f; // Time interval between spawning objects
    public Transform spawnPoint; // Point where objects will spawn
    public float despawnX = -11f; // X-coordinate at which objects will despawn
    public static CoinSpawner Instance;
    private bool isSpawning = true;

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

    public void StopSpawning()
    {
        isSpawning = false;
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Start spawning objects
            InvokeRepeating("SpawnCoin", 0f, spawnInterval);
        }
    }

    void SpawnCoin()
    {
        if (!isSpawning)
            return;
        // Spawn the coin at the specified position using Photon
        GameObject coin = PhotonNetwork.Instantiate(coinPrefab.name, spawnPoint.position, Quaternion.identity);

            // Move the spawned coin across the screen
            Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(-5f, 0f); // Adjust speed as necessary
    }

    void Update()
    {
        if (!isSpawning)
            return;
        // Despawn objects that have reached the despawn X-coordinate
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Coin"))
        {
            if (obj.transform.position.x <= despawnX)
            {
                PhotonNetwork.Destroy(obj);
            }
        }
    }
}