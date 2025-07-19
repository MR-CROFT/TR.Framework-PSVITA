using UnityEngine;

public class FixedCamera : MonoBehaviour
{
    public Transform fixedCameraPosition; // Objeto fixo onde a câmera se posicionará
    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool isPlayerInTrigger = false;

    void Start()
    {
        // Obtém a câmera principal no início do jogo
        mainCamera = Camera.main;

        // Salva a posição e rotação original da câmera
        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;
    }

    void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que entrou no trigger tem a tag "Player"
        if (other.CompareTag("Player"))
        {
            // Move a câmera para a posição fixa e a orienta conforme o objeto fixo
            mainCamera.transform.position = fixedCameraPosition.position;
            mainCamera.transform.rotation = fixedCameraPosition.rotation;

            // Desabilita o script de seguir o jogador (se houver)
            // Se houver um script de follow na câmera, desabilite-o aqui
            // Exemplo: mainCamera.GetComponent<CameraFollow>().enabled = false;

            isPlayerInTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Verifica se o objeto que saiu do trigger tem a tag "Player"
        if (other.CompareTag("Player"))
        {
            // Restaura a posição e rotação original da câmera
            mainCamera.transform.position = originalCameraPosition;
            mainCamera.transform.rotation = originalCameraRotation;

            // Reabilita o script de seguir o jogador (se houver)
            // Exemplo: mainCamera.GetComponent<CameraFollow>().enabled = true;

            isPlayerInTrigger = false;
        }
    }

    void LateUpdate()
    {
        // Atualiza a posição original da câmera se o jogador não estiver no trigger
        if (!isPlayerInTrigger)
        {
            originalCameraPosition = mainCamera.transform.position;
            originalCameraRotation = mainCamera.transform.rotation;
        }
    }
}
