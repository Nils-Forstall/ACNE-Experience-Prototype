using UnityEngine;

public class MuteWhenNotPlaying : MonoBehaviour
{
    private AudioSource audioSource;
    

    void Start()
    {
        audioSource = GetComponent<AudioSource>();  // Get the AudioSource component
    }

    void Update()
    {
        if (Game.Instance.getIsPlaying())
        {
            UnmuteAudio();
        }
        else
        {
            MuteAudio();
        }
    }

    void MuteAudio()
    {
        audioSource.volume = 0f;  // Mute audio
    }

    void UnmuteAudio()
    {
        audioSource.volume = 1f;  // Restore volume (adjust if needed)
    }
}
