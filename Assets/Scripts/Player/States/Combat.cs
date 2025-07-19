using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : StateBase<PlayerController>
{
    private Transform target;
    private CabalAI enemyScript;
    private float cooldownT;
    private AmmoManager ammoManager; // Reference to AmmoManager

    // GameObjects for guns and holsters
    private GameObject gunHandLeft;
    private GameObject gunHandRight;
    private GameObject gunHolsterLeft;
    private GameObject gunHolsterRight;

    // Reference to PlayerSFX script
    private PlayerSFX playerSFX;

    public override void OnEnter(PlayerController player)
    {
        // Initialize the AmmoManager reference
        ammoManager = player.GetComponent<AmmoManager>(); // Get the AmmoManager from the PlayerController

        if (ammoManager == null)
        {
            Debug.LogError("AmmoManager component not found on the PlayerController.");
        }

        // Access the LARAD, which is a child of PlayerController
        GameObject larad = player.transform.Find("LARAD").gameObject;

        // Find the GameObjects within the LARAD hierarchy
        gunHandLeft = larad.transform.Find("GUNHAND_LEFT").gameObject;
        gunHandRight = larad.transform.Find("GUNHAND_RIGHT").gameObject;
        gunHolsterLeft = larad.transform.Find("GUNHOLSTER_LEFT").gameObject;
        gunHolsterRight = larad.transform.Find("GUNHOLSTER_RIGHT").gameObject;

        // Activate the guns and deactivate holsters
        if (gunHandLeft != null) gunHandLeft.SetActive(true);
        if (gunHandRight != null) gunHandRight.SetActive(true);
        if (gunHolsterLeft != null) gunHolsterLeft.SetActive(false);
        if (gunHolsterRight != null) gunHolsterRight.SetActive(false);

        player.EnableCharControl();
        player.Anim.applyRootMotion = false;
        player.ForceWaistRotation = true;
        player.Anim.SetBool("isCombat", true);
        player.Stats.ShowCanvas();

        // Activate player's gun hands and deactivate holsters
        player.pistolLHand.SetActive(true);
        player.pistolRHand.SetActive(true);
        player.pistolLLeg.SetActive(false);
        player.pistolRLeg.SetActive(false);

        // Reference to the PlayerSFX script
        playerSFX = player.GetComponent<PlayerSFX>();

        cooldownT = 0;
        target = null;
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.applyRootMotion = false;
        player.camController.State = CameraState.Grounded;
        player.lockOnCanvas.gameObject.SetActive(false);

        if (target != null)
        {
            player.currentEnemyTarget = target;
            player.lockOnCanvas.gameObject.SetActive(false);
        }
        else
        {
            player.currentEnemyTarget = null;
        }

        // Deactivate player's gun hands and activate holsters
        player.pistolLHand.SetActive(false);
        player.pistolRHand.SetActive(false);
        player.pistolLLeg.SetActive(true);
        player.pistolRLeg.SetActive(true);

        if (gunHolsterLeft != null) gunHolsterLeft.SetActive(true);
        if (gunHolsterRight != null) gunHolsterRight.SetActive(true);
        if (gunHandLeft != null) gunHandLeft.SetActive(false);
        if (gunHandRight != null) gunHandRight.SetActive(false);

        player.ForceWaistRotation = false;
    }

    public override void Update(PlayerController player)
    {
        player.Anim.SetFloat("Stairs", 0f, 0.1f, Time.deltaTime);

        if (!Input.GetKey(player.playerInput.drawWeapon) && Input.GetAxisRaw("CombatTrigger") < 0.1f)
        {
            player.Anim.SetBool("isCombat", false);
            player.Anim.SetBool("isTargetting", false);
            player.Anim.SetBool("isFiring", false);
            player.Stats.HideCanvas();
            player.pistolLHand.SetActive(false);
            player.pistolRHand.SetActive(false);
            player.pistolLLeg.SetActive(true);
            player.pistolRLeg.SetActive(true);
            if (gunHolsterLeft != null) gunHolsterLeft.SetActive(true);
            if (gunHolsterRight != null) gunHolsterRight.SetActive(true);
            if (gunHandLeft != null) gunHandLeft.SetActive(false);
            if (gunHandRight != null) gunHandRight.SetActive(false);
            player.ForceWaistRotation = false;
            player.StateMachine.GoToState<Locomotion>();
            return;
        }

        if (player.Grounded)
        {
            if (Input.GetKeyDown(player.playerInput.jump))
            {
                player.StateMachine.GoToState<CombatJumping>();
                return;
            }

            float moveSpeed = Input.GetKey(player.playerInput.walk) ? player.walkSpeed : player.runSpeed;

            player.MoveStrafeGround(moveSpeed);
            if (player.TargetSpeed > 1f)
                player.RotateToVelocityStrafe();
        }
        else
        {
            player.ApplyGravity(player.gravity);
        }

        if (Input.GetKeyDown(player.playerInput.crouch))
        {
            CycleTarget(player);
        }

        if (target == null)
        {
            player.lockOnCanvas.gameObject.SetActive(false);
            CheckForTargets(player);
        }

        if (target != null)
        {
            // Rotate player to face the target
            Vector3 directionToTarget = (target.position - player.transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * player.rotationSpeed);

            player.Anim.SetFloat("AimAngle", Vector3.SignedAngle(directionToTarget, player.transform.forward, Vector3.up));

            if (Vector3.Distance(player.transform.position, target.position) > 1f)
                player.lockOnCanvas.LookAt(player.playerTarget);

            player.lockOnCanvas.transform.position = target.position + Vector3.up * 1.5f + player.lockOnCanvas.forward * .5f;
            UpdateLockOnCanvasPosition(player);
        }
        else
        {
            player.Anim.SetFloat("AimAngle", player.CombatAngle);
        }

        // Firing logic with consistent cooldown
        if (Input.GetKey(player.playerInput.fireWeapon) && cooldownT <= 0)
{
    if (ammoManager != null && ammoManager.UseAmmo(1)) // Check and use ammo
    {
        if (target != null)
        {
            enemyScript.TakeDamage((int)player.damage);  // Use TakeDamage() instead of direct health manipulation
            playerSFX?.PlayShootSound(); // Play the shooting sound when targeting an enemy
        }
        else
        {
            playerSFX?.PlayShootSound(); // Play shooting sound when not targeting an enemy
        }

        cooldownT = player.shotDelay; // Consistent cooldown for both targeting and non-targeting
    }
    else
    {
        Debug.Log("Out of Ammo"); // Handle the case where there's no ammo
    }
}

        player.WaistRotation = player.transform.rotation;
        player.Anim.SetBool("isTargetting", true);

        player.camController.State = target == null ? CameraState.Grounded : CameraState.Combat;

        player.Anim.SetBool("isFiring", Input.GetKey(player.playerInput.fireWeapon));

        cooldownT -= Time.deltaTime;
    }

    private void CycleTarget(PlayerController player)
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, 10f);
        Transform newTarget = null;
        bool foundNextTarget = false;

        foreach (Collider c in hitColliders)
        {
            if (c.gameObject.CompareTag("Enemy"))
            {
                if (foundNextTarget)
                {
                    newTarget = c.transform;
                    break;
                }

                if (target != null && c.transform == target)
                {
                    foundNextTarget = true;
                }
            }
        }

        // If no new target found, reset to first available target
        if (newTarget == null && hitColliders.Length > 0)
        {
            foreach (Collider c in hitColliders)
            {
                if (c.gameObject.CompareTag("Enemy"))
                {
                    newTarget = c.transform;
                    break;
                }
            }
        }

        target = newTarget;
        if (target != null)
        {
            enemyScript = target.GetComponent<CabalAI>();
            player.camController.LookAt = target;
        }
    }

    private void UpdateLockOnCanvasPosition(PlayerController player)
    {
        if (target != null)
        {
            Vector3 lockOnPosition = target.position + Vector3.up * 1.5f;
            player.lockOnCanvas.position = lockOnPosition;
            player.lockOnCanvas.LookAt(player.transform.position);
        }
    }

    private void CheckForTargets(PlayerController player)
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, 12.5f);
        foreach (Collider c in hitColliders)
        {
            if (c.gameObject.CompareTag("Enemy"))
            {
                player.lockOnCanvas.gameObject.SetActive(true);
                player.camController.LookAt = target = c.gameObject.transform;
                enemyScript = target.GetComponent<CabalAI>();
                return; // Exit once the first target is found
            }
        }
        target = null; // Set target to null if no enemy is found
    }
}
