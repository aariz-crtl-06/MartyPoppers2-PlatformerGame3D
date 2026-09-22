using UnityEngine;

public class PauserManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject confirmExitPanel;

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    // Stash the gameplay cursor state so we can restore it exactly on resume
    private CursorLockMode _previousLockState;
    private bool _previousCursorVisible;

    private void Awake()
    {
        // Make sure we start unpaused and both panels hidden
        Time.timeScale = 1f;
        IsPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (confirmExitPanel != null) confirmExitPanel.SetActive(false);
    }

    private void Update()
    {
        // This still runs even when Time.timeScale = 0, since Update()
        // isn't affected by timeScale — only time-based math is.
        if (Input.GetKeyDown(pauseKey))
        {
            if (!IsPaused)
            {
                Pause();
            }
            else if (confirmExitPanel != null && confirmExitPanel.activeSelf)
            {
                // Escape while on the confirm screen backs out to the pause menu
                CancelExit();
            }
            else
            {
                Resume();
            }
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        // Save current cursor state, then unlock/show it for menu use
        _previousLockState = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (confirmExitPanel != null) confirmExitPanel.SetActive(false);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        // Restore whatever the cursor state was before pausing
        Cursor.lockState = _previousLockState;
        Cursor.visible = _previousCursorVisible;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (confirmExitPanel != null) confirmExitPanel.SetActive(false);
    }

    // Hook this to the "Exit" button on the pause panel
    public void RequestExit()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (confirmExitPanel != null) confirmExitPanel.SetActive(true);
    }

    // Hook this to "No / Cancel" on the confirm panel
    public void CancelExit()
    {
        if (confirmExitPanel != null) confirmExitPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    // Hook this to "Yes / Confirm Exit" on the confirm panel
    public void ConfirmExit()
    {
        // Time.timeScale must be reset before quitting/loading a menu scene,
        // otherwise the next scene can load already paused.
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}