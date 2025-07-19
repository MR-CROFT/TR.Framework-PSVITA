using UnityEngine;
using System.Collections;

public class WardrobeInteraction : MonoBehaviour
{
    [Header("References")]
    public Animator playerAnimator;            // Animator do Player
    public Animator wardrobeAnimator;          // Animator do Armário
    public AudioClip openSoundClip;            // Som de abrir o armário
    public AudioClip closeSoundClip;           // Som de fechar o armário
    public PlayerInventory playerInventory;    // Referência ao Inventário do Player
    public InventoryItem itemToAdd;            // Item a ser adicionado ao inventário
    public Transform targetPosition;           // Posição alvo para o Player se mover
    public PickUICam pickCam;                  // Referência ao PickUICam

    private bool isPlayerInRange = false;      // Verifica se o player está dentro do trigger
    private bool isWardrobeOpen = false;       // Verifica o estado do armário
    private bool isProcessing = false;         // Verifica se o processo de abertura/fechamento está em andamento

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    void Update()
    {
        // Verifica se o botão de ação foi pressionado, o jogador está no trigger, e o processo não está em andamento
        if (isPlayerInRange && Input.GetButtonDown("Action") && !isProcessing)
        {
            if (!isWardrobeOpen)
            {
                StartCoroutine(OpenAndCloseWardrobe());
            }
        }
    }

    IEnumerator OpenAndCloseWardrobe()
    {
        isProcessing = true; // Inicia o bloqueio do processo

        // Move o Player para a posição alvo antes de abrir o armário
        MovePlayerToTargetPosition();

        // Reproduz a animação de abrir o armário e do player
        playerAnimator.SetTrigger("OpenWardrobe");
        PlaySound(openSoundClip); // Toca o som de abrir o armário

        // Dispara a animação de abrir o armário (ambas as portas)
        wardrobeAnimator.SetTrigger("Open");

        // Espera até que a animação de abrir termine + tempo adicional para garantir execução completa
        yield return new WaitForSeconds(wardrobeAnimator.GetCurrentAnimatorStateInfo(0).length + 0.5f);

        // Checa se há um item a ser adicionado e, se sim, executa a animação TakeItem
        if (itemToAdd != null)
        {
            playerAnimator.SetTrigger("TakeItemW");

            // Espera até que a animação de pegar o item termine
            yield return new WaitForSeconds(playerAnimator.GetCurrentAnimatorStateInfo(0).length);

            // Adiciona o item ao inventário do player após a animação de pegar o item
            playerInventory.AddItem(itemToAdd);

            // Usa o PickUICam para exibir o item
            pickCam.SetAndEnable(itemToAdd.gameObject);
            itemToAdd.gameObject.SetActive(false);
        }

        // Adiciona um intervalo de tempo após o armário ser aberto e antes de fechar
        yield return new WaitForSeconds(3f); // Intervalo de 3 segundos (ajuste conforme necessário)

        // Agora que a animação TakeItem terminou, reproduz a animação de fechar o armário
        playerAnimator.SetTrigger("CloseWardrobe");
        PlaySound(closeSoundClip); // Toca o som de fechar o armário

        // Aguarda até que a animação de fechar do player comece
        yield return new WaitUntil(() => playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("CloseWardrobe"));

        // Dispara a animação de fechar o armário
        wardrobeAnimator.SetTrigger("Close");

        // Espera até que a animação de fechar termine
        yield return new WaitForSeconds(wardrobeAnimator.GetCurrentAnimatorStateInfo(0).length);

        isWardrobeOpen = false;
        isProcessing = false; // Libera o processo

        // Aguarda 10 segundos antes de destruir o script
        yield return new WaitForSeconds(3f);
        Destroy(this); // Destroi apenas o script após 3 segundos
    }

    void MovePlayerToTargetPosition()
    {
        if (targetPosition != null)
        {
            // Move o Player para a posição do targetPosition
            Transform playerTransform = playerAnimator.transform;
            playerTransform.position = targetPosition.position;
            playerTransform.rotation = targetPosition.rotation;
        }
    }

    void PlaySound(AudioClip clip)
    {
        // Toca o som no local do armário
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}
