using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int maxJumps = 2;

    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpCount;
    private PhotonView view;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>();

        if (view.IsMine)
        {
            PlayerStats.Instance.myCoins = 0;
        }
    }

    void Update()
    {
        if (view.IsMine)
        {
            Move();
            Jump();
        }
    }

    void Move()
    {
        float moveDirection = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            jumpCount = 1;
        }
        else if (!isGrounded && Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpCount++;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("floor"))
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (view.IsMine)
        {
            if (col.gameObject.CompareTag("Coin"))
            {
                PlayerStats.Instance.myCoins++;
                photonView.RPC("UpdateCoinCount", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, PlayerStats.Instance.myCoins);
                photonView.RPC("DestroyCoin", RpcTarget.All, col.gameObject.GetComponent<PhotonView>().ViewID);
            }

            if (col.gameObject.CompareTag("Missile"))
            {
                // Call GameManager to handle player death
                GameManager.Instance.PlayerDied(PhotonNetwork.NickName);

                // Destroy the player GameObject across the network
                photonView.RPC("DestroyPlayer", RpcTarget.All, this.gameObject.GetComponent<PhotonView>().ViewID);

                // Destroy the missile GameObject across the network
                photonView.RPC("DestroyMissile", RpcTarget.All, col.gameObject.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    [PunRPC]
    public void UpdateCoinCount(int playerId, int coins)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerId)
        {
            PlayerStats.Instance.myCoins = coins;
        }
        else
        {
            PlayerStats.Instance.opponentCoins = coins;
        }

        GameManager.Instance.UpdateCoinUI();
    }

    [PunRPC]
    public void DestroyCoin(int viewID)
    {
        PhotonView coinView = PhotonView.Find(viewID);
        if (coinView != null && coinView.IsMine)
        {
            PhotonNetwork.Destroy(coinView.gameObject);
        }
    }

    [PunRPC]
    public void DestroyMissile(int viewID)
    {
        PhotonView missileView = PhotonView.Find(viewID);
        if (missileView != null && missileView.IsMine)
        {
            PhotonNetwork.Destroy(missileView.gameObject);
        }
    }

    [PunRPC]
    public void DestroyPlayer(int viewID)
    {
        PhotonView playerView = PhotonView.Find(viewID);
        if (playerView != null && playerView.IsMine)
        {
            PhotonNetwork.Destroy(playerView.gameObject);
        }
    }
}