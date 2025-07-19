using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100;
    public Canvas canvas;
    public RectTransform healthBar;

    private int health;
    private PlayerController player;

    private void Start()
    {
        health = maxHealth;
        player = GetComponent<PlayerController>();
    }

    public int Health
    {
        get { return health; }
        set
        {
            health = value;

            if (health > maxHealth)
                health = maxHealth;
            else if (health <= 0)
            {
                health = 0;
                player.StateMachine.GoToState<Dead>();
                HideCanvas(); // Optionally hide the canvas when health reaches zero
            }

            // Update health bar UI
            healthBar.localScale = new Vector3((float)health / maxHealth, 1f, 1f);
        }
    }

    public void DecreaseHealth(int amount)
    {
        // Reduce health by the specified amount
        Health -= amount;
	canvas.enabled = true;
    }

    public void HideCanvas()
    {
        // Hide the canvas when this method is called
        if (canvas != null)
        {
            canvas.enabled = false;
        }
    }

    public void ShowCanvas()
    {
        // Show the canvas when this method is called
        if (canvas != null)
        {
            canvas.enabled = true;
        }
    }
}
