using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Audio Mixers")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioMixer musicMixer;
    [SerializeField] private AudioMixer sfxMixer;
    
    [Header("Audio Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float sfxVolume = 1f;
    
    [Header("Legacy Audio System")]
    [SerializeField] private AudioSource levelMusic, gameOverMusic, winMusic;
    [SerializeField] private AudioSource[] sfx;
    
    public static AudioManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        LoadAudioSettings();
    }
    
    #region Volume Control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        
        if (masterMixer != null)
        {
            float dbValue = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            masterMixer.SetFloat("MasterVolume", dbValue);
        }
        
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        
        if (musicMixer != null)
        {
            float dbValue = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            musicMixer.SetFloat("MusicVolume", dbValue);
        }
        
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
        
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        
        if (sfxMixer != null)
        {
            float dbValue = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            sfxMixer.SetFloat("SFXVolume", dbValue);
        }
        
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * masterVolume;
        }
        
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    public float GetMasterVolume()
    {
        return masterVolume;
    }
    
    public float GetMusicVolume()
    {
        return musicVolume;
    }
    
    public float GetSFXVolume()
    {
        return sfxVolume;
    }
    #endregion
    
    #region Audio Playback
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }
    }
    
    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxSource != null && sfxClip != null)
        {
            sfxSource.PlayOneShot(sfxClip, sfxVolume * masterVolume);
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }
    
    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }
    
    // Legacy Audio System Methods (for backward compatibility)
    public void PlayGameOver()
    {
        if (levelMusic != null)
            levelMusic.Stop();
        if (gameOverMusic != null)
            gameOverMusic.Play();
    }
    
    public void PlayLevelWin()
    {
        if (levelMusic != null)
            levelMusic.Stop();
        if (winMusic != null)
            winMusic.Play();
    }
    
    /// <summary>
    /// Plays SFX using int index (legacy system - kept for backward compatibility)
    /// </summary>
    /// <param name="sfxIndex">Index of SFX in the sfx array</param>
    public void PlaySFX(int sfxIndex)
    {
        if (sfxIndex >= 0 && sfxIndex < sfx.Length && sfx[sfxIndex] != null)
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
    #endregion
    
    #region Settings Management
    private void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }
    
    public void ResetAudioSettings()
    {
        SetMasterVolume(1f);
        SetMusicVolume(1f);
        SetSFXVolume(1f);
    }
    #endregion
}
