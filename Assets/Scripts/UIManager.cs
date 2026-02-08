using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

// This class manages UI state and navigation, such as the title screen and pause menu.
public class UIManager : MonoBehaviour
{
    [Header("VR Input")]
    public UnityEngine.InputSystem.InputActionReference pauseAction;

    [Header("Pause (VR-safe)")]
    [Tooltip("Rigidbodies to freeze while paused (e.g., puck + paddles).")]
    [SerializeField] private Rigidbody[] pauseRigidbodies;

    public GameObject pauseMenu;
    public GameObject titlePanel;
    
    // This flag tracks if the game is in an "active" playable state.
    // It's used to prevent pausing from the title or game over screens.
    private bool isGameActive = false;
    private bool isPaused = false;

    void Start()
    {
        // When the scene loads, show the title screen and pause the game.
        if (titlePanel != null)
        {
            titlePanel.SetActive(true);
        }
        // In VR, we keep Time.timeScale at 1 so the XR Simulator can move the hands.
        // We use isGameActive to keep the puck and paddles from moving via their own scripts.
        Time.timeScale = 1; 
        isGameActive = false;
    }

    void OnEnable()
    {
        // Ensure the action is enabled even if the map isn't (helps in-editor + simulator).
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.Disable();
        }
    }

    void Update()
    {
        // Only listen for the pause key if the game is currently active.
        if (isGameActive)
        {
            bool keyboardPause = Input.GetKeyDown(KeyCode.Escape);
            bool vrPause = (pauseAction != null && pauseAction.action != null && pauseAction.action.triggered);

            if (keyboardPause || vrPause)
            {
                TogglePause();
            }
        }
    }

    // Allows the GameManager to control the active state of the UI.
    public void SetGameActive(bool isActive)
    {
        isGameActive = isActive;
    }

    // Called by the "Start Game" button on the title screen.
    public void StartGame()
    {
        if (titlePanel != null)
        {
            titlePanel.SetActive(false);
            Time.timeScale = 1; // Unpause the game.
            SetGameActive(true);

            // Start the background audio now that the user has interacted with the page.
            // This is required for audio to work in most web browsers.
            if (AudioManager.Instance != null && AudioManager.Instance.bgMusic != null)
            {
                if (!AudioManager.Instance.bgMusic.isPlaying)
                {
                    AudioManager.Instance.bgMusic.Play();
                }
            }
        }
    }

    // Toggles the pause state of the game.
    public void TogglePause()
    {
        if (pauseMenu != null)
        {
            isPaused = !pauseMenu.activeSelf;
            pauseMenu.SetActive(isPaused);
            
            // VR-safe pause: keep timeScale at 1 so XR hands + UI still work.
            // Instead, freeze gameplay rigidbodies (puck/paddles/etc).
            SetPausedBodies(isPaused);

            // When unpausing, we clear the EventSystem's selected object.
            // This prevents buttons from getting visually "stuck" in their pressed state.
            if (!isPaused)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    private void SetPausedBodies(bool paused)
    {
        if (pauseRigidbodies == null) return;

        for (int i = 0; i < pauseRigidbodies.Length; i++)
        {
            var rb = pauseRigidbodies[i];
            if (rb == null) continue;

            if (paused)
            {
                // Stop motion immediately, then make kinematic so physics won't move it.
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            else
            {
                // Resume physics. We keep velocities at zero to avoid "launching" on unpause.
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    // This is the public method called by the restart buttons.
    public void RestartGame()
    {
        // We start a coroutine so we can add a delay, allowing the click sound to play
        // before the scene gets reloaded and destroys the AudioManager.
        StartCoroutine(RestartGameRoutine());
    }

    // This coroutine handles the actual restart sequence.
    private IEnumerator RestartGameRoutine()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        // We must use WaitForSecondsRealtime because Time.timeScale might be 0,
        // which would cause a normal WaitForSeconds to wait forever.
        yield return new WaitForSecondsRealtime(0.2f);

        // Reset time scale to normal before reloading the scene.
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
