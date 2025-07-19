using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.PSVita;

public class CutsceneEndLevel : MonoBehaviour
{
    public string moviePath = "Movies/EndTutorialCutscene.mp4";  // Caminho para o vídeo
    public RenderTexture renderTexture;  // Textura para renderizar o vídeo
    public string nextSceneName;     // Nome da cena a ser carregada após o vídeo
    private PlayerController playerController;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isPlaying = false;

    void Start()
    {
        // Inicializa o vídeo player com a textura renderizada
        PSVitaVideoPlayer.Init(renderTexture);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Salva a posição e rotação original do Player
                originalPosition = playerController.transform.position;
                originalRotation = playerController.transform.rotation;

                // Bloqueia os controles do Player
                playerController.locked = true;
                playerController.enabled = false;

                // Reproduz o vídeo
                if (!isPlaying)
                {
                    PSVitaVideoPlayer.Play(moviePath, PSVitaVideoPlayer.Looping.Continuous, PSVitaVideoPlayer.Mode.RenderToTexture);
                    isPlaying = true;
                }
            }
        }
    }

    void OnPreRender()
    {
        // Atualiza o estado do vídeo
        PSVitaVideoPlayer.Update();
    }

    void OnMovieEvent(int eventID)
    {
        PSVitaVideoPlayer.MovieEvent movieEvent = (PSVitaVideoPlayer.MovieEvent)eventID;
        switch (movieEvent)
        {
            case PSVitaVideoPlayer.MovieEvent.STOP:
                EndReached();
                break;
        }
    }

    private void EndReached()
    {
        // Para o vídeo após a primeira execução
        PSVitaVideoPlayer.Stop();

        // Restaura a posição e rotação original do Player
        if (playerController != null)
        {
            playerController.transform.position = originalPosition;
            playerController.transform.rotation = originalRotation;

            // Desbloqueia os controles do Player
            playerController.enabled = true;
            playerController.locked = false;
        }

        // Carrega a nova cena
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }

        isPlaying = false;
    }
}
