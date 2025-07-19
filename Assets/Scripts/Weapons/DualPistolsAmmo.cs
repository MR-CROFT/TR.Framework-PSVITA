using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualPistolsAmmo : InventoryItem
{
    public int ammoAmount = 10; // Amount of ammo to add when picked up

    public override void Use(PlayerController player)
    {
        // Find the AmmoManager component in the player's game object
        AmmoManager ammoManager = player.GetComponent<AmmoManager>();

        if (ammoManager != null)
        {
            ammoManager.AddAmmo(ammoAmount); // Use AddAmmo instead of IncreaseAmmo
        }

        // Optionally destroy the item after use if needed
        if (destroyOnUse)
        {
            Destroy(gameObject);
        }
    }
}
