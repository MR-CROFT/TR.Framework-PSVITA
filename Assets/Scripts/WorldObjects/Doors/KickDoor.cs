using UnityEngine;
using System.Collections;

public class KickDoor : MonoBehaviour
{
    public Transform door;
    public string openDoorAnimationName; // Nome da animação da porta inserido no Inspector
    public string playerOpenAnimationName; // Nome da animação de abertura do player
    public AudioClip doorOpenSound; // Som da porta ao abrir
    private Animator doorAnimator;
    private Animator playerAnimator;
    private AudioSource audioSource;
    private bool isPlayerInRange = false;

    void Start()
    {
        doorAnimator = door.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (doorAnimator == null)
        {
            Debug.LogError("Animator component not found on door.");
        }

        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found on this game object.");
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetButtonDown("Action"))
        {
            OpenDoor();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerAnimator = other.GetComponent<Animator>();

            if (playerAnimator == null)
            {
                Debug.LogError("Animator component not found on player.");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private void OpenDoor()
    {
        if (playerAnimator != null)
        {
            playerAnimator.Play(playerOpenAnimationName); // Toca a animação de abertura do player
        }

        if (doorAnimator != null)
        {
            doorAnimator.Play(openDoorAnimationName); // Toca a animação de abertura da porta
        }

        if (audioSource != null && doorOpenSound != null)
        {
            StartCoroutine(PlayDoorSoundWithDelay(1.0f)); // Inicia a coroutine para tocar o som com atraso
        }
    }

    private IEnumerator PlayDoorSoundWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Espera o tempo especificado
        audioSource.PlayOneShot(doorOpenSound); // Toca o som da porta abrindo
    }
}
