using UnityEngine;
using System.Collections;

public class PortalSpawnEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private Collider2D portalCollider;

    private SpriteRenderer sr;
    private Vector3 targetScale;
    private Coroutine routine;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        targetScale = transform.localScale;

        if (portalCollider != null)
            portalCollider.enabled = false;
    }

    private void OnEnable()
    {
        PlayOpen();
    }

    public void PlayOpen()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        if (portalCollider != null)
            portalCollider.enabled = false;

        float t = 0f;
        transform.localScale = Vector3.zero;

        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);

            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, k);
            c.a = k;
            sr.color = c;

            yield return null;
        }

        transform.localScale = targetScale;
        c.a = 1f;
        sr.color = c;

        if (portalCollider != null)
            portalCollider.enabled = true;

        routine = null;
    }

    public void Close()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        if (portalCollider != null)
            portalCollider.enabled = false;

        float t = 0f;

        Color c = sr.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = 1f - (t / duration);

            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, k);
            c.a = k;
            sr.color = c;

            yield return null;
        }

        transform.localScale = Vector3.zero;
        c.a = 0f;
        sr.color = c;

        routine = null;

        gameObject.SetActive(false);
    }
}

/*
כשנרצה לכבות נפעיל:
portal.GetComponent<PortalSpawnEffect>().Close();
ולא
portal.SetActive(false);
כי זה ידלג על האנימציה
*/