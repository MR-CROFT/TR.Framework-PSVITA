using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBlock : MonoBehaviour
{
    public enum MovementAxis { X, Z }

    [Header("Configurações do Objeto Empurrável/Puxável")]
    [SerializeField] private string pushAnimation = "PushObject"; // Nome da animação de empurrar
    [SerializeField] private string pullAnimation = "PullObject"; // Nome da animação de puxar

    private PlayerController playerController;
    private Animator playerAnimator;
    private bool isPlayerInTrigger = false;
    private bool isPushingOrPulling = false;
    private GameObject player;
    private MovementAxis moveAxis;

    void Update()
    {
        if (isPlayerInTrigger && !isPushingOrPulling)
        {
            if (Input.GetButtonDown("Fire") && playerController.charControl.isGrounded)
            {
                DetermineMoveAxis();
                PrepareForPushPull();
            }
        }

        if (isPushingOrPulling)
        {
            HandlePushPull();
        }
    }

    private void DetermineMoveAxis()
    {
        Vector3 playerPosition = player.transform.position;
        Vector3 objectPosition = transform.position;

        // Determina o eixo de movimento com base na posição relativa do jogador
        if (Mathf.Abs(playerPosition.x - objectPosition.x) > Mathf.Abs(playerPosition.z - objectPosition.z))
        {
            moveAxis = MovementAxis.X;
        }
        else
        {
            moveAxis = MovementAxis.Z;
        }
    }

    private void PrepareForPushPull()
    {
        isPushingOrPulling = true;
        playerController.locked = true; // Bloqueia o movimento padrão do player
        playerController.charControl.enabled = false; // Desativa o CharacterController para preparar o movimento manual
        playerAnimator.SetBool("isPushingOrPulling", true); // Ativa a animação de preparação

        // Faz o objeto seguir o player como filho para acompanhar a animação
        transform.SetParent(player.transform);
    }

    private void HandlePushPull()
    {
        float verticalInput = Input.GetAxis("Vertical");

        if (Input.GetButton("Fire"))
        {
            if (verticalInput > 0) // Empurrar
            {
                playerAnimator.Play(pushAnimation);
            }
            else if (verticalInput < 0) // Puxar
            {
                playerAnimator.Play(pullAnimation);
            }
        }
        else // Se o jogador soltar o botão "Fire", interrompe a ação imediatamente
        {
            StopPushPull();
        }
    }

    private void StopPushPull()
    {
        isPushingOrPulling = false;
        playerController.locked = false; // Desbloqueia o movimento padrão do player
        playerController.charControl.enabled = true; // Reativa o CharacterController
        playerAnimator.SetBool("isPushingOrPulling", false); // Desativa a animação de preparação

        // Remove o objeto como filho do player para que ele permaneça no lugar final
        transform.SetParent(null);

        // Interrompe a animação em execução
        playerAnimator.Play("Idle"); // Substitua "Idle" pela animação padrão de repouso
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            player = other.gameObject;
            playerController = player.GetComponent<PlayerController>();
            playerAnimator = player.GetComponent<Animator>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            player = null;
            playerController = null;
            playerAnimator = null;
            if (isPushingOrPulling)
            {
                StopPushPull();
            }
        }
    }
}
