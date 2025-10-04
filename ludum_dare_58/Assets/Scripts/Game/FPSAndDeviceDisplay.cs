using UnityEngine;
using TMPro; // Import the TextMeshPro namespace

public class FPSAndDeviceDisplay : MonoBehaviour
{
    // Use TextMeshProUGUI for TextMeshPro UI elements
    public TextMeshProUGUI displayText; 

    private float deltaTime = 0.0f;
    private string deviceType;

    void Start()
    {
        // Determine the device type using WebGL specific checks and general device checks
        #if UNITY_WEBGL && !UNITY_EDITOR
            if (Input.touchSupported)
            {
                deviceType = "Mobile";
            }
            else
            {
                deviceType = "Desktop";
            }
        #else
            if (SystemInfo.deviceType == DeviceType.Handheld || Input.touchSupported)
            {
                deviceType = "Mobile";
            }
            else
            {
                deviceType = "Desktop";
            }
        #endif
    }

    void Update()
    {
        // Calculate FPS with some smoothing
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
           
        // Update the display text with FPS and device type
        displayText.text = $"FPS: {Mathf.Ceil(fps)}\nDevice: {deviceType}";
    }
}
