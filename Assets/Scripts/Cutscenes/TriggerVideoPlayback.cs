using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerVideoPlayback : MonoBehaviour
{
    // Nome da cena a ser carregada
    [SerializeField] private string sceneName;

    // Método chamado quando outro Collider entra no Trigger
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que entrou tem a tag "Player"
        if (other.CompareTag("Player"))
        {
            // Carrega a nova cena
            SceneManager.LoadScene(sceneName);
        }
    }
}
