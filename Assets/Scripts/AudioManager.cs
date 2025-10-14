using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource hitSound, goalSound, bgMusic, dashSound, gameOverSound, uiClickSound;

    void Awake()
    {
        Instance = this;
        bgMusic.Play();
    }

    public void PlayHit() => hitSound.Play();
    public void PlayGoal() => goalSound.Play();
    public void PlayDash() => dashSound.Play();
    public void PlayGameOver() => gameOverSound.Play();
    public void PlayUIClick() => uiClickSound.Play();
}
