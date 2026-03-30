using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // Assign your player here in the inspector
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -5);
    [SerializeField] private float smoothSpeed = 0.125f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Optional: Look at the player
        // transform.LookAt(target);
    }
}

