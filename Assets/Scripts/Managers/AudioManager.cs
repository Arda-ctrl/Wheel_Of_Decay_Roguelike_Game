using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource levelMusic, gameOverMusic, winMusic;
    public AudioSource[] sfx;
    void Awake()
    {
        Instance = this;
    }
    public void PlayGameOver()
    {
        levelMusic.Stop();

        gameOverMusic.Play();
    }
    public void PlayLevelWin()
    {
        levelMusic.Stop();

        winMusic.Play();
    }
    public void PlaySFX(int sfxToPlay)
    {
        sfx[sfxToPlay].Stop();
        sfx[sfxToPlay].Play();
    }
}
