// These libraries give us access to Unity features.
// Similar to importing packages in Java.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script makes a UI element move up and down smoothly
// (like floating in the air).
public class UIFloat : MonoBehaviour
{
    // How far the object moves up and down (in pixels/units)
    public float floatAmount = 10f;

    // How fast the floating motion happens
    public float floatSpeed = 1f;

    // This will store the original starting position of the object
    private Vector3 startPos;

    // Start() runs once when the object first appears in the scene
    void Start()
    {
        // Save the object's starting position so we know where to float around
        startPos = transform.localPosition;
    }

    // Update() runs every frame (usually ~60 times per second)
    void Update()
    {
        // Mathf.Sin creates a smooth wave that goes between -1 and 1.
        // Multiplying it by floatAmount controls how far the object moves.
        // Time.unscaledTime keeps the animation running even if the game is paused.
        float yOffset = Mathf.Sin(Time.unscaledTime * floatSpeed) * floatAmount;

        // Move the object up and down relative to its starting position
        transform.localPosition = startPos + new Vector3(0f, yOffset, 0f);
    }
}
