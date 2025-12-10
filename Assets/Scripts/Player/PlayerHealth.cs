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
    public bool IsAtFullHealth => currentHealth >= maxHealth;

    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }


    public void TakeDamage(int amount)
    {
        if (playerController != null && playerController.IsInvulnerable)
        {
            return; 
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
        if (amount <= 0) return;
        if (currentHealth <= 0) return; // dead, can't heal

        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        // TODO: notify health UI if needed
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