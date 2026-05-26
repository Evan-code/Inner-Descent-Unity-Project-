using UnityEngine;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Scene Names")]
    public string tutorialSceneName = "Tutorial";

    public void StartButton()
    {
        SceneTransitionManager.Instance.LoadSceneWithFade(tutorialSceneName);
    }

    public void ExitButton()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}