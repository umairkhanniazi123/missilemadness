using UnityEngine;
using Photon.Pun;

public class MissileSpawner : MonoBehaviour
{
    public static MissileSpawner Instance;
    public GameObject missilePrefab;
    public Transform spawnPoint;
    public float spawnInterval = 2f;
    public float missileSpeed = 5f;
    public float despawnX = -11f;

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

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            InvokeRepeating("SpawnMissile", 0f, spawnInterval);
        }
    }

    void SpawnMissile()
    {
        if (!isSpawning) 
            return;

        GameObject missile = PhotonNetwork.Instantiate(missilePrefab.name, spawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = missile.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(-missileSpeed, 0f);
    }

    void Update()
    {
        if (!isSpawning) 
            return;

        foreach (GameObject missile in GameObject.FindGameObjectsWithTag("Missile"))
        {
            if (missile.transform.position.x <= despawnX)
            {
                PhotonNetwork.Destroy(missile);
            }
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }
}