using UnityEngine;

public class WebGLInputDetector : MonoBehaviour
{
    void Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            // For WebGL builds, check for touch support.
            if (Input.touchSupported)
            {
                Debug.Log("Touch supported: Activating mobile joystick controls.");
                // Initialize your mobile joystick here.
            }
            else
            {
                Debug.Log("Touch not supported: Activating desktop WASD controls.");
                // Initialize your desktop WASD controls here.
            }
        #else
            // For other builds, you can use device type detection.
            if (SystemInfo.deviceType == DeviceType.Handheld || Input.touchSupported)
            {
                Debug.Log("Mobile device detected.");
                // Activate mobile joystick controls.
            }
            else
            {
                Debug.Log("Desktop device detected.");
                // Activate desktop WASD controls.
            }
        #endif
    }
}
