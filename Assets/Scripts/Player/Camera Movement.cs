using System.Collections;           
using System.Collections.Generic;    
using UnityEngine;                   


public class SmoothCameraFollow : MonoBehaviour {
    
    // A Vector3 stores a position or direction in 3D space (x, y, z)
    // and offset will store the distance between the camera and the object it follows
    private Vector3 offset;

    [SerializeField] private Transform target;

    // smoothTime will control how smoothly the camera follows the target
    // Smaller numbers = faster/snappier movement
    // Larger numbers = slower/smoother movement
    [SerializeField] private float smoothTime;

    // This stores the current velocity of the camera
    // It's needed for the SmoothDamp thingy to keep track of how fast the camera is moving
    // so Vector3.zero really just means (0,0,0)
    private Vector3 _currentVelocity = Vector3.zero;

    // This calculates the starting distance between the camera and the target
    // Example:
    // camera position = (0,10,-10)
    // player position = (0,0,0)
    // offset becomes (0,10,-10)
    private void Awake() {
        // Calculate the distance between the camera and the object it will follow
        offset = transform.position - target.position;
    }


    // LateUpdate() runs once every frame, but AFTER all Update() functions finish
    // and cameras often use LateUpdate so they follow the object AFTER it has moved
    private void LateUpdate() {

        // It takes the target's current position and adds the original offset
        // This keeps the camera the same distance away from the target
        Vector3 targetPosition = target.position + offset;

        // SmoothDamp gradually moves the camera toward the target position
        // and it creates smooth, natural camera movement instead of snapping instantly
        // some parameters:
        // 1) current camera position
        // 2) desired camera position
        // 3) reference to current velocity (needed for smoothing calculations)
        // 4) how long it should take to smooth toward the target
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);
    }
}
