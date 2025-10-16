using UnityEngine;

// This class manages all audio playback for the game.
// It uses a singleton pattern so it can be easily accessed from any other script.
public class AudioManager : MonoBehaviour
{
    // Static instance of the AudioManager to be accessed from anywhere.
    public static AudioManager Instance;

    public AudioSource hitSound;
    public AudioSource goalSound;
    public AudioSource bgMusic;
    public AudioSource dashSound;
    public AudioSource gameOverSound;
    public AudioSource uiClickSound;

    void Awake()
    {
        // Set up the singleton instance.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // If an instance already exists, destroy this one.
            Destroy(gameObject);
            return;
        }
        
        if (bgMusic != null)
        {
            bgMusic.Play();
        }
    }

    // --- Public Methods to Play Sounds ---

    public void PlayHit() => hitSound?.Play();
    public void PlayGoal() => goalSound?.Play();
    public void PlayDash() => dashSound?.Play();
    public void PlayGameOver() => gameOverSound?.Play();
    public void PlayUIClick() => uiClickSound?.Play();
}
