using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public TextAsset soundSettingsFile;
    private float lastGuidanceSoundTime;
    private const string DEFAULT_JSON = @"
{
    ""sounds"": [
        {
            ""soundName"": ""Walk Forward"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": true,
            ""priority"": 128,
            ""volume"": 0.7,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 1.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Walk Backward"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": true,
            ""priority"": 128,
            ""volume"": 0.7,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 1.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Turn Left"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": false,
            ""priority"": 128,
            ""volume"": 0.6,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 0.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Turn Right"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": false,
            ""priority"": 128,
            ""volume"": 0.6,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 0.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Villain Near"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": true,
            ""priority"": 128,
            ""volume"": 0.8,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 1.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Villain Can See You"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": false,
            ""priority"": 100,
            ""volume"": 1.0,
            ""pitch"": 1.2,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 1.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Collide With Wall"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": false,
            ""priority"": 128,
            ""volume"": 0.5,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 1.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Linear""
        },
        {
            ""soundName"": ""Death"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": false,
            ""priority"": 0,
            ""volume"": 1.0,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 0.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Win"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": false,
            ""priority"": 128,
            ""volume"": 0.9,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 0.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Distance From Solution Path"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": true,
            ""priority"": 128,
            ""volume"": 0.4,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 1.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Soundtrack"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": true,
            ""loop"": true,
            ""priority"": 0,
            ""volume"": 0.5,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 0.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Player Guidance - Turn Left"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": false,
            ""priority"": 128,
            ""volume"": 0.7,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 0.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Player Guidance - Rotate Right"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": false,
            ""priority"": 128,
            ""volume"": 0.7,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 0.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Player Guidance - Move Forward"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": false,
            ""priority"": 128,
            ""volume"": 0.7,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 0.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        },
        {
            ""soundName"": ""Player Guidance - Move Backward"",
            ""mute"": false,
            ""spatialize"": false,
            ""spatializePostEffect"": false,
            ""bypassEffects"": false,
            ""bypassListenerEffects"": false,
            ""bypassReverbZones"": false,
            ""playOnAwake"": false,
            ""loop"": false,
            ""priority"": 128,
            ""volume"": 0.7,
            ""pitch"": 1.0,
            ""stereoPan"": 0.0,
            ""spatialBlend"": 0.0,
            ""reverbZoneMix"": 1.0,
            ""dopplerLevel"": 1.0,
            ""spread"": 0.0,
            ""minDistance"": 1.0,
            ""maxDistance"": 500.0,
            ""rolloffMode"": ""Logarithmic""
        }
    ]
}
";


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
        public string soundName;           // "Walk Forward", "Villain Near", etc.

        public bool mute;                  // audioSource.mute
        public bool spatialize;            // audioSource.spatialize
        public bool spatializePostEffect;  // audioSource.spatializePostEffects
        public bool bypassEffects;
        public bool bypassListenerEffects;
        public bool bypassReverbZones;
        public bool playOnAwake;
        public bool loop;
        public int priority;
        public float volume;

        // If your JSON uses a single float for pitch, do this:
        public float pitch;                // audioSource.pitch

        // If you?d prefer a random pitch range, you can revert to an array:
        // public float[] pitch;

        public float stereoPan;            // audioSource.panStereo
        public float spatialBlend;         // audioSource.spatialBlend
        public float reverbZoneMix;        // audioSource.reverbZoneMix
        public float dopplerLevel;         // audioSource.dopplerLevel
        public float spread;               // audioSource.spread
        public float minDistance;          // audioSource.minDistance
        public float maxDistance;          // audioSource.maxDistance

        // This is a string in JSON that you'll convert to the AudioRolloffMode enum
        public string rolloffMode;         // "Logarithmic", "Linear", or "Custom"
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

