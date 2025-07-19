using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : InventoryItem
{
    public enum HandOption
    {
        RightHand,
        LeftHand,
        BothHands
    }

    [Header("Weapon Configuration")]
    public HandOption handOption;        // Option to select which hand(s) to equip the weapon

    public int damage;                   // Damage dealt by the weapon
    public int maxAmmo;                  // Maximum ammo capacity
    public int currentAmmo;              // Current ammo count
    private PlayerController player;     // Reference to the player
    
    public AudioClip shootSFX;           // Sound effect for shooting
    public AudioClip reloadSFX;          // Sound effect for reloading

    // Override Start to initialize WeaponItem-specific behavior
    protected override void Start()
    {
        base.Start();  // Calls InventoryItem's Start method
        currentAmmo = maxAmmo;  // Start with full ammo
    }

    public override void Use(PlayerController player)
    {
        this.player = player;

        // Desativar as armas atuais, se houver
        if (player.currentWeapon != null)
        {
            player.currentWeapon.SetActive(false);
        }

        // Equipar a nova arma conforme a opção selecionada
        EquipWeapon();
    }

    private void EquipWeapon()
    {
        switch (handOption)
        {
            case HandOption.RightHand:
                EquipRightHand();
                break;
            case HandOption.LeftHand:
                EquipLeftHand();
                break;
            case HandOption.BothHands:
                EquipBothHands();
                break;
        }
    }

    private void EquipRightHand()
    {
        GameObject gunHandRight = player.larad.transform.Find("GUNHAND_RIGHT").gameObject;
        gunHandRight.SetActive(true);

        player.currentWeapon = gunHandRight;
        player.currentWeaponItem = this;
    }

    private void EquipLeftHand()
    {
        GameObject gunHandLeft = player.larad.transform.Find("GUNHAND_LEFT").gameObject;
        gunHandLeft.SetActive(true);

        player.currentWeapon = gunHandLeft;
        player.currentWeaponItem = this;
    }

    private void EquipBothHands()
    {
        EquipRightHand();
        EquipLeftHand();
    }

    public void Shoot()
    {
        if (currentAmmo > 0)
        {
            // Tocar o som de disparo
            PlaySound(shootSFX);

            currentAmmo--; // Diminuir a contagem de munição
            // Adicionar lógica de disparo aqui (por exemplo, raycasting para acertos)
        }
        else
        {
            Debug.Log("Out of ammo! Reload required.");
            // Opcional: tocar um som de "sem munição" aqui
        }
    }

    public void Reload()
    {
        currentAmmo = maxAmmo; // Recarregar a munição
        
        // Tocar o som de recarregamento
        PlaySound(reloadSFX);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && player != null)
        {
            AudioSource.PlayClipAtPoint(clip, player.transform.position);
        }
    }
}
