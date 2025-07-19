using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;  // Adicionado para usar IEnumerator

public class LoadScene : MonoBehaviour
{
    public string sceneName = "NomeDaCena";
    public float delay = 3.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
