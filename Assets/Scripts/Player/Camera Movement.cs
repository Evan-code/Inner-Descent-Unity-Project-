using UnityEngine;

// This script makes the camera smoothly follow a target
// Usually the target is the player
public class SmoothCameraFollow : MonoBehaviour {
    // This stores the starting distance between the camera and the target
    private Vector3 offset;

    // This is the object the camera follows
    [SerializeField] private Transform target;

    // Smaller smoothTime means faster camera movement
    // Larger smoothTime means slower and smoother camera movement
    [SerializeField] private float smoothTime = 0.15f;

    // SmoothDamp uses this to remember camera velocity while smoothing
    private Vector3 currentVelocity = Vector3.zero;

    private void Awake() {
        // If no target was assigned, do nothing
        if (target == null) {
            return;
        }

        // Calculate the starting distance between camera and target
        offset = transform.position - target.position;
    }

    private void LateUpdate() {
        // If target is missing, do nothing
        if (target == null) {
            return;
        }

        // The camera wants to be at the target position plus the offset
        Vector3 targetPosition = target.position + offset;

        // Smoothly move the camera toward the target position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }
}