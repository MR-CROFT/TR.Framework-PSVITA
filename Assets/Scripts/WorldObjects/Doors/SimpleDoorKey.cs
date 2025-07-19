using UnityEngine;

public class SimpleDoorKey : Door
{
    public Transform realDoor;
    public string openDoorAnimationName = "OpenDoor"; // Nome da animação para abrir a porta com valor padrão
    public string lockedDoorAnimationName = "LockedDoor"; // Nome da animação para porta trancada com valor padrão
    public string playerLockedDoorAnimationName = "LockedDoor"; // Nome da animação para o jogador quando a porta está trancada com valor padrão
    private Animator doorAnimator;

    public AudioClip insertKeySFX; // Efeito sonoro para quando a chave é inserida
    public AudioClip doorOpenSFX; // Efeito sonoro para quando a porta abre
    public AudioClip doorLockedSFX; // Efeito sonoro para quando a porta está trancada
    private AudioSource audioSource;

    // Use this for initialization
    void Start()
    {
        doorAnimator = realDoor.GetComponent<Animator>();
        if (doorAnimator == null)
        {
            Debug.LogError("Animator component not found on realDoor.");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not found on this GameObject.");
        }
    }

    public override void OpenDoor(PlayerController player)
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        // Verifica se o jogador tem a chave correta no inventário
        bool hasKey = false;
        foreach (ItemInfo itemInfo in inventory.Items[0]) // A chave é do tipo 0
        {
            if (itemInfo.item.itemName == keyName)
            {
                hasKey = true;
                break;
            }
        }

        if (hasKey)
        {
            // Se o jogador tiver a chave, abre o inventário
            player.gameObject.GetComponent<RingMenu>().EnableKeyMenu(this);
        }
        else
        {
            // Se o jogador não tiver a chave, mover para o Open Point e tocar a animação
            Animator playerAnimator = player.GetComponent<Animator>();
            if (playerAnimator != null && !string.IsNullOrEmpty(playerLockedDoorAnimationName))
            {
                // Mover o jogador para o Open Point
                player.transform.position = openPoint.position;
                player.transform.rotation = openPoint.rotation;
                
                // Tocar a animação no jogador
                playerAnimator.Play(playerLockedDoorAnimationName); 
            }

            if (doorLockedSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(doorLockedSFX); // Som de porta trancada
            }
        }
    }

    public override void OpenDoorAct()
    {
        // Esta função já verifica se o player tem a chave correta
        if (keyName != curKeyName)
        {
            // Som de porta trancada, animação já foi executada em OpenDoor
            if (doorLockedSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(doorLockedSFX); // Som de porta trancada
            }
            return;
        }

        // Se o player tiver a chave correta, segue com o processo de abrir a porta
        if (insertKeySFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(insertKeySFX); // Toca o som de inserção da chave
        }

        if (doorAnimator != null)
        {
            doorAnimator.Play(openDoorAnimationName); // Toca a animação de abrir a porta configurada no Inspector
            if (doorOpenSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(doorOpenSFX); // Toca o som de abertura da porta
            }
        }

        // Destruir o script após 3 segundos
        Invoke("DestroyScript", 3f);
    }

    private void DestroyScript()
    {
        Destroy(this);
    }
}
