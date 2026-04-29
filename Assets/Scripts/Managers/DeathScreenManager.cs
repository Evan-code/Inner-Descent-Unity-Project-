using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject deathScreen;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private Health playerHealth;

    void Start()
    {
        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.OnDied += ShowDeathScreen;
            }
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDied -= ShowDeathScreen;
        }
    }

    private void ShowDeathScreen()
    {
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void RestartRoom()
    {
        Time.timeScale = 1f;

        PlayerRunData.ResetRun();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartOver()
    {
        Time.timeScale = 1f;

        PlayerRunData.ResetRun();

        SceneManager.LoadScene(mainMenuSceneName);
    }
}