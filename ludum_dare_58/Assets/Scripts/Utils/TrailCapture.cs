using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

public class TrailCapture : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera that only renders trails (can stay disabled in Hierarchy)")]
    public Camera trailCamera;

    [Header("Capture Settings")]
    public int captureWidth = 1024;
    public int captureHeight = 1024;
    public Color backgroundColor = new Color(0, 0, 0, 0);
    public bool transparentBackground = true;

    [Header("UI (optional)")]
    public Button captureButton;
    public RawImage previewImage;

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void DownloadImage(string base64Data, string filename);
#endif

    private void Start()
    {
        // Auto-find camera if not assigned
        if (trailCamera == null)
            trailCamera = GetComponentInChildren<Camera>();

        if (trailCamera == null)
        {
            Debug.LogError("[TrailCapture] No trail camera found in children!");
            return;
        }

        // Configure the capture camera (off-screen only)
        trailCamera.clearFlags = CameraClearFlags.SolidColor;
        trailCamera.backgroundColor = backgroundColor;

        // Make sure it's disabled so it never renders to screen
        trailCamera.enabled = false;

        if (captureButton != null)
            captureButton.onClick.AddListener(CaptureAndSave);
    }

    public void CaptureAndSave()
    {
        StartCoroutine(CaptureRoutine());
    }

    private IEnumerator CaptureRoutine()
    {
        // Wait for the end of the frame so trails are fully updated
        yield return new WaitForEndOfFrame();

        // Create a temporary render texture
        RenderTexture rt = new RenderTexture(
            captureWidth,
            captureHeight,
            24,
            RenderTextureFormat.ARGB32
        );

        // Backup existing settings
        var prevTarget = trailCamera.targetTexture;
        var prevActive = RenderTexture.active;

        trailCamera.targetTexture = rt;

        // Render off-screen (camera can be disabled)
        trailCamera.Render();

        // Read pixels from the RenderTexture
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(captureWidth, captureHeight, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        tex.Apply();

        // Restore
        trailCamera.targetTexture = prevTarget;
        RenderTexture.active = prevActive;
        rt.Release();
        Destroy(rt);

        // Encode to PNG
        byte[] pngData = tex.EncodeToPNG();

        // Optional preview in UI
        if (previewImage != null)
        {
            previewImage.texture = tex;
            previewImage.color = Color.white;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        string base64 = Convert.ToBase64String(pngData);
        DownloadImage(base64, "trail_capture.png");
#else
        string path = Path.Combine(Application.dataPath, "trail_capture.png");
        File.WriteAllBytes(path, pngData);
        Debug.Log($"[TrailCapture] Saved trail image to: {path}");
#endif
    }
}
