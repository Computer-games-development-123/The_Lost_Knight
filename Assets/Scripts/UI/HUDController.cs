using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI potionsText;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Health Bar Asset")]
    [SerializeField] private Image healthFill; // Image type = Filled

    private PlayerStats ps;

    private void Start()
    {
        TryBindPlayer();
        Refresh();
    }

    private void Update()
    {
        if (ps == null)
        {
            TryBindPlayer();
            return;
        }

        Refresh();
    }

    private void TryBindPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        ps = player.GetComponent<PlayerStats>();
    }

    private void Refresh()
    {
        if (ps == null) return;

        if (coinsText != null)
            coinsText.text = $"{ps.coins}";

        if (potionsText != null)
            potionsText.text = $"{ps.potions}/{ps.MaxPotions}";

        if (healthFill != null)
        {
            float maxHp = Mathf.Max(1f, ps.MaxHP);
            healthFill.fillAmount = ps.currentHP / maxHp;
        }

        if (hpText != null)
            hpText.text = $"{ps.currentHP}/{ps.MaxHP}";
    }
}
