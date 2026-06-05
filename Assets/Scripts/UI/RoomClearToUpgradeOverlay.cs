using UnityEngine;

// This script opens the upgrade overlay when the room is cleared
// It listens to RoomManager's OnRoomCleared event
public class RoomClearToUpgradeOverlay : MonoBehaviour {
    // This is the script that knows when the room is cleared
    [SerializeField] private RoomManager roomManager;

    // This is the script that shows the upgrade screen
    [SerializeField] private UpgradeOverlayManager upgradeOverlayManager;

    // This prevents the upgrade screen from opening more than once
    private bool triggered = false;

    void Start() {
        // If roomManager was not dragged in, try getting it from this same object
        if (roomManager == null) {
            roomManager = GetComponent<RoomManager>();
        }

        // If RoomManager exists, listen for room clear
        if (roomManager != null) {
            roomManager.OnRoomCleared += HandleRoomCleared;
        } else {
            // Warn if there is no RoomManager
            Debug.LogWarning("RoomClearToUpgradeOverlay: No RoomManager assigned.");
        }
    }

    void OnDestroy() {
        // Stop listening when this object gets destroyed
        if (roomManager != null) {
            roomManager.OnRoomCleared -= HandleRoomCleared;
        }
    }

    private void HandleRoomCleared() {
        // If this already happened, do nothing
        if (triggered) {
            return;
        }

        // Mark as triggered so it only happens once
        triggered = true;

        // Show the upgrade overlay if it exists
        if (upgradeOverlayManager != null) {
            upgradeOverlayManager.ShowUpgradeOverlay();
        } else {
            // Warn if upgrade overlay manager is missing
            Debug.LogWarning("RoomClearToUpgradeOverlay: No UpgradeOverlayManager assigned.");
        }
    }
}