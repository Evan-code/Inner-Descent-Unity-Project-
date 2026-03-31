using UnityEngine;

// This script listens for the room being cleared
// and opens the upgrade overlay.
public class RoomClearToUpgradeOverlay : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private UpgradeOverlayManager upgradeOverlayManager;

    private bool triggered = false;

    void Start()
    {
        if (roomManager != null)
        {
            roomManager.OnRoomCleared += HandleRoomCleared;
        }
    }

    void OnDestroy()
    {
        if (roomManager != null)
        {
            roomManager.OnRoomCleared -= HandleRoomCleared;
        }
    }

    void HandleRoomCleared()
    {
        if (triggered)
            return;

        triggered = true;

        if (upgradeOverlayManager != null)
        {
            upgradeOverlayManager.ShowUpgradeOverlay();
        }
    }
}