/*
🧠 Setup Guide
1. Create a “Trails” Layer

Go to Edit → Project Settings → Tags & Layers → Layers

Add a new layer called Trails

Assign your TrailRenderer objects to that layer.

2. Add a Dedicated Capture Camera

Duplicate your main camera or create a new one.

Set its Culling Mask to only include Trails.

Set the Clear Flags to Solid Color and Background Color to transparent.

Uncheck Audio Listener and any scripts you don’t need.

3. Attach the Script

Create an empty GameObject, name it TrailCaptureManager.

Attach TrailCapture.cs.

Assign your capture camera to the trailCamera field.

(Optional) Add a UI Button and drag it into the captureButton field.

(Optional) Add a RawImage to show a live preview of the captured trails.

🌐 WebGL Notes

To make downloads work in WebGL, add this small JS plugin:

🧩 File: Assets/Plugins/DownloadImage.jslib
mergeInto(LibraryManager.library, {
  DownloadImage: function (base64DataPtr, filenamePtr) {
    var base64Data = UTF8ToString(base64DataPtr);
    var filename = UTF8ToString(filenamePtr);
    var a = document.createElement("a");
    a.href = "data:image/png;base64," + base64Data;
    a.download = filename;
    a.click();
  }
});


✅ Works seamlessly in browsers — it will trigger a “Save File” dialog for your PNG.


*/

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
    [Tooltip("Camera that only renders trails (set its Culling Mask to 'Trails')")]
    public Camera trailCamera;

    [Header("Capture Settings")]
    public int captureWidth = 1024;
    public int captureHeight = 1024;
    public Color backgroundColor = new Color(0, 0, 0, 0); // transparent
    public bool transparentBackground = true;

    [Header("UI (optional)")]
    public Button captureButton;
    public RawImage previewImage; // optional in-game preview

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void DownloadImage(string base64Data, string filename);
#endif

    void Start()
    {
        if (trailCamera == null)
        {
            Debug.LogError("[TrailCapture] Please assign a camera!");
            return;
        }

        trailCamera.clearFlags = CameraClearFlags.SolidColor;
        trailCamera.backgroundColor = backgroundColor;

        if (captureButton)
            captureButton.onClick.AddListener(CaptureAndSave);
    }

    public void CaptureAndSave()
    {
        StartCoroutine(CaptureRoutine());
    }

    private IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame(); // wait for trails to render

        RenderTexture rt = new RenderTexture(captureWidth, captureHeight, 24, RenderTextureFormat.ARGB32);
        trailCamera.targetTexture = rt;
        trailCamera.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(captureWidth, captureHeight, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        tex.Apply();

        trailCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

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
