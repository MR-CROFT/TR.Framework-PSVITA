using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatJumping : StateBase<PlayerController>
{
    private bool hasJumped = false;
    private float cooldownT; // To manage shooting cooldown
    private PlayerSFX playerSFX;

    // GameObjects for guns and holsters
    private GameObject gunHandLeft;
    private GameObject gunHandRight;
    private GameObject gunHolsterLeft;
    private GameObject gunHolsterRight;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isCombatJumping", true);
        hasJumped = false;
        cooldownT = 0;

        // Initialize shooting sound
        playerSFX = player.GetComponent<PlayerSFX>();

        // Access gun and holster GameObjects
        gunHandLeft = player.transform.Find("LARAD/GUNHAND_LEFT").gameObject;
        gunHandRight = player.transform.Find("LARAD/GUNHAND_RIGHT").gameObject;
        gunHolsterLeft = player.transform.Find("LARAD/GUNHOLSTER_LEFT").gameObject;
        gunHolsterRight = player.transform.Find("LARAD/GUNHOLSTER_RIGHT").gameObject;

        // Activate guns and deactivate holsters
        if (gunHandLeft != null) gunHandLeft.SetActive(true);
        if (gunHandRight != null) gunHandRight.SetActive(true);
        if (gunHolsterLeft != null) gunHolsterLeft.SetActive(false);
        if (gunHolsterRight != null) gunHolsterRight.SetActive(false);

        float absAngle = Mathf.Abs(player.CombatAngle);
        player.transform.rotation = absAngle > 45f && absAngle < 135f ?
            Quaternion.LookRotation(Vector3.Cross(player.transform.forward, Vector3.up))
            : Quaternion.LookRotation((absAngle <= 45f ? 1f : -1f) * 
            Vector3.Scale(new Vector3(1f, 0f, 1f), player.Velocity.normalized));
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isCombatJumping", false);
        player.Anim.applyRootMotion = false; // Disable root motion when exiting the state

        // Deactivate guns and activate holsters when exiting the state
        if (gunHandLeft != null) gunHandLeft.SetActive(false);
        if (gunHandRight != null) gunHandRight.SetActive(false);
        if (gunHolsterLeft != null) gunHolsterLeft.SetActive(true);
        if (gunHolsterRight != null) gunHolsterRight.SetActive(true);
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);
        AnimatorTransitionInfo transInfo = player.Anim.GetAnimatorTransitionInfo(0);

        LockOn(player);

        // Shooting logic
        if (Input.GetKey(player.playerInput.fireWeapon) && cooldownT <= 0)
        {
            if (player.currentEnemyTarget != null)
            {
                // If there's a target, deal damage
                EnemyController enemyScript = player.currentEnemyTarget.GetComponent<EnemyController>();
                if (enemyScript != null)
                {
                    enemyScript.Health -= (int)player.damage;
                    if (enemyScript.Health <= 0)
                    {
                        CheckForTargets(player);
                    }
                }
            }
            playerSFX?.PlayShootSound(); // Play shooting sound

            cooldownT = player.shotDelay;
        }

        cooldownT -= Time.deltaTime;

        if (hasJumped)
        {
            player.ApplyGravity(player.gravity);

            if (player.Grounded)
                player.StateMachine.GoToState<Combat>();
        }
        else
        {
            if (transInfo.IsName("CombatCompress -> JumpR"))
            {
                player.ForceWaistRotation = false;
                player.Velocity = player.transform.right * 4f + Vector3.up * player.jumpYVel;
                hasJumped = true;
            }
            else if (transInfo.IsName("CombatCompress -> JumpL"))
            {
                player.ForceWaistRotation = false;
                player.Velocity = player.transform.right * -4f + Vector3.up * player.jumpYVel;
                hasJumped = true;
            }
            else if (transInfo.IsName("CombatCompress -> JumpB"))
            {
                player.ForceWaistRotation = false;
                player.Velocity = player.transform.forward * 0.5f;

                // Enable root motion for JumpB animation
                player.Anim.applyRootMotion = true;

                // Disable combat animations
                player.Anim.SetBool("isCombat", false);
                player.Anim.SetBool("isTargetting", false);
                player.Anim.SetBool("isFiring", false);
                player.pistolLHand.SetActive(false);
                player.pistolRHand.SetActive(false);
                player.pistolLLeg.SetActive(true);
                player.pistolRLeg.SetActive(true);
                player.ForceWaistRotation = false;

                // Start coroutine for 1.3 seconds delay
                player.StartCoroutine(WaitForJumpDelay());
            }
            else if (transInfo.IsName("CombatCompress -> JumpF"))
            {
                player.Velocity = player.transform.forward * 4f + Vector3.up * player.jumpYVel;
                hasJumped = true;
            }
            // Diagonal
            else if (transInfo.IsName("CombatCompress -> JumpFL"))
            {
                player.ForceWaistRotation = false;
                player.Velocity = -player.transform.right + player.transform.forward * 4f + Vector3.up * player.jumpYVel;
                hasJumped = true;
            }
            else if (transInfo.IsName("CombatCompress -> JumpFR"))
            {
                player.ForceWaistRotation = false;
                player.Velocity = player.transform.right + player.transform.forward * 4f + Vector3.up * player.jumpYVel;
                hasJumped = true;
            }
        }
    }

    private IEnumerator WaitForJumpDelay()
    {
        yield return new WaitForSeconds(1.3f); // Wait for 1.3 seconds
        hasJumped = true;
    }

    private void LockOn(PlayerController player)
    {
        player.camController.State = player.currentEnemyTarget == null ? CameraState.Grounded : CameraState.Combat;

        if (player.currentEnemyTarget == null)
            return;

        player.lockOnCanvas.gameObject.SetActive(true);
        player.camController.LookAt = player.currentEnemyTarget;

        // Aim at player if player is further than 1 unit to avoid clipping
        if (Vector3.Distance(player.transform.position, player.currentEnemyTarget.position) > 1f)
            player.lockOnCanvas.LookAt(player.playerTarget);

        // Move target graphic
        player.lockOnCanvas.transform.position = player.currentEnemyTarget.position + Vector3.up * 1.5f + player.lockOnCanvas.forward * .5f;
    }

    private void CheckForTargets(PlayerController player)
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, 10f);
        foreach (Collider c in hitColliders)
        {
            if (c.gameObject.CompareTag("Enemy"))
            {
                player.lockOnCanvas.gameObject.SetActive(true);
                player.camController.LookAt = player.currentEnemyTarget = c.gameObject.transform;
                break;
            }
            else
            {
                player.currentEnemyTarget = null;
            }
        }
    }
}
