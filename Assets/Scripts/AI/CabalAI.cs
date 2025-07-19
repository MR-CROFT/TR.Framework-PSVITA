using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CabalAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private Collider enemyCollider;
    private Rigidbody enemyRigidbody; // New: Reference to the Rigidbody

    public Transform player;
    private PlayerStats playerStats;
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;

    public float detectionRadius = 15f;
    public float attackDistance = 10f;
    public float fireRate = 0.1f;
    public int damagePerShot = 10;
    public int shotsBeforeReload = 15;
    public float reloadDuration = 2f;
    public AudioClip reloadSound;

    private int currentShots = 0;

    public GameObject itemToDrop;
    public GameObject grenadePrefab;
    public Transform grenadeLaunchPoint;

    public bool canLaunchGrenade = true;
    public float grenadeLaunchDelay = 10f;
    public AudioClip grenadeLaunchSFX;

    private enum AIState { Idle, Walk, Run, Search, Attack, Reload }
    private AIState currentState;

    public Transform[] patrolPoints;
    private int currentPatrolIndex;

    public AudioClip detectionSound;
    public AudioClip searchSound;
    public AudioClip shootSound;
    public AudioClip eliminationSound;
    public AudioClip radioSound;

    private AudioClip lastPlayedClip;
    private bool playerDetected = false;
    private bool isShooting = false;

    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;

    private bool isInNeckBreakZone = false;
    private bool enemyEliminated = false;

    public int maxHealth = 100;
    public int currentHealth;
    private bool isDead = false;

    private bool radioPlayed = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        enemyCollider = GetComponent<Collider>();
        enemyRigidbody = GetComponent<Rigidbody>(); // New: Get the Rigidbody component

        currentHealth = maxHealth;

        currentState = AIState.Idle;
        GoToNextPatrolPoint();

        playerStats = player.GetComponent<PlayerStats>();
        playerAnimator = player.GetComponent<Animator>();
        playerRigidbody = player.GetComponent<Rigidbody>();

        StartCoroutine(PlaySearchSound());
    }

    private void Update()
    {
        if (isDead) return;

        DetectPlayer();
        HandleState();

        if (isInNeckBreakZone && Input.GetButtonDown("Action") && !enemyEliminated && playerAnimator.GetBool("isStealth"))
        {
            StartCoroutine(PerformNeckBreak());
        }
    }

    private void DetectPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRadius)
        {
            bool isPlayerStealth = playerAnimator.GetBool("isStealth");

            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (!isPlayerStealth && angleToPlayer <= 90f)
            {
                if (currentState != AIState.Run && currentState != AIState.Attack)
                {
                    currentState = AIState.Run;
                    agent.isStopped = false;

                    if (!playerDetected)
                    {
                        PlaySound(detectionSound);
                        playerDetected = true;

                        if (canLaunchGrenade)
                        {
                            StartCoroutine(LaunchGrenadeAfterDelay(grenadeLaunchDelay));
                        }
                    }
                }
            }
        }
    }

    private void HandleState()
    {
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState();
                break;
            case AIState.Walk:
                HandleWalkState();
                break;
            case AIState.Run:
                HandleRunState();
                break;
            case AIState.Search:
                HandleSearchState();
                break;
            case AIState.Attack:
                HandleAttackState();
                break;
            case AIState.Reload:
                HandleReloadState();
                break;
        }
    }

    private void HandleIdleState()
    {
        animator.Play("Idle");
        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            currentState = AIState.Walk;
            GoToNextPatrolPoint();
        }
    }

    private void HandleWalkState()
    {
        animator.Play("Walk");
        agent.speed = walkSpeed;

        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        currentState = AIState.Idle;
        agent.isStopped = true;
        animator.SetTrigger("LookAround");

        yield return new WaitForSeconds(3f);

        agent.isStopped = false;
        GoToNextPatrolPoint();
        currentState = AIState.Walk;
    }

    private void HandleRunState()
    {
        animator.Play("Run");
        agent.speed = runSpeed;
        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            currentState = AIState.Attack;
            agent.isStopped = true;
        }
    }

    private void HandleSearchState()
    {
        animator.Play("Search");
        agent.speed = walkSpeed;

        if (!playerDetected && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
            currentState = AIState.Walk;
        }
    }

    private void HandleAttackState()
    {
        animator.Play("Attack");

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        if (!isShooting)
        {
            StartCoroutine(ShootPlayer());
        }

        if (Vector3.Distance(transform.position, player.position) > attackDistance)
        {
            currentState = AIState.Run;
            agent.isStopped = false;
        }
    }

    private void HandleReloadState()
    {
        animator.SetTrigger("Reload");
        PlaySound(reloadSound);

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(reloadDuration);

        currentShots = 0;
        currentState = AIState.Attack;
    }

    private IEnumerator ShootPlayer()
    {
        isShooting = true;

        while (currentState == AIState.Attack && Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            if (currentShots >= shotsBeforeReload)
            {
                currentState = AIState.Reload;
                isShooting = false;
                yield break;
            }

            if (playerStats != null && playerStats.Health > 0)
            {
                float hitChance = CalculateHitChance();
                if (Random.value <= hitChance)
                {
                    PlaySound(shootSound);
                    playerStats.DecreaseHealth(damagePerShot);
                }

                currentShots++;

                yield return new WaitForSeconds(fireRate);
            }
            else
            {
                if (!radioPlayed)
                {
                    radioPlayed = true;
                    PlaySound(radioSound);
                    animator.SetTrigger("LookAround"); 
                }
                isShooting = false;
                currentState = AIState.Search;
                yield break;
            }
        }

        isShooting = false;
    }

    private float CalculateHitChance()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float maxHitChanceDistance = detectionRadius;
        float minHitChance = 0.2f;

        float hitChance = 1f - ((distanceToPlayer / maxHitChanceDistance) * (1f - minHitChance));
        return Mathf.Clamp(hitChance, minHitChance, 1f);
    }

    private IEnumerator PerformNeckBreak()
    {
        enemyEliminated = true;

        agent.isStopped = true;
        this.enabled = false;

        playerAnimator.Play("NeckBreak");
        animator.Play("NeckBreak");

        yield return new WaitForSeconds(1f);

        PlaySound(eliminationSound);

        DropItem();

        yield return new WaitForSeconds(15f);

        Destroy(gameObject);
    }

    private void DropItem()
    {
        if (itemToDrop != null)
        {
            Instantiate(itemToDrop, transform.position, Quaternion.identity);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("Enemy took damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Enemy died.");

        // Stop all current activities
        StopAllCoroutines();

        // Disable AI-related components
        agent.isStopped = true;
        agent.enabled = false;
        this.enabled = false;

        // Disable collisions and physics interactions
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        if (enemyRigidbody != null)
        {
            enemyRigidbody.isKinematic = true; // Prevent the body from interacting with physics
        }

        // Trigger the death animation
        animator.SetTrigger("Die");
        animator.SetBool("isDead", true);

        // Play death sound if available
        if (eliminationSound != null)
        {
            PlaySound(eliminationSound);
        }
        else
        {
            Debug.LogWarning("Elimination sound is not set.");
        }

        // Destroy the enemy object after a delay
        StartCoroutine(DelayedDestroy());
    }

    private IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(15f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInNeckBreakZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInNeckBreakZone = false;
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
            return;

        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == lastPlayedClip && audioSource.isPlaying)
            return;

        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
            lastPlayedClip = clip;
        }
    }

    private IEnumerator PlaySearchSound()
    {
        while (true)
        {
            float delay = Random.Range(120f, 180f);
            yield return new WaitForSeconds(delay);
            PlaySound(radioSound);
        }
    }

    private IEnumerator LaunchGrenadeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerDetected && currentState == AIState.Run && grenadePrefab != null && grenadeLaunchPoint != null)
        {
            Instantiate(grenadePrefab, grenadeLaunchPoint.position, grenadeLaunchPoint.rotation);
            PlaySound(grenadeLaunchSFX);

            yield return new WaitForSeconds(2f);
            currentState = AIState.Attack;
        }
    }
}
