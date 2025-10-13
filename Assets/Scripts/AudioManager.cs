using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource hitSound, goalSound, bgMusic;

    void Awake()
    {
        Instance = this;
        bgMusic.Play();
    }

    public void PlayHit() => hitSound.Play();
    public void PlayGoal() => goalSound.Play();
}
