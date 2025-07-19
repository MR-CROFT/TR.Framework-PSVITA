using UnityEngine;
using UnityEngine.PSVita;
using UnityEngine.SceneManagement;

public class PSVitaVideo : MonoBehaviour
{
    public string m_MoviePath;
    public RenderTexture m_RenderTexture;
    private bool m_IsPlaying = false;

    void Start()
    {
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
        // Substitua "NextSceneName" pelo nome da cena que deseja carregar
        SceneManager.LoadScene("Paris");
    }
}
