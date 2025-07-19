using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;  // Maximum health of the enemy
    private int currentHealth;   // Current health of the enemy

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle the enemy's death (animation, removing from scene, etc.)
        Destroy(gameObject);
    }
}
