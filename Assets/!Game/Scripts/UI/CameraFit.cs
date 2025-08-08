using UnityEngine;

public class CameraFit : MonoBehaviour
{
    public float targetAspect = 9f / 16f;

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            cam.orthographicSize /= scaleHeight;
        }
    }
}
