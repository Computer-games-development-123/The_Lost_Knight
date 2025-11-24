using UnityEngine;

public class YojiGateBarrier : MonoBehaviour
{
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // אם מסיבה כלשהי כבר דיברו עם יוג'י לפני שהסצנה נטענה
        if (GameManager.Instance != null && GameManager.Instance.hasTalkedToYoji)
        {
            col.enabled = false;
        }
    }

    public void DisableBarrier()
    {
        if (col != null)
        {
            col.enabled = false;
            Debug.Log("[YojiGateBarrier] Barrier disabled, player can now reach the portal.");
        }
    }
}
