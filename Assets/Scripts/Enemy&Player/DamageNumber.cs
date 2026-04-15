using TMPro;
using UnityEngine;

// This script makes a damage number float upward and disappear.
public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float lifetime = 0.8f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Float upward over time
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Face the camera
        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }

    public void SetDamage(int amount)
    {
        if (damageText != null)
        {
            damageText.text = amount.ToString();
        }
    }
}