public void LoadSoundSettings()
{
    if (soundSettingsFile == null)
    {
        Debug.LogError("No sound settings file assigned!");
        return;
    }

    // Read the text from the assigned TextAsset
    string json = soundSettingsFile.text;

    SoundSettingsList settingsList = JsonUtility.FromJson<SoundSettingsList>(json);

    if (settingsList == null || settingsList.sounds == null)
    {
        Debug.LogError("Failed to parse sound settings JSON.");
        return;
    }

    // Clear and populate
    _soundSettings.Clear();
    foreach (SoundSettings sound in settingsList.sounds)
    {
        _soundSettings[sound.soundName] = sound;
    }

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

    public void PlaySoundOn(GameObject target, string soundName)
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

        // Check if the target already has an AudioSource for this sound
        AudioSource audioSource;
        if (!_audioSources.ContainsKey(soundName) || !_audioSources[soundName])
        {
            audioSource = target.AddComponent<AudioSource>(); // Attach to the target (enemy)
            _audioSources[soundName] = audioSource;  // Store reference in the dictionary
        }
        else
        {
            audioSource = _audioSources[soundName];
        }

        audioSource.clip = _audioClipDict[soundName];

        // Apply settings from the JSON
        ApplyAISoundSettings(soundName, audioSource);

        // Play the sound at the enemy's position
        audioSource.Play();
    }


    private void ApplyAISoundSettings(string soundName, AudioSource audioSource)
    {
        if (!_soundSettings.ContainsKey(soundName)) return;

        SoundSettings settings = _soundSettings[soundName];

        // --- Simple fields ---
        audioSource.mute = settings.mute;
        audioSource.spatialize = settings.spatialize;
        audioSource.spatializePostEffects = settings.spatializePostEffect;
        audioSource.bypassEffects = settings.bypassEffects;
        audioSource.bypassListenerEffects = settings.bypassListenerEffects;
        audioSource.bypassReverbZones = settings.bypassReverbZones;
        audioSource.playOnAwake = settings.playOnAwake;
        audioSource.loop = settings.loop;
        audioSource.priority = settings.priority;
        audioSource.volume = settings.volume;

        // If pitch is a single float:
        audioSource.pitch = settings.pitch;

        // If you keep pitch as an array (random range):
        // if (settings.pitch != null && settings.pitch.Length == 2)
        // {
        //     audioSource.pitch = Random.Range(settings.pitch[0], settings.pitch[1]);
        // }
        // else
        // {
        //     audioSource.pitch = settings.pitch[0];
        // }

        audioSource.panStereo = settings.stereoPan;
        audioSource.spatialBlend = settings.spatialBlend;
        audioSource.reverbZoneMix = settings.reverbZoneMix;
        audioSource.dopplerLevel = settings.dopplerLevel;
        audioSource.spread = settings.spread;
        audioSource.minDistance = settings.minDistance;
        audioSource.maxDistance = settings.maxDistance;

        // Convert the string rolloffMode to the Unity enum
        switch (settings.rolloffMode)
        {
            case "Linear":
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                break;
            case "Custom":
                audioSource.rolloffMode = AudioRolloffMode.Custom;
                break;
            default:
                audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                break;
        }
    }

    public void PlayGuidanceSounds(Maze maze, int2 playerCoords, Transform playerTransform)
    {
        // Check if enough time has passed since the last guidance sound
        if (Time.time - lastGuidanceSoundTime < 1f)
        {
            // Debug.Log("Not enough time has passed since the last guidance sound.");
            return; // Not enough time has passed, do nothing
        }

        // Update the last guidance sound time
        lastGuidanceSoundTime = Time.time;

        // Determine the player's facing direction
        Vector3 forward = playerTransform.forward;

        // Determine the cardinal direction the player is facing
        string cardinalDirection = GetCardinalDirection(forward);

        // Calculate the left and right cells based on the cardinal direction
        int2 leftCell = playerCoords;
        int2 rightCell = playerCoords;

        switch (cardinalDirection)
        {
            case "North":
                leftCell += new int2(-1, 0); // West
                rightCell += new int2(1, 0); // East
                break;
            case "South":
                leftCell += new int2(1, 0); // East
                rightCell += new int2(-1, 0); // West
                break;
            case "East":
                leftCell += new int2(0, 1); // North
                rightCell += new int2(0, -1); // South
                break;
            case "West":
                leftCell += new int2(0, -1); // South
                rightCell += new int2(0, 1); // North
                break;
        }

        Debug.Log($"Player Coords: {playerCoords}, Left Cell: {leftCell}, Right Cell: {rightCell}");
        Debug.Log($"Player Forward: {forward}, Cardinal Direction: {cardinalDirection}");

        if (!maze.WallBetweenCells(playerCoords, leftCell))
        {
            PlaySound("Whoosh Left");
            Debug.Log("Playing sound: Whoosh Left");
        }
        else
        {
            Debug.Log("Wall detected to the left.");
        }

        if (!maze.WallBetweenCells(playerCoords, rightCell))
        {
            PlaySound("Whoosh Right");
            Debug.Log("Playing sound: Whoosh Right");
        }
        else
        {
            Debug.Log("Wall detected to the right.");
        }
    }

    private string GetCardinalDirection(Vector3 forward)
    {
        Vector3[] directions = new Vector3[]
        {
            new Vector3(0, 0, 1), // North
            new Vector3(1, 0, 0), // East
            new Vector3(0, 0, -1), // South
            new Vector3(-1, 0, 0), // West
            new Vector3(0.7071f, 0, 0.7071f), // Northeast
            new Vector3(0.7071f, 0, -0.7071f), // Southeast
            new Vector3(-0.7071f, 0, -0.7071f), // Southwest
            new Vector3(-0.7071f, 0, 0.7071f) // Northwest
        };

        string[] directionNames = new string[]
        {
            "North",
            "East",
            "South",
            "West",
            "Northeast",
            "Southeast",
            "Southwest",
            "Northwest"
        };

        float maxDot = -1f;
        int bestMatch = -1;

        for (int i = 0; i < directions.Length; i++)
        {
            float dot = Vector3.Dot(forward.normalized, directions[i]);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestMatch = i;
            }
        }

        return directionNames[bestMatch];
    }

    // public void PlayGuidanceSounds(Maze maze, int2 playerCoords, Transform playerTransform)
    // {
    //     // Check if enough time has passed since the last guidance sound
    //     if (Time.time - lastGuidanceSoundTime < 1f)
    //     {
    //         // Debug.Log("Not enough time has passed since the last guidance sound.");
    //         return; // Not enough time has passed, do nothing
    //     }

    //     // Update the last guidance sound time
    //     lastGuidanceSoundTime = Time.time;

    //     // Determine the player's facing direction
    //     Vector3 forward = playerTransform.forward;
    //     Vector3 right = playerTransform.right;

    //     // Calculate the left and right cells based on the player's facing direction
    //     int2 rightCell = playerCoords + GetDirectionVector(-right);
    //     int2 leftCell = playerCoords + GetDirectionVector(right);


    //     Debug.Log($"Player Coords: {playerCoords}, Left Cell: {leftCell}, Right Cell: {rightCell}");
    //     Debug.Log($"Player Forward: {forward}, Player Right: {right}");

    //     if (!maze.WallBetweenCells(playerCoords, leftCell))
    //     {
    //         PlaySound("Whoosh Left");
    //         Debug.Log("Playing sound: Whoosh Left");
    //     }
    //     else
    //     {
    //         Debug.Log("Wall detected to the left.");
    //     }

    //     if (!maze.WallBetweenCells(playerCoords, rightCell))
    //     {
    //         PlaySound("Whoosh Right");
    //         Debug.Log("Playing sound: Whoosh Right");
    //     }
    //     else
    //     {
    //         Debug.Log("Wall detected to the right.");
    //     }
    // }

    // private int2 GetDirectionVector(Vector3 direction)
    // {
    //     // Determine the closest grid direction
    //     if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
    //     {
    //         return new int2(Mathf.RoundToInt(direction.x), 0);
    //     }
    //     else
    //     {
    //         return new int2(0, Mathf.RoundToInt(direction.z));
    //     }
    // }

    public void StopAllSounds()
    {
        foreach (var source in _audioSources.Values)
        {
            source.Stop();
        }
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
