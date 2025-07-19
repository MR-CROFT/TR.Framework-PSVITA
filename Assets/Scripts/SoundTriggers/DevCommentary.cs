using UnityEngine;

public class DevCommentary : MonoBehaviour
{
    public AudioClip dialogueClip;  // Assign the dialogue audio clip in the Inspector
    private AudioSource audioSource;
    private bool isPlaying = false; // Variable to track if the sound is already playing
    private AudioSource[] allAudioSources; // Array to store all audio sources in the scene

    void Start()
    {
        // Add an AudioSource component to the GameObject if it doesn't already have one
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = dialogueClip;

        // Get all audio sources in the scene
        allAudioSources = FindObjectsOfType<AudioSource>();
    }

    // Detect when the player enters the trigger
    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the tag "Player"
        if (other.CompareTag("Player"))
        {
            // Check if the audio is already playing
            if (!isPlaying)
            {
                // Lower the volume of all other audio sources except this one
                MuteOtherAudioSources(true);

                // Play the dialogue sound
                audioSource.Play();
                isPlaying = true;

                // Restore volumes after the audio is finished playing
                Invoke(nameof(RestoreAudioSources), dialogueClip.length);

                // Destroy the game object after restoring audio sources
                Invoke(nameof(DestroyAfterRestore), dialogueClip.length + 1.0f);
            }
        }
    }

    // Function to mute/unmute all other audio sources in the scene
    private void MuteOtherAudioSources(bool mute)
    {
        foreach (var source in allAudioSources)
        {
            // Check if the audio source is not the one on this game object
            if (source != audioSource)
            {
                source.volume = mute ? 0.1f : 1.0f; // Lower to 10% or restore to 100%
            }
        }
    }

    // Function to restore the original volumes of all audio sources
    private void RestoreAudioSources()
    {
        MuteOtherAudioSources(false);
    }

    // Function to destroy the game object after audio restoration
    private void DestroyAfterRestore()
    {
        Destroy(gameObject);
    }
}
