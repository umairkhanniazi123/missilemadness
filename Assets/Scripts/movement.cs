using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEditor;
using Photon.Pun;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int maxJumps = 2; // Maximum number of jumps allowed (including the initial jump)

    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpCount;
    private bool isGameRunning = true; // Flag to indicate if the game is running or stopped

    private int totalCoins, gold, diamonds, xp;
    public TMP_Text coinText, goldText, diamondsText;

    //public GameObject gameOverPanel;
    //public GameObject inventoryPanel;
    private int startTime;
    private int totalTimePlayed;
    

    PhotonView view;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpCount = 0;
        view = GetComponent<PhotonView>();
    }

    void StopGame()
    {
        // Stop the game and show game over panel
        isGameRunning = false;
        //gameOverPanel.SetActive(true);
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {



        if (!isGameRunning)
            return; // If the game is stopped, exit Update()

        if(view.IsMine)
        {
            // Move left and right
            float moveDirection = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

            // Jump if grounded and spacebar is pressed
            if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f); // Reset vertical velocity
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                isGrounded = false; // Set grounded status to false
                jumpCount = 1; // Reset jump count
            }
            // Double jump if in the air and spacebar is pressed and jump count is less than max jumps
            else if (!isGrounded && Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f); // Reset vertical velocity
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                jumpCount++; // Increment jump count
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the character collides with a collider tagged as "Ground"
        if (collision.gameObject.CompareTag("floor"))
        {
            isGrounded = true;
            jumpCount = 0; // Reset jump count when grounded
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!isGameRunning)
            return; // If the game is stopped, exit OnTriggerEnter2D()

        if (col.gameObject.CompareTag("Coin"))
        {
            totalCoins++;
            xp += 5; // Add 5 XP for each coin
            UpdateUI();
            Destroy(col.gameObject);
        }

        if (col.gameObject.CompareTag("Gold"))
        {
            gold++;
            xp += 10; // Add 10 XP for each gold
            UpdateUI();
            Destroy(col.gameObject);
        }

        if (col.gameObject.CompareTag("Diamonds"))
        {
            diamonds++;
            xp += 20; // Add 20 XP for each diamond
            UpdateUI();
            Destroy(col.gameObject);
        }

        if (col.gameObject.CompareTag("Missile"))
        {
            // Stop the game
            
            StopGame();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Reset grounded status when the character exits the ground collider
        if (collision.gameObject.CompareTag("floor"))
        {
            isGrounded = false;
        }
    }

    void UpdateUI()
    {
        coinText.text = "Coins: " + totalCoins.ToString();
        goldText.text = "Gold: " + gold.ToString();
        diamondsText.text = "Diamonds: " + diamonds.ToString();
    }

}