using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("AudioManager is null! Ensure there is an AudioManager in the scene.");
            }
            return _instance;
        }
    }

    [SerializeField] public AudioClip[] audioClips; // Assign clips in Inspector
    private Dictionary<string, AudioClip> _audioClipDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioSource> _audioSources = new Dictionary<string, AudioSource>();
    private Dictionary<string, SoundSettings> _soundSettings = new Dictionary<string, SoundSettings>();

    [System.Serializable]
    public class SoundSettings
    {
        public string soundName;  // Added this field to map settings correctly
        public float volume;
        public bool loop;
        public float[] pitch; // Can be a range
    }

    [System.Serializable]
    public class SoundSettingsList
    {
        public SoundSettings[] sounds;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Store audio clips in dictionary
        foreach (AudioClip clip in audioClips)
        {
            _audioClipDict[clip.name] = clip;
        }

        LoadSoundSettings();
    }

    private void Update(){
        UpdateSoundSettings();
    }

    private void LoadSoundSettings()
    {
        string jsonString = @"
        {
            ""sounds"": [
                { ""soundName"": ""footsteps-01"",""description"": ""Walking forward"", ""volume"": 4.0, ""loop"": true, ""pitch"": [0.8, 1.2] },
                { ""soundName"": ""footsteps-02"", ""description"": ""Walking backward"", ""volume"": 4.0, ""loop"": true, ""pitch"": [1.0, 1.0] },
                { ""soundName"": ""Ding"", ""description"": ""Start game"", ""volume"": 11.0, ""loop"": false, ""pitch"": [0.9, 1.1] },
                { ""soundName"": ""Tank Movement left"", ""description"": ""Turning toward your left"", ""volume"": 11.0, ""loop"": true, ""pitch"": [1.0, 1.0] },
                { ""soundName"": ""Tank Movement right"", ""description"": ""Turning toward your right"", ""volume"": 11.0, ""loop"": true, ""pitch"": [1.0, 1.0] },
                { ""soundName"": ""Cowbell 2"", ""description"": ""Colliding with the wall"", ""volume"": 20.0, ""loop"": false, ""pitch"": [1.0, 1.0] },
                { ""soundName"": ""Alarm"", ""description"": ""Enemy can see you"", ""volume"": 20.0, ""loop"": true, ""pitch"": [1.0, 1.0] },
                { ""soundName"": ""Blue Box v2"", ""description"": ""Enemy is near you"", ""volume"": 20.0, ""loop"": true, ""pitch"": [1.0, 1.0] },
                { ""soundName"": ""Death Sound"", ""description"": ""Losing the game"", ""volume"": 20.0, ""loop"": false, ""pitch"": [1.0, 1.0] }
                
            ]
        }";

        // { ""soundName"": ""soundtrack"", ""description"": ""Game soundtrack"", ""volume"": 20.0, ""loop"": true, ""pitch"": [1.0, 1.0] }

        SoundSettingsList settingsList = JsonUtility.FromJson<SoundSettingsList>(jsonString);

        if (settingsList == null || settingsList.sounds == null)
        {
            Debug.LogError("Failed to parse sound settings JSON.");
            return;
        }

        foreach (SoundSettings sound in settingsList.sounds)
        {
            _soundSettings[sound.soundName] = sound;
        }

        // Apply updates to currently playing sounds
        UpdateSoundSettings();
    }

    /// <summary>
    /// Plays a sound using AI-configured settings.
    /// </summary>
    public void PlaySound(string soundName)
    {
        if (!_soundSettings.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound settings for '{soundName}' not found.");
            return;
        }

        if (!_audioClipDict.ContainsKey(soundName))
        {
            Debug.LogWarning($"AudioClip '{soundName}' not found in assigned clips.");
            return;
        }

        if (!_audioSources.ContainsKey(soundName))
        {
            GameObject soundObject = new GameObject("Sound_" + soundName);
            soundObject.transform.parent = transform;
            AudioSource source = soundObject.AddComponent<AudioSource>();
            _audioSources[soundName] = source;
        }

        AudioSource audioSource = _audioSources[soundName];
        audioSource.clip = _audioClipDict[soundName];

        ApplyAISoundSettings(soundName, audioSource);
        audioSource.Play();
    }

    private void UpdateSoundSettings()
    {
        foreach (var kvp in _audioSources)
        {
            string soundName = kvp.Key;
            AudioSource source = kvp.Value;

            if (_soundSettings.ContainsKey(soundName))
            {
                ApplyAISoundSettings(soundName, source);
            }
        }
    }

    private void ApplyAISoundSettings(string soundName, AudioSource audioSource)
    {
        if (!_soundSettings.ContainsKey(soundName)) return;

        SoundSettings settings = _soundSettings[soundName];

        audioSource.volume = settings.volume;
        audioSource.loop = settings.loop;

        // Apply random pitch if a range is given
        audioSource.pitch = (settings.pitch.Length == 2) ?
            Random.Range(settings.pitch[0], settings.pitch[1]) : settings.pitch[0];
    }



    /// <summary>
    /// Stops a specific sound.
    /// </summary>
    public void StopSound(string soundName)
    {
        if (_audioSources.ContainsKey(soundName))
        {
            _audioSources[soundName].Stop();
        }
    }
}
