using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 10;  // Amount of ammo to provide

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.currentWeaponItem != null)
            {
                player.currentWeaponItem.currentAmmo += ammoAmount;
                if (player.currentWeaponItem.currentAmmo > player.currentWeaponItem.maxAmmo)
                {
                    player.currentWeaponItem.currentAmmo = player.currentWeaponItem.maxAmmo;
                }
                Destroy(gameObject);
            }
        }
    }
}
