using UnityEngine;
/// <summary>
/// Adjusts the camera viewport to maintain a 16:9 aspect ratio with letterboxing.
/// </summary>
[RequireComponent(typeof(Camera))]
public class Letterbox16by9 : MonoBehaviour
{
    private const float targetAspect = 16f / 9f;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            // window is narrower -> add bars top/bottom
            Rect r = cam.rect;
            r.width = 1f;
            r.height = scaleHeight;
            r.x = 0f;
            r.y = (1f - scaleHeight) / 2f;
            cam.rect = r;
        }
        else
        {
            // window is wider -> add bars left/right
            float scaleWidth = 1f / scaleHeight;
            Rect r = cam.rect;
            r.width = scaleWidth;
            r.height = 1f;
            r.x = (1f - scaleWidth) / 2f;
            r.y = 0f;
            cam.rect = r;
        }
    }
}
