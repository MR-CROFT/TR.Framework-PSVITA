using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyWardrobe : MonoBehaviour
{
    public string itemName;
    public bool isCollected = false;  // Indica se o item já foi coletado

    public void CollectItem(Animator playerAnimator, string pickUpItemTrigger)
    {
        if (!isCollected)
        {
            // Executa a animação de pegar o item
            playerAnimator.SetTrigger(pickUpItemTrigger);

            // Marca o item como coletado
            isCollected = true;

            // Desativa o objeto do KeyItem ou realiza outra ação necessária
            gameObject.SetActive(false);
        }
    }
}
