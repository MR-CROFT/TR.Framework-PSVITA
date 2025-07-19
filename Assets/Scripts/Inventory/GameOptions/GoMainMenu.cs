using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoMainMenu : InventoryItem 
{
    [Tooltip("Nome da cena para a qual mudar")]
    public string sceneName;

    public override void Use(PlayerController playerController)
    {
        Debug.Log("Método Use chamado.");

        if (!string.IsNullOrEmpty(sceneName))
        {
            // Verifica se o modelo de inventário está definido
            if (inventoryModel != null)
            {
                Debug.Log("InventoryModel está definido. Iniciando rotação.");
                
                // Verifica se há um Rigidbody no inventoryModel e define como kinematic para evitar interferência de física
                Rigidbody rb = inventoryModel.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
                
                StartCoroutine(RotateAndChangeScene());
            }
            else
            {
                Debug.LogWarning("InventoryModel não está definido para o item SceneChange.");
                ChangeScene();
            }
        }
        else
        {
            Debug.LogWarning("Nome da cena não definido para o item SceneChange.");
        }
    }

    private IEnumerator RotateAndChangeScene()
    {
        Debug.Log("Corrotina RotateAndChangeScene chamada.");

        float duration = 1f; // Duração da rotação em segundos
        float elapsedTime = 0f;
        Quaternion initialRotation = inventoryModel.transform.rotation;
        Quaternion targetRotation = initialRotation * Quaternion.Euler(0f, 180f, 0f);

        // Animação de rotação do InventoryItem
        while (elapsedTime < duration)
        {
            inventoryModel.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Garante que a rotação termine exatamente em 180 graus
        inventoryModel.transform.rotation = targetRotation;

        // Muda para a cena após a rotação
        ChangeScene();
    }

    private void ChangeScene()
    {
        Debug.Log($"Mudando para a cena: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
