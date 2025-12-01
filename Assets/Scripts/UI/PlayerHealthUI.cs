using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public Slider healthSlider;
    public Image fillImage;

    [Header("Colors")]
    public Color healthyColor = Color.green;   // 70%+
    public Color midColor = Color.yellow;      // 30%-70%
    public Color lowColor = Color.red;         // 0%-30%

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }

        if (playerHealth != null && healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.value = playerHealth.CurrentHealth;
        }
    }

    private void Update()
    {
        if (playerHealth == null || healthSlider == null)
            return;

        // Update slider value
        healthSlider.value = playerHealth.CurrentHealth;

        // Health ratio between 0 and 1
        float t = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;

        // Choose color based on health percentage
        if (t > 0.7f)
        {
            fillImage.color = healthyColor;
        }
        else if (t > 0.3f)
        {
            fillImage.color = midColor;
        }
        else
        {
            fillImage.color = lowColor;
        }
    }
}
