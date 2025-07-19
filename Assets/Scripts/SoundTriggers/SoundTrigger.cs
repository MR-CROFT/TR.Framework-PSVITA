using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    public AudioClip dialogueClip;  // Assign the dialogue audio clip in the Inspector
    private AudioSource audioSource;
    private bool isPlaying = false; // Variable to track if the sound is already playing

    void Start()
    {
        // Add an AudioSource component to the GameObject if it doesn't already have one
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = dialogueClip;
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
                // Play the dialogue sound
                audioSource.Play();
                isPlaying = true;

                // Destroy the game object after the sound plays with a 1-second delay
                Destroy(gameObject, dialogueClip.length + 0.5f);
            }
        }
    }
}
