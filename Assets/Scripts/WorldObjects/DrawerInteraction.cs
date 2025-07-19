using UnityEngine;
using System.Collections;

public class DrawerInteraction : MonoBehaviour
{
    [Header("References")]
    public Animator playerAnimator;          // Animator do Player
    public Animator drawerAnimator;          // Animator da Gaveta
    public AudioClip openSoundClip;          // Som de abrir a gaveta
    public AudioClip closeSoundClip;         // Som de fechar a gaveta
    public PlayerInventory playerInventory;  // Referência ao Inventário do Player
    public InventoryItem itemToAdd;          // Item a ser adicionado ao inventário
    public Transform targetPosition;         // Posição alvo para o Player se mover
    public PickUICam pickCam; // Referência ao PickUICam

    private bool isPlayerInRange = false;    // Verifica se o player está dentro do trigger
    private bool isDrawerOpen = false;       // Verifica o estado da gaveta
    private bool isProcessing = false;       // Verifica se o processo de abertura/fechamento está em andamento

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
            if (!isDrawerOpen)
            {
                StartCoroutine(OpenAndCloseDrawer());
            }
        }
    }

    IEnumerator OpenAndCloseDrawer()
    {
        isProcessing = true; // Inicia o bloqueio do processo

        // Move o Player para a posição alvo antes de abrir a gaveta
        MovePlayerToTargetPosition();

        // Reproduz a animação de abrir a gaveta e do player
        playerAnimator.SetTrigger("OpenDrawer");
        PlaySound(openSoundClip); // Toca o som de abrir a gaveta

        drawerAnimator.SetTrigger("Open");    // Trigger de abrir gaveta

        // Espera até que a animação de abrir termine + tempo adicional para garantir execução completa
        yield return new WaitForSeconds(drawerAnimator.GetCurrentAnimatorStateInfo(0).length + 0.1f);

        // Checa se há um item a ser adicionado e, se sim, executa a animação TakeDrawer
        if (itemToAdd != null)
        {
            playerAnimator.SetTrigger("TakeDrawer");

            // Espera até que a animação de pegar o item termine
            yield return new WaitForSeconds(playerAnimator.GetCurrentAnimatorStateInfo(0).length);

            // Adiciona o item ao inventário do player após a animação de pegar o item
            playerInventory.AddItem(itemToAdd);
            pickCam.SetAndEnable(itemToAdd.gameObject);
        }

        // Adiciona um intervalo de tempo após a gaveta ser aberta e antes de fechar
        yield return new WaitForSeconds(4f); // Intervalo de 2 segundos (ajuste conforme necessário)

        // Agora que a animação TakeDrawer terminou, reproduz a animação de fechar a gaveta
        playerAnimator.SetTrigger("CloseDrawer");
        PlaySound(closeSoundClip); // Toca o som de fechar a gaveta

        drawerAnimator.SetTrigger("Close");    // Trigger para fechar a gaveta

        // Espera até que a animação de fechar termine
        yield return new WaitForSeconds(drawerAnimator.GetCurrentAnimatorStateInfo(0).length);

        isDrawerOpen = false;
        isProcessing = false; // Libera o processo

        // Aguarda 10 segundos antes de destruir o script
        yield return new WaitForSeconds(3f);
        Destroy(this); // Destroi apenas o script após 10 segundos
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
        // Toca o som no local da gaveta
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}
