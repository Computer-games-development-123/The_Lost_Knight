using UnityEngine;

/// <summary>
/// Player Health - HP Management ONLY
/// Syncs maxHealth FROM PlayerState on Start()
/// currentHealth is runtime-only (NOT saved)
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData deathDialogue;
    
    [Header("Player HP (Runtime Only)")]
    [SerializeField] private float maxHealth = 50f;  // Synced from PlayerState on Start
    [SerializeField] private float currentHealth = 50f;  // Runtime only, NOT saved

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsAtFullHealth => currentHealth >= maxHealth;

    private PlayerController playerController;
    private Invulnerability invulnerability;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        invulnerability = GetComponent<Invulnerability>();
    }

    private void Start()
    {
        // Start with full HP
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
        if (invulnerability != null && invulnerability.IsInvulnerable)
        {
            Debug.Log("Player is invulnerable - damage blocked");
            return; 
        }

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Player took {amount} damage. HP: {currentHealth}/{maxHealth}");

        // Trigger i-frames
        if (invulnerability != null)
        {
            invulnerability.Trigger();
        }

        // Trigger hurt animation
        if (playerController != null)
        {
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
        
        Debug.Log($"💚 Healed {amount} HP. Current: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Called by PlayerState when max health increases (shop upgrade)
    /// </summary>
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        Debug.Log($"💚 Max health updated to {maxHealth}");
    }

    private void Die()
    {
        Debug.Log("💀 Player died!");

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

    #region Upgrade Methods (Called by StoreController)
    
    /// <summary>
    /// Increase max health by a fixed amount
    /// Called by StoreController when purchasing HP upgrade
    /// </summary>
    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        Debug.Log($"❤️ Max health increased by {amount}. New max: {maxHealth}");
    }

    /// <summary>
    /// Multiply max health
    /// Called by StoreController when purchasing Magic Armor
    /// </summary>
    public void MultiplyMaxHealth(float multiplier)
    {
        maxHealth *= multiplier;
        Debug.Log($"❤️ Max health multiplied by {multiplier}. New max: {maxHealth}");
    }
    
    #endregion
}