using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialItems : MonoBehaviour
{
    [Header("Initial Items Prefabs")]
    public GameObject[] initialItems;  // Array para os prefabs dos itens iniciais

    private PlayerInventory playerInventory;

    private void Start()
    {
        // Encontrar o jogador através da tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Garantir que o jogador foi encontrado e que ele tem o script PlayerInventory
        if (player != null)
        {
            playerInventory = player.GetComponent<PlayerInventory>();

            if (playerInventory != null)
            {
                ProvideInitialItems();
            }
            else
            {
                Debug.LogError("PlayerInventory não encontrado no jogador.");
            }
        }
        else
        {
            Debug.LogError("Jogador com tag 'Player' não encontrado.");
        }
    }

    private void ProvideInitialItems()
    {
        // Iterar sobre os prefabs dos itens iniciais e adicionar ao inventário do jogador
        foreach (GameObject itemPrefab in initialItems)
        {
            if (itemPrefab != null)
            {
                // Instanciar o item temporariamente e obter o componente InventoryItem
                GameObject instantiatedItem = Instantiate(itemPrefab);

                InventoryItem item = instantiatedItem.GetComponent<InventoryItem>();

                // Verificação adicional para garantir que o item foi instanciado corretamente
                if (item != null)
                {
                    // Adicionar o item ao inventário do jogador
                    playerInventory.AddItem(item);

                    // Destruir o item instanciado após adicioná-lo ao inventário
                    Destroy(instantiatedItem);
                }
                else
                {
                    Debug.LogError("Prefab do item não possui o componente InventoryItem.");
                }
            }
            else
            {
                Debug.LogError("O prefab de item é nulo.");
            }
        }
    }
}
