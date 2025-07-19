using UnityEngine;
using System.Collections;

public class Vault : MonoBehaviour
{
    private bool canVault = false;           // Flag to check if player can vault
    private PlayerController player;         // Reference to the PlayerController
    public string vaultAnimationName;        // Name of the vault animation (set in Inspector)
    private Animator playerAnimator;

    private void Start()
    {
        // Ensure playerAnimator is null at start
        playerAnimator = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            playerAnimator = other.GetComponent<Animator>();

            if (player != null && playerAnimator != null)
            {
                canVault = true;
                playerAnimator.SetBool("isNearVault", true);
            }
            else
            {
                Debug.LogError("Player or Animator component not found on player.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (player != null && playerAnimator != null)
            {
                canVault = false;
                playerAnimator.SetBool("isNearVault", false);
                player = null;
                playerAnimator = null;
            }
        }
    }

    private void Update()
    {
        if (canVault && Input.GetButtonDown("Action")) // Using a generic "Action" button
        {
            TriggerVault();
        }
    }

    private void TriggerVault()
    {
        player.DisableCharControl();                   // Disable player control
        playerAnimator.applyRootMotion = true;         // Enable root motion for animation movement
        playerAnimator.Play(vaultAnimationName);       // Play vault animation
        StartCoroutine(ReEnableControlAfterVault());   // Coroutine to re-enable control
    }

    private IEnumerator ReEnableControlAfterVault()
    {
        // Wait for the vault animation to complete
        float animationLength = playerAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);

        playerAnimator.applyRootMotion = false;        // Disable root motion after the animation
        player.EnableCharControl();                    // Re-enable player control
    }
}
