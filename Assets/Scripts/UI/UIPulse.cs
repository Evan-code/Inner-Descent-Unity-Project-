using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Needed for UI elements like Image

// This script makes a UI image fade in and out (pulse effect)
public class UIPulse : MonoBehaviour
{
    public float minAlpha = 0.25f; // Lowest transparency (more see-through)
    public float maxAlpha = 0.65f; // Highest transparency (less see-through)
    public float pulseSpeed = 1.5f; // How fast it pulses

    private Image img;        // The UI Image component
    private Color baseColor;  // Original color of the image

    void Start()
    {
        // Get the Image component on this object
        img = GetComponent<Image>();

        // Save its original color
        baseColor = img.color;
    }

    void Update()
    {
        // Create a smooth up-and-down value using sine wave
        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) / 2f;

        // Use that value to smoothly change between min and max alpha
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        // Copy the original color
        Color c = baseColor;

        // Change only the transparency (alpha)
        c.a = alpha;

        // Apply the new color back to the image
        img.color = c;
    }
}
