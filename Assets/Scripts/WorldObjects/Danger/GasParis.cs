using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GasParis : MonoBehaviour {

    public Vector3 startPosition = new Vector3(0, -11, 0);
    public Vector3 targetPosition = new Vector3(0, 10, 0);
    public float duration = 300f; // 5 minutes = 300 seconds
    private float startTime;

    private bool isPlayerInTrigger = false;
    private PlayerStats playerStats;

    public AudioSource audioSource; // Variável para o SFX
    private float audioTimer = 0f; // Temporizador para controlar o tempo entre os sons
    public float audioInterval = 20f; // Intervalo de 20 segundos para o som

    void Start()
    {
        startTime = Time.time;
        transform.position = startPosition;
    }

    void Update()
    {
        // Calculate how much time has passed relative to the duration
        float timePassed = Time.time - startTime;
        if (timePassed < duration)
        {
            // Interpolate between start and target positions
            float fraction = timePassed / duration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, fraction);
        }
        else
        {
            // Stop movement when duration is exceeded
            transform.position = targetPosition;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                audioTimer = 0f; // Resetar o temporizador ao entrar no trigger
                StartCoroutine(DepleteHealth());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            StopCoroutine(DepleteHealth());
        }
    }

    System.Collections.IEnumerator DepleteHealth()
    {
        while (isPlayerInTrigger)
        {
            // A cada 20 segundos, reproduzir o áudio
            if (audioTimer >= audioInterval)
            {
                audioSource.Play();
                audioTimer = 0f; // Resetar o temporizador após tocar o som
            }

            playerStats.DecreaseHealth(5); // Depletes 1 health per 3 seconds
            yield return new WaitForSeconds(3f); // Depletes every second

            audioTimer += 3f; // Incrementar o temporizador
        }
    }
}
