using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stealth : StateBase<PlayerController>
{
    private float originalSpeed;
    private bool isStealthActive = false; // Controle de estado para o modo Stealth

    public override void OnEnter(PlayerController player)
    {
        // Armazena a velocidade original
        originalSpeed = player.walkSpeed;
    }

    public override void OnExit(PlayerController player)
    {
        // Retorna à velocidade original e desativa o modo stealth
        player.Anim.applyRootMotion = false;
        player.Anim.SetBool("isStealth", false);
        player.walkSpeed = originalSpeed; // Retorna à velocidade original
    }

    public override void Update(PlayerController player)
    {
        // Detecta o clique para alternar o estado de stealth
        if (Input.GetKeyDown(player.playerInput.stealth))
        {
            isStealthActive = !isStealthActive; // Alterna o estado

            if (isStealthActive)
            {
                // Entrar no modo stealth
                player.Anim.applyRootMotion = true;
                player.Anim.SetBool("isStealth", true);
                player.walkSpeed = player.stealthSpeed;
                player.Velocity = Vector3.zero;
            }
            else
            {
                // Sair do modo stealth e retornar à velocidade normal
                player.Anim.applyRootMotion = false;
                player.Anim.SetBool("isStealth", false);
                player.walkSpeed = originalSpeed;
                player.StateMachine.GoToState<Locomotion>();
                return;
            }
        }

        if (isStealthActive)
        {
            // Usa a velocidade de stealth para o movimento em todas as direções
            float moveSpeed = player.stealthSpeed;

            // Movimento e rotação em modo stealth
            player.MoveGrounded(moveSpeed, pushDown: false);
            player.RotateToVelocityGround(smoothing: 4f);
        }
    }
}
