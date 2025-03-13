using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AccessibilityPrototype;

public class SpatialSoundElements : MonoBehaviour
{
    // List to keep track of all sound elements
    private List<GameObject> soundElements = new List<GameObject>();

    [SerializeField]
    private Player player;

    [SerializeField]
    AudioClip left;
    [SerializeField]
    AudioClip right;
    [SerializeField]
    AudioClip forward;

    [SerializeField]
    AudioClip reset;

    [SerializeField]
    bool periodicUpdate = true;
    float timeToWait = 0.3f;
    bool playedSoundsThisStop = false;

    // Keep track of the last timestamp when the player was moving
    private float lastMoveTimestamp = 0;

    Dictionary<RelativeDirection, AudioClip> directionToClip;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting sound elements");
        directionToClip = new Dictionary<RelativeDirection, AudioClip>
        {
            { RelativeDirection.Left, left },
            { RelativeDirection.Right, right },
            { RelativeDirection.Forward, forward }
        };
        if (periodicUpdate)
        {
            StartCoroutine(PerformActionsEveryFewSeconds());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!periodicUpdate && Game.Instance.getIsPlaying())
        {
            if (player.getIsMoving())
            {
                lastMoveTimestamp = Time.time;
                playedSoundsThisStop = false;
            } else if (!playedSoundsThisStop && Time.time - lastMoveTimestamp > timeToWait)
            {
                playedSoundsThisStop = true;
                updateSoundElements();
            }
        } 
    }

    private IEnumerator PerformActionsEveryFewSeconds()
    {
        while (true)
        {
            Debug.Log("Updating sound elements");
            yield return new WaitForSeconds(2.7f);
            AudioManager.Instance.PlaySoundFromClip(reset);
            foreach (GameObject soundElement in soundElements)
        {
            Destroy(soundElement);
        }
            yield return new WaitForSeconds(0.3f);


            if (Game.Instance.getIsPlaying())
            {
                updateSoundElements();
            } else {
                continue;
            }
        }
    }

    private void updateSoundElements()
    {
        AudioManager.Instance.PlaySoundFromClip(reset);
        
        foreach (GameObject soundElement in soundElements)
        {
            Destroy(soundElement);
        }
        Debug.Log("Updating sound elements");
        
        soundElements.Clear();

        


        List<(Vector3, RelativeDirection)> soundElementPositions = player.GetOpenSpaces();
        foreach ((Vector3 position, RelativeDirection direction) in soundElementPositions)
        {
            createSoundElement(position, direction);
        }

    }

   

    private void createSoundElement(Vector3 position, RelativeDirection direction)
    {
        Debug.Log($"Creating sound element at {position} facing {direction}");
        // Create a new GameObject
        GameObject soundElement = new GameObject("SoundElement");

        // Add an AudioSource component to the GameObject
        AudioSource audioSource = soundElement.AddComponent<AudioSource>();

        // Set the AudioClip to play
        audioSource.clip = directionToClip[direction];

        // Set audio settings to be spatial
        audioSource.spatialBlend = 1f;

        // Set the position of the sound element
        soundElement.transform.position = position;

        // Set the volume of the audio clip
        audioSource.volume = 1f;

        audioSource.loop = true;

        // Play the audio clip
        audioSource.Play();

        // Add the sound element to the list
        soundElements.Add(soundElement);
    }
}