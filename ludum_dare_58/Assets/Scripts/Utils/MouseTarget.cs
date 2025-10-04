using UnityEngine;

public class MouseTarget : MonoBehaviour
{
    
    void Update()
    {
        // if click, update position
        if (Input.GetMouseButtonDown(0))
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        Vector3 newPosition = GetMouseWorldPosition();
        transform.position = newPosition;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.y = transform.position.y; // Maintain current Y position
        return worldPosition;
    }
}