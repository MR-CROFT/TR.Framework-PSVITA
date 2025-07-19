using UnityEngine;
using System.Collections; // Necessário para usar IEnumerator

public class DelayedAudio : MonoBehaviour
{
    // Referência ao componente AudioSource
    private AudioSource audioSource;

    // Tempo de espera antes de iniciar o áudio (60 segundos)
    public float delayTime = 60f;

    void Start()
    {
        // Obtém o componente AudioSource anexado ao mesmo GameObject
        audioSource = GetComponent<AudioSource>();

        // Inicia a rotina para atrasar a reprodução do áudio
        StartCoroutine(PlayAudioAfterDelay());
    }

    private IEnumerator PlayAudioAfterDelay()
    {
        // Aguarda o tempo especificado
        yield return new WaitForSeconds(delayTime);

        // Reproduz o áudio
        audioSource.Play();
    }
}
