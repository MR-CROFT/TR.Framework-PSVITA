using UnityEngine;
using UnityEngine.AI;

public class DogAI : MonoBehaviour
{
    public float detectionRadius = 15f;  // Raio de detecção do Player
    public float barkRange = 1f;         // Alcance do latido
    public float health = 100f;          // Vida do cão

    private NavMeshAgent agent;          // Agente para movimentação
    private Animator animator;           // Animator para animações
    public Transform player;             // Referência ao Player

    public AudioClip barkSFX;            // Som de latido
    public AudioClip sniffSFX;           // Som de cheirar
    private AudioSource audioSource;     // Componente para tocar som

    private bool isDead = false;         // Verificação se o cão está morto
    private float sniffCooldown = 5f;    // Tempo entre animações de Sniff
    private float sniffTimer = 0f;
    private bool hasBarked = false;      // Controle para garantir que o latido seja reproduzido apenas uma vez

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>(); // Pega o AudioSource no GameObject
        audioSource.enabled = false; // Desabilita o AudioSource inicialmente
    }

    private void Update()
    {
        if (isDead) return;  // Se o cão estiver morto, não executar nada

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Habilita o som quando o jogador está dentro do raio de detecção
        if (distanceToPlayer <= detectionRadius)
        {
            if (!audioSource.enabled)
            {
                audioSource.enabled = true; // Habilita o áudio se estiver dentro do raio de detecção
            }

            if (distanceToPlayer <= barkRange)
            {
                // Está no alcance de latir, então para o movimento e late
                Bark();
            }
            else
            {
                // Se o player sair do barkRange, resetar o controle de latido
                hasBarked = false;
                MoveTowardsPlayer();
            }
        }
        else
        {
            // Desabilita o som quando o jogador está fora do raio de detecção
            if (audioSource.enabled)
            {
                audioSource.enabled = false; // Desabilita o áudio se estiver fora do raio de detecção
            }

            // Fora do alcance de detecção, anda ou fica parado e executa Sniff
            if (agent.remainingDistance > 0.1f)
            {
                animator.SetBool("isWalking", true);
                animator.SetBool("isRunning", false);
            }
            else
            {
                animator.SetBool("isWalking", false);
                PlayRandomSniffAnimation();  // Executa a animação de Sniff aleatória
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        agent.isStopped = false; // Certificar que o agente não está parado
        agent.SetDestination(player.position); // Define o destino para o player
        animator.SetBool("isRunning", true);   // Ativa a animação de corrida
        animator.SetBool("isWalking", false);  // Desativa a animação de andar
    }

    private void Bark()
    {
        // Parar o movimento durante o latido
        if (!agent.isStopped)
        {
            agent.isStopped = true; // Certifica que o movimento para quando o player está dentro do barkRange
        }

        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetTrigger("Bark");  // Trigger para animação de latido

        // Toca o som de latido apenas uma vez, verificando se o AudioSource está habilitado
        if (!hasBarked && barkSFX != null && audioSource.enabled)
        {
            audioSource.PlayOneShot(barkSFX);
            hasBarked = true;  // Marca que o latido foi executado
        }
    }

    private void PlayRandomSniffAnimation()
    {
        sniffTimer -= Time.deltaTime;
        if (sniffTimer <= 0f && audioSource.enabled)
        {
            // Toca a animação de Sniff e o som, verificando se o AudioSource está habilitado
            animator.SetTrigger("Sniff");  // Trigger para animação de Sniff
            if (sniffSFX != null)
            {
                audioSource.PlayOneShot(sniffSFX);
            }
            // Reseta o timer
            sniffTimer = sniffCooldown;
        }
    }

    private void ResumeMovement()
    {
        agent.isStopped = false; // Certificar que o agente pode se mover novamente
    }

    // Função para o cão receber dano
    public void TakeDamage(float damage)
    {
        if (isDead) return;  // Se já estiver morto, não tomar mais dano

        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    // Função para destruir o cão quando ele morrer
    private void Die()
    {
        isDead = true;
        agent.isStopped = true;
        animator.SetTrigger("Die");

        // Desativa o agente e o collider para evitar interações adicionais
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;

        // Destruir o objeto após um tempo para permitir a animação de morte
        Destroy(gameObject, 3f);  // Ajuste o tempo conforme necessário para a animação "Die"
    }

    // Gizmos para visualizar o alcance de detecção no editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, barkRange);
    }
}
