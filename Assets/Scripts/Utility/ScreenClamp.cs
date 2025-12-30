using UnityEngine;

/// <summary>
/// Screen Clamp - Keeps character inside camera bounds
/// FIXED: Uses public property IsKnockback instead of private field
/// </summary>
public class ScreenClamp : MonoBehaviour
{
    public float edgeBuffer = 0.5f;
    private Camera cam;

    private void RefreshCamera() => cam = Camera.main;

    private void LateUpdate()
    {
        // ✅ FIXED: Use IsKnockback property instead of isKnockback field
        var kb = GetComponent<Knockback>();
        if (kb != null && kb.IsKnockback) return;

        if (cam == null)
        {
            RefreshCamera();
            if (cam == null) return;
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 camPos = cam.transform.position;
        Vector3 pos = transform.position;

        float minX = camPos.x - halfWidth + edgeBuffer;
        float maxX = camPos.x + halfWidth - edgeBuffer;
        float minY = camPos.y - halfHeight + edgeBuffer;
        float maxY = camPos.y + halfHeight - edgeBuffer;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}