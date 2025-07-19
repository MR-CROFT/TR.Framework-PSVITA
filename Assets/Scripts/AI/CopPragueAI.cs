using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CopPragueAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;

    public Transform player;
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    public float detectionRadius = 10f;
    public float arrestDistance = 2f;

    private enum AIState { Idle, Walk, Run, Arrest }
    private AIState currentState;

    public Transform[] patrolPoints;
    private int currentPatrolIndex;

    public AudioClip arrestSound;
    public AudioClip detectionSound;
    public AudioClip radioSound;
    public AudioClip eliminationSound;

    private AudioClip lastPlayedClip;
    private bool playerDetected = false;

    public string sceneToLoad = "GameOverMenu";

    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;

    private bool isInNeckBreakZone = false;
    private bool enemyEliminated = false;

    // Health-related variables
    public int startHealth = 100;
    private int health;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        health = startHealth;

        currentState = AIState.Idle;
        GoToNextPatrolPoint();

        playerAnimator = player.GetComponent<Animator>();
        playerRigidbody = player.GetComponent<Rigidbody>();

        StartCoroutine(PlayRadioSound());
    }

    private void Update()
    {
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
                if (currentState != AIState.Run && currentState != AIState.Arrest)
                {
                    currentState = AIState.Run;
                    agent.isStopped = false;

                    if (!playerDetected)
                    {
                        PlaySound(detectionSound);
                        playerDetected = true;
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
            case AIState.Arrest:
                HandleArrestState();
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

        if (Vector3.Distance(transform.position, player.position) <= arrestDistance)
        {
            currentState = AIState.Arrest;
            agent.isStopped = true;
            PlaySound(arrestSound);
        }
    }

    private void HandleArrestState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("CopDrawGun"))
        {
            animator.Play("CopDrawGun");
            Debug.Log("Player Arrested!");

            if (playerAnimator != null)
            {
                playerAnimator.Play("Arrested");
            }

            if (playerRigidbody != null)
            {
                playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }

            StartCoroutine(LoadGameOverSceneAfterDelay(9f));
        }
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

    private IEnumerator LoadGameOverSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerRigidbody != null)
        {
            playerRigidbody.constraints = RigidbodyConstraints.None;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    public void PlayerDetectedOutsideDetectionRadius()
    {
        if (currentState != AIState.Run && currentState != AIState.Arrest)
        {
            currentState = AIState.Run;
            agent.isStopped = false;

            if (!playerDetected)
            {
                PlaySound(detectionSound);
                playerDetected = true;
            }
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

    private IEnumerator PlayRadioSound()
    {
        while (true)
        {
            yield return new WaitForSeconds(120f);
            PlaySound(radioSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // Funções relacionadas ao Health
    public int Health
    {
        get { return health; }
        set
        {
            health = value;
            if (health <= 0)
            {
                health = 0;
                animator.SetBool("isDead", true);

                // Destrói o inimigo após 5 segundos
                Destroy(gameObject, 5f);
            }
        }
    }

    // Método público para causar dano
    public void TakeDamage(int damage)
    {
        Health -= damage;
    }
}
