using System;
using UnityEngine;

public class camera_publisher : MonoBehaviour
{
    private Camera _camera;

    [Header("Camera Resolution")]
    public int resWidth = 640;
    public int resHeight = 480;

    [Header("Capture Settings")]
    public float captureFPS = 15f;

    [HideInInspector]
    public byte[] jpg;

    private RenderTexture rt;
    private float captureTimer = 0f;

    void Start()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            Debug.LogError("Camera component not found on this GameObject!");
            return;
        }

        rt = new RenderTexture(resWidth, resHeight, 24);
    }

    void Update()
    {
        if (_camera == null) return;

        captureTimer += Time.deltaTime;
        if (captureTimer >= 1f / captureFPS)
        {
            CaptureJPG();
            captureTimer = 0f;
        }
    }

    private void CaptureJPG()
    {
        try
        {
            RenderTexture prevRT = RenderTexture.active;
            RenderTexture prevCamRT = _camera.targetTexture;

            _camera.targetTexture = rt;
            RenderTexture.active = rt;

            // Render camera view
            _camera.Render();

            // --- NEW PART: crop viewport region ---
            Rect vp = _camera.rect;
            int cropX = (int)(vp.x * resWidth);
            int cropY = (int)(vp.y * resHeight);
            int cropW = (int)(vp.width * resWidth);
            int cropH = (int)(vp.height * resHeight);
            // --------------------------------------

            Texture2D tex = new Texture2D(cropW, cropH, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(cropX, cropY, cropW, cropH), 0, 0);
            tex.Apply();

            jpg = tex.EncodeToJPG();

            Destroy(tex);

            _camera.targetTexture = prevCamRT;
            RenderTexture.active = prevRT;
        }
        catch (Exception e)
        {
            Debug.LogError("Error capturing JPEG: " + e.Message);
            jpg = null;
        }
    }

    void OnDestroy()
    {
        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
        }
    }
}
