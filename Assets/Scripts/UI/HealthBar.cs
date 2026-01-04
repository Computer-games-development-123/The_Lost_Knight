using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Asset")]
    [SerializeField] private Image healthFill;
    private BossBase BB;

    private void Update()
    {
        TryBindPlayer();

        Refresh();
    }
    private void TryBindPlayer()
    {
        // Find Player GameObject
        var boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
        {
            BB = boss.GetComponent<BossBase>();
        }
        if (BB == null)
        {
            Debug.LogWarning("Can't find BossBase component!");
        }

    }

    private void Refresh()
    {
        if (BB == null) return;

        // Health bar (from PlayerHealth)
        if (healthFill != null)
        {
            float maxHp = Mathf.Max(1f, BB.maxHP);
            healthFill.fillAmount = BB.CurrentHP / maxHp;
        }

        // // HP text (from PlayerHealth)
        // if (hpText != null)
        //     hpText.text = $"{playerHealth.CurrentHealth:F0}/{playerHealth.MaxHealth:F0}";
    }
}
