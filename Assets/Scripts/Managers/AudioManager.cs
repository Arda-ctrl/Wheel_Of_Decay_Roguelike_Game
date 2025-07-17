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
    
    /// <summary>
    /// Plays SFX using AudioClip (new system)
    /// </summary>
    /// <param name="sfxClip">AudioClip to play</param>
    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxClip == null) return;
        
        // Find an available AudioSource or create one
        AudioSource availableSource = GetAvailableAudioSource();
        if (availableSource != null)
        {
            availableSource.clip = sfxClip;
            availableSource.Play();
        }
    }
    
    /// <summary>
    /// Plays SFX using int index (legacy system - kept for backward compatibility)
    /// </summary>
    /// <param name="sfxIndex">Index of SFX in the sfx array</param>
    public void PlaySFX(int sfxIndex)
    {
        if (sfxIndex >= 0 && sfxIndex < sfx.Length)
        {
            sfx[sfxIndex].Stop();
            sfx[sfxIndex].Play();
        }
    }
    
    /// <summary>
    /// Gets an available AudioSource for playing SFX
    /// </summary>
    /// <returns>Available AudioSource or null if none available</returns>
    private AudioSource GetAvailableAudioSource()
    {
        // First try to find an AudioSource that's not playing
        foreach (AudioSource source in sfx)
        {
            if (source != null && !source.isPlaying)
            {
                return source;
            }
        }
        
        // If all are playing, use the first one (will interrupt current sound)
        if (sfx.Length > 0 && sfx[0] != null)
        {
            return sfx[0];
        }
        
        return null;
    }
}
