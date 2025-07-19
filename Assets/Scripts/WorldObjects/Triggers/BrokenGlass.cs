using UnityEngine;

public class BrokenGlass : MonoBehaviour
{
    [Header("Objetos Filhos")]
    public GameObject objetoParaDesativar; // Objeto filho a ser desativado
    public GameObject objetoParaAtivar; // Objeto filho a ser ativado

    [Header("Som")]
    public AudioClip sfx; // Som que será reproduzido
    private AudioSource audioSource;

    private bool somTocado = false; // Controle para garantir que o som só toque uma vez

    void Start()
    {
        // Inicializa o AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = sfx;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que entrou no collider tem a tag "Player"
        if (other.CompareTag("Player"))
        {
            // Desativa o objeto filho selecionado
            if (objetoParaDesativar != null)
                objetoParaDesativar.SetActive(false);

            // Ativa o outro objeto filho
            if (objetoParaAtivar != null)
                objetoParaAtivar.SetActive(true);

            // Reproduz o som apenas se ainda não tiver sido tocado
            if (!somTocado && sfx != null)
            {
                audioSource.Play();
                somTocado = true; // Marca o som como tocado para não repetir
            }
        }
    }
}
