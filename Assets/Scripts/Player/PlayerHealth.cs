using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player HP (for UI)")]
    public float maxHealth = 50f;
    public float CurrentHealth { get; private set; }

    private PlayerController playerController;

    private void Awake()
    {
        // Try to find the PlayerController on the same GameObject
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            maxHealth = GameManager.Instance.maxHP;
            CurrentHealth = GameManager.Instance.currentHP;
        }
        else
        {
            CurrentHealth = maxHealth;
        }
    }

    private void Update()
    {
        // Mirror HP from GameManager each frame so the UI can display it
        if (GameManager.Instance != null)
        {
            maxHealth = GameManager.Instance.maxHP;
            CurrentHealth = Mathf.Clamp(GameManager.Instance.currentHP, 0f, maxHealth);
        }
    }

    // This method exists so old code that calls PlayerHealth.TakeDamage still works.
    // It simply forwards the damage to PlayerController / GameManager logic.
    public void TakeDamage(int amount)
    {
        if (playerController != null)
        {
            playerController.TakeDamage(amount);
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.currentHP -= amount;

            if (GameManager.Instance.currentHP <= 0)
            {
                GameManager.Instance.OnPlayerDied();
            }
        }
    }
}
