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
        // BYPASS invulnerability for scripted deaths (George's instant kill)
        if (amount >= 9000)
        {
            Debug.Log($"⚠️ SCRIPTED DEATH: Taking {amount} damage (bypassing invulnerability)");
            currentHealth = 0;
            Die();
            return;
        }

        // Normal damage - respect invulnerability
        if (playerController != null && playerController.IsInvulnerable)
        {
            Debug.Log("Player is invulnerable - damage blocked");
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
        if (currentHealth <= 0) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
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

        if (DialogueManager.Instance != null && deathDialogue != null)
        {
            DialogueManager.Instance.Play(deathDialogue, () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnPlayerDied();
                }
            });
        }
        else
        {
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