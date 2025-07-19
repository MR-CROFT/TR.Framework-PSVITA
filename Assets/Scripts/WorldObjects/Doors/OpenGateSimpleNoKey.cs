using UnityEngine;
using System.Collections;

public class OpenGateSimpleNoKey : MonoBehaviour
{
    public Transform door;
    public string openDoorAnimationName;
    public string playerOpenAnimationName;
    public AudioClip doorOpenSound;
    public AudioClip playerOpenSound; // Novo: Som do player durante a animação
    public Transform playerTargetPosition; // Novo: Posição onde o player deve estar

    private Animator doorAnimator;
    private Animator playerAnimator;
    private AudioSource audioSource;
    private AudioSource playerAudioSource; // Novo: Fonte de áudio para o player
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
            StartCoroutine(MovePlayerAndOpenDoor());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerAnimator = other.GetComponent<Animator>();
            playerAudioSource = other.GetComponent<AudioSource>(); // Novo: Pegando o AudioSource do player

            if (playerAnimator == null)
            {
                Debug.LogError("Animator component not found on player.");
            }

            if (playerAudioSource == null)
            {
                Debug.LogError("AudioSource component not found on player.");
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

    private IEnumerator MovePlayerAndOpenDoor()
    {
        if (playerTargetPosition != null)
        {
            // Movimenta o player para a posição-alvo antes de iniciar a animação
            float duration = 0.25f; // Tempo para o player chegar à posição
            float elapsedTime = 0f;
            Vector3 initialPosition = playerAnimator.transform.position;

            while (elapsedTime < duration)
            {
                playerAnimator.transform.position = Vector3.Lerp(initialPosition, playerTargetPosition.position, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            playerAnimator.transform.position = playerTargetPosition.position; // Garante que a posição final seja precisa
        }

        // Toca a animação e som do player
        if (playerAnimator != null)
        {
            playerAnimator.Play(playerOpenAnimationName);

            if (playerAudioSource != null && playerOpenSound != null)
            {
                playerAudioSource.PlayOneShot(playerOpenSound);
            }
        }

        // Toca a animação da porta
        if (doorAnimator != null)
        {
            doorAnimator.Play(openDoorAnimationName);
        }

        // Inicia a coroutine para tocar o som da porta com atraso
        if (audioSource != null && doorOpenSound != null)
        {
            StartCoroutine(PlayDoorSoundWithDelay(1.0f));
        }
    }

    private IEnumerator PlayDoorSoundWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(doorOpenSound);
    }
}