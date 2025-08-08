using System;
using UnityEngine;

[Serializable]
public class SettingsData
{
    [Header("Brightness Settings")]
    public float brightness = 0f;
    
    [Header("Audio Settings")]
    public float masterVolume = 1f;
    public float soundVolume = 1f;
    public float musicVolume = 1f;
    
    [Header("Video Settings")]
    public int resolutionIndex = 0;
    public bool fullscreen = true;
    public bool vSync = false;
    public int particleEffectsQuality = 1;
    public int blurQuality = 1;
    
    [Header("Game Settings")]
    public int languageIndex = 0;
    
    [Header("Control Settings")]
    public bool controllerEnabled = true;
    public bool keyboardEnabled = true;
    
    public SettingsData()
    {
        // Default values
        brightness = 0f;
        masterVolume = 1f;
        soundVolume = 1f;
        musicVolume = 1f;
        resolutionIndex = 0;
        fullscreen = true;
        vSync = false;
        particleEffectsQuality = 1;
        blurQuality = 1;
        languageIndex = 0;
        controllerEnabled = true;
        keyboardEnabled = true;
    }
} 