using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAutoSize : MonoBehaviour
{
    [SerializeField] float referenceOrthographicSize = 5f;
    [SerializeField] float referenceAspectWidth = 16f;
    [SerializeField] float referenceAspectHeight = 9f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        float referenceAspect = referenceAspectWidth / referenceAspectHeight;
        float currentAspect = (float)Screen.width / Screen.height;

        cam.orthographicSize = referenceOrthographicSize * (referenceAspect / currentAspect);
    }
}