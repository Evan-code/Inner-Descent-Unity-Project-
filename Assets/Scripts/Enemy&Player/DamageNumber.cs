using TMPro;
using UnityEngine;

// This script controls the floating damage number
// It makes the number float upward, face the camera, and disappear
public class DamageNumber : MonoBehaviour {
    // This is the text that shows the damage number
    [SerializeField] private TMP_Text damageText;

    // This controls how fast the number floats upward
    [SerializeField] private float floatSpeed = 1.5f;

    // This controls how long the number stays before deleting itself
    [SerializeField] private float lifetime = 0.8f;

    // This stores the main camera so the number can face it
    private Camera mainCamera;

    void Start() {
        // Finds the main camera
        mainCamera = Camera.main;

        // Deletes this damage number after its lifetime ends
        Destroy(gameObject, lifetime);
    }

    void Update() {
        // Moves the damage number upward every frame
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Checks if the camera exists
        if (mainCamera != null) {
            // Makes the text face the same direction as the camera
            transform.forward = mainCamera.transform.forward;
        }
    }

    public void SetDamage(int amount) {
        // Checks if the text object exists before changing it
        if (damageText != null) {
            // Converts the number into text and shows it
            damageText.text = amount.ToString();
        }
    }
}