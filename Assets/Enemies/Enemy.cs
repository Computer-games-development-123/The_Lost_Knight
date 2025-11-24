using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;

    // כמה נזק האויב עושה לשחקן במגע
    public int contactDamage = 1;

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(name + " took damage, hp = " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log(name + " died");
            Destroy(gameObject);
        }
    }

    // פגיעה בשחקן במגע (אפשר לשפר אחר כך)
private void OnCollisionEnter2D(Collision2D collision)
{
    Debug.Log(name + " collided with " + collision.collider.name);

    if (collision.collider.CompareTag("Player"))
    {
        Debug.Log(name + " collided with PLAYER, trying to deal damage");

        PlayerHealth ph = collision.collider.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            Debug.Log("Found PlayerHealth, dealing " + contactDamage + " damage");
            ph.TakeDamage(contactDamage);
        }
        else
        {
            Debug.LogWarning("Player has NO PlayerHealth component!");
        }
    }
}

}
