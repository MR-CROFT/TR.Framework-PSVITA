using UnityEngine;
using UnityEngine.PSVita;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExampleRenderTexturePlayback : MonoBehaviour
{
    public string m_MoviePath;
    public RenderTexture m_RenderTexture;
    private bool m_IsPlaying = false;

    // Adiciona uma variável pública para selecionar a cena no Inspector
    public string sceneToLoad; 

    void Start()
    {
        StartCoroutine(PlayVideoAfterDelay(0.1f)); // Chama a corrotina para esperar 1 segundo antes de iniciar o vídeo
    }

    IEnumerator PlayVideoAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Espera por 1 segundo
        PSVitaVideoPlayer.Init(m_RenderTexture);
        PSVitaVideoPlayer.Play(m_MoviePath, PSVitaVideoPlayer.Looping.None, PSVitaVideoPlayer.Mode.RenderToTexture);
    }

    void OnPreRender()
    {
        PSVitaVideoPlayer.Update();
    }

    void OnMovieEvent(int eventID)
    {
        PSVitaVideoPlayer.MovieEvent movieEvent = (PSVitaVideoPlayer.MovieEvent)eventID;
        switch (movieEvent)
        {
            case PSVitaVideoPlayer.MovieEvent.PLAY:
                m_IsPlaying = true;
                break;

            case PSVitaVideoPlayer.MovieEvent.STOP:
                m_IsPlaying = false;
                LoadNextScene();
                break;
        }
    }

    void LoadNextScene()
    {
        // Carrega a cena selecionada no Inspector
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Nenhuma cena foi selecionada para carregar.");
        }
    }
}
