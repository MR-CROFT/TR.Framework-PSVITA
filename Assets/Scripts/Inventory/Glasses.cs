using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glasses : InventoryItem
{
    public override void Use(PlayerController player)
    {
        // Find all the SaveCrystal objects in the scene
        GameObject[] saveCrystals = GameObject.FindGameObjectsWithTag("SaveCrystal");

        // Activate each SaveCrystal
        foreach (GameObject crystal in saveCrystals)
        {
            crystal.SetActive(true);
        }

        // You can add any additional logic here if needed.
        // The item remains in the inventory as we don't set destroyOnUse to true.
    }
}
