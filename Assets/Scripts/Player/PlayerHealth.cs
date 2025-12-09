using UnityEngine;
using TMPro;
public class PlayerHealth : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData deathDialogue;
    [Header("Player HP")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float currentHealth = 50f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        // Initialize health to max at start
        currentHealth = maxHealth;
    }


    public void TakeDamage(int amount)
    {
        if (playerController != null && playerController.IsInvulnerable)
        {
            return; // Respect i-frames from PlayerController
        }

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Player took {amount} damage. HP: {currentHealth}/{maxHealth}");

        // Trigger i-frames in PlayerController
        if (playerController != null)
        {
            playerController.TriggerInvulnerability();
            playerController.PlayHurtAnimation();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Healed {amount}. HP: {currentHealth}/{maxHealth}");
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log("Player died!");

        if (playerController != null)
        {
            playerController.PlayDeathAnimation();
        }

        // If we have a death dialogue, show it first, then do OnPlayerDied
        if (DialogueManager.Instance != null && deathDialogue != null)
        {
            DialogueManager.Instance.Play(deathDialogue, () =>
            {
                // After the dialogue finishes, apply death logic (coins + reload scene)
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnPlayerDied();
                }
            });
        }
        else
        {
            // No dialogue? Just do death logic immediately
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDied();
            }
        }
    }


    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }
}