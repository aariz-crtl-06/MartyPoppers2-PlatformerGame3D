using UnityEngine;
using UnityEngine.InputSystem;

public class BossFightManager : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private GameObject tryAgainMenu;

    void Start()
    {
        if (playerCombat == null)
            playerCombat = GetComponent<PlayerCombat>();

        tryAgainMenu.SetActive(false);
        playerCombat.OnPlayerDeath += ShowTryAgainMenu;
    }

    private void ShowTryAgainMenu()
    {
        tryAgainMenu.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnTryAgainPressed()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void OnDestroy()
    {
        if (playerCombat != null)
            playerCombat.OnPlayerDeath -= ShowTryAgainMenu;
    }
}