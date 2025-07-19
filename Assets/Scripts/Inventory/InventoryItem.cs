using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryItem : MonoBehaviour
{
    public string itemName;
    public bool destroyOnUse;

    public Sprite sprite;
    public GameObject inventoryModel;

    public int type = 0;   // 0 - keyItem, 1 - Health/weapon, 2 - menu

    private PlayerInventory inventory;
    private PickUICam pickCam;

    // Updated to be protected and virtual so it can be overridden in derived classes
    protected virtual void Start()
    {
        pickCam = GameObject.FindObjectOfType<PickUICam>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (type == 2) return; // Type 2 is a menu, not a key or actionable item

        if (other.transform.CompareTag("Player"))
        {
            if (Input.GetButtonDown("Action"))
            {
                // Check the height of the item relative to the player
                float playerHeight = other.transform.position.y;
                float itemHeight = transform.position.y;
                Animator anim = other.gameObject.GetComponent<Animator>();

                if (itemHeight > playerHeight + 1.0f) // Adjust this height as needed
                {
                    anim.SetTrigger("PickupMedium");
                }
                else
                {
                    anim.SetTrigger("PickUp");
                }

                // Invoke the door opening function if it's a relevant item
                Door door = other.GetComponentInChildren<SimpleDoorKey>();
                if (door != null)
                {
                    door.OpenDoor(other.GetComponent<PlayerController>());
                }
                else
                {
                    gameObject.GetComponent<Collider>().enabled = false;
                    inventory = other.gameObject.GetComponent<PlayerInventory>();
                    Invoke("PickedItem", 0.5f);
                }
            }
        }
    }

    private void PickedItem()
    {
        inventory.AddItem(this);
        pickCam.SetAndEnable(gameObject);
        gameObject.SetActive(false);
    }

    // Abstract method that derived classes must implement
    public abstract void Use(PlayerController player);
}
