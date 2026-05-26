using UnityEngine;

public class TutorialToGame : MonoBehaviour
{
    [Header("Next Scene")]
    public string nextSceneName = "Square Room";

    private bool hasPressed = false;

    void Update()
    {
        if (hasPressed) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasPressed = true;
            SceneTransitionManager.Instance.LoadSceneWithFade(nextSceneName);
        }
    }
}