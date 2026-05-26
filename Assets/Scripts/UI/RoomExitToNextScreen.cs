using UnityEngine;

public class RoomExitToNextScene : MonoBehaviour
{
    [Header("Next Scene")]
    public string nextSceneName;

    public void GoToNextScene()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade(nextSceneName);
    }
}