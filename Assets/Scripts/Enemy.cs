using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 3;
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

}
