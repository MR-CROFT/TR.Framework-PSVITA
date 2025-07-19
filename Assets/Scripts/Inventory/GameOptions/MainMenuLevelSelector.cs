using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLevelSelector : MonoBehaviour
{
    // Método público para carregar uma cena
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
