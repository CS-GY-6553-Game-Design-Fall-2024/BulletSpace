using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public static PlayerController current;

    [Header("=== Stats ===")]
    [SerializeField] private float m_maxHealth;
    [SerializeField] private float m_currentHealth;
    public float healthRatio => m_currentHealth / m_maxHealth;
    [SerializeField] private bool m_playerActive = true;

    [Header("=== Movement Settings ===")]
    [SerializeField] private Vector2 m_origin = new Vector2(0f, -3f);
    [SerializeField] private float moveSpeed = 5f;  // Speed at which the player moves
    [SerializeField] private List<Gun> m_playerGuns = new List<Gun>();

    private void Awake() {
        current = this;
    }

    private void Start() {
        if (m_playerGuns.Count == 0) {
            Debug.LogError("No Gun component found on the player!");
        }
    }

    void Update()
    {
        // Disable everything if we're not active
        if (!m_playerActive) return;

        // Handle player movement
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow keys
        float moveY = Input.GetAxisRaw("Vertical");   // W/S or Up/Down Arrow keys
        if (Input.GetKey("x")) moveY = -1f;
        Vector3 move = new Vector3(moveX, moveY, 0) * moveSpeed * Time.deltaTime;
        transform.position += move;

        // Restrict movement
        ClampPositionToWindow();

        // Handle firing input. Only handle if we actually have a player gun referebnced
        if (m_playerGuns.Count == 0) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Start firing when spacebar is pressed
            foreach(Gun g in m_playerGuns) g.StartFireCycle();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            // Stop firing when spacebar is released
            foreach(Gun g in m_playerGuns) g.StopFireCycle();
        }
    }

    void ClampPositionToWindow()
    {
        Vector3 clampedPosition = transform.position;
        // TODO: clamp view to viewport
        float clampx = AspectRatioController.current.worldWidth / 2;
        float clampy = AspectRatioController.current.worldHeight / 2;
        // AspectRatioController.current.worldHeight
        clampedPosition.x = Mathf.Clamp(transform.position.x, -1 * clampx, clampx);
        clampedPosition.y = Mathf.Clamp(transform.position.y, -1 * clampy, clampy);
             
        transform.position = clampedPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Define the damage value
        float damage = 10f;  // Adjust this value based on your game's balance

        // Subtract health from the WorldCache
        m_currentHealth = Mathf.Clamp(m_currentHealth - damage, 0f, m_maxHealth);

        // Destroy the enemy bullet
        Destroy(other.gameObject);

        // Check if health goes below zero
        if (m_currentHealth <= 0) {
            // Inform the LevelController that it's game over
            LevelController.current.GameOver();
        }
    }

    public void ResetHealth() {
        m_currentHealth = m_maxHealth;
    }

    public void ResetPosition() {
        transform.position = new Vector3(m_origin.x, m_origin.y, 0f);
    }

    public void TogglePlayerActions(bool setTo) {
        m_playerActive = setTo;
        if (!setTo) foreach(Gun g in m_playerGuns) g.StopFireCycle();
    }
}
