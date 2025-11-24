using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player HP")]
    public int maxHealth = 50;
    public int CurrentHealth { get; private set; }

    private bool isDead = false;

    private void Start()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;  // לא לקבל נזק אחרי מוות

        CurrentHealth -= amount;
        if (CurrentHealth < 0) CurrentHealth = 0;   // לא לרדת מתחת ל־0

        Debug.Log($"Player took damage: {amount}, hp = {CurrentHealth}");

        if (CurrentHealth == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player died! Game Over");

        // הכי פשוט למטלה – ריסט לסצנה
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }
}
