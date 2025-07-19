using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float delay = 3f; // Tempo até a explosão
    public float explosionRadius = 5f; // Raio da explosão
    public float explosionForce = 700f; // Força da explosão
    public int damage = 50; // Dano causado pela explosão

    public GameObject explosionEffect; // Efeito visual da explosão
    public AudioClip explosionSound; // Som da explosão

    private bool hasExploded = false;
    private float countdown;

    private void Start()
    {
        countdown = delay;
    }

    private void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0f && !hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        // Mostrar o efeito da explosão
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        // Tocar o som da explosão
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Detectar todos os objetos dentro do raio de explosão
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            // Aplicar força nos objetos com Rigidbody
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // Causar dano ao jogador ou outros objetos com health
            PlayerStats playerStats = nearbyObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.DecreaseHealth(damage);
            }

            // Adicionar lógica para outros tipos de dano a inimigos ou objetos destrutíveis
            // Inimigo inimigo = nearbyObject.GetComponent<Inimigo>();
            // if (inimigo != null)
            // {
            //     inimigo.TakeDamage(damage);
            // }
        }

        // Destruir a granada após a explosão
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
