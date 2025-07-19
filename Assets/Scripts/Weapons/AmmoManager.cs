using UnityEngine;
using UnityEngine.UI; // Required for UI Text

public class AmmoManager : MonoBehaviour
{
    public int defaultAmmo = 50; // The initial amount of ammo
    private int currentAmmo; // The current amount of ammo

    public Text ammoDisplay; // UI Text component to display ammo

    private void Start()
    {
        ResetAmmo();
    }

    /// <summary>
    /// Resets the ammo to the default value and updates the display.
    /// </summary>
    public void ResetAmmo()
    {
        currentAmmo = defaultAmmo;
        UpdateAmmoDisplay(); // Update UI when resetting ammo
    }

    /// <summary>
    /// Attempts to use the specified amount of ammo. 
    /// Returns true if successful, false if there is not enough ammo.
    /// </summary>
    /// <param name="amount">The amount of ammo to use</param>
    /// <returns>True if ammo was successfully used, otherwise false</returns>
    public bool UseAmmo(int amount)
    {
        if (currentAmmo >= amount)
        {
            currentAmmo -= amount;
            UpdateAmmoDisplay(); // Update UI when using ammo
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds the specified amount of ammo and updates the display.
    /// </summary>
    /// <param name="amount">The amount of ammo to add</param>
    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        UpdateAmmoDisplay(); // Update UI when adding ammo
    }

    /// <summary>
    /// Gets the current amount of ammo.
    /// </summary>
    /// <returns>The current amount of ammo</returns>
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    /// <summary>
    /// Updates the UI text display for ammo.
    /// </summary>
    private void UpdateAmmoDisplay()
    {
        if (ammoDisplay != null)
        {
            ammoDisplay.text = $"{currentAmmo}";
        }
    }
}
