using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CopParisAI : MonoBehaviour
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
    public AudioClip eliminationSound;  // Variável para o som de eliminação

    private AudioClip lastPlayedClip;
    private bool playerDetected = false;

    public string sceneToLoad = "GameOverMenu";

    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;

    private bool isInNeckBreakZone = false; // Flag para verificar se o jogador está na zona de NeckBreak
    private bool enemyEliminated = false; // Flag para garantir que o inimigo seja eliminado uma única vez

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

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

        // Verifica se o jogador pressionou o botão de ação, está na zona de NeckBreak e está em stealth mode
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

            // Verifica se o player está atrás do inimigo
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            // O jogador só é detectado se não estiver em stealth e não estiver atrás do inimigo
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

        // Se o agente estiver quase no ponto de patrulha e não estiver aguardando na corrotina, inicia a espera
        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        currentState = AIState.Idle; // Define o estado como Idle enquanto espera
        agent.isStopped = true; // Para o agente
        animator.SetTrigger("LookAround"); // Toca a animação LookAround durante a espera

        yield return new WaitForSeconds(3f); // Aguarda por 3 segundos

        agent.isStopped = false; // Reinicia o agente
        GoToNextPatrolPoint(); // Vai para o próximo ponto de patrulha
        currentState = AIState.Walk; // Retorna ao estado de Walk
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

            // Inicia a corrotina para aguardar 9 segundos e depois carregar a nova cena
            StartCoroutine(LoadGameOverSceneAfterDelay(9f));
        }
    }

    private IEnumerator PerformNeckBreak()
    {
        enemyEliminated = true; // Marca o inimigo como eliminado

        // Desativa o agente e a detecção para impedir outras ações
        agent.isStopped = true;
        this.enabled = false;

        // Sincroniza as animações
        playerAnimator.Play("NeckBreak");
        animator.Play("NeckBreak");

        // Aguarda 0.5 segundos antes de tocar o som de eliminação
        yield return new WaitForSeconds(1f);

        // Toca o som de eliminação
        PlaySound(eliminationSound);

        // Espera a duração da animação do NeckBreak (ajuste conforme necessário)
        yield return new WaitForSeconds(15f);

        // Destrói o inimigo
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Marca que o jogador está na zona de NeckBreak
            isInNeckBreakZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Desmarca que o jogador está na zona de NeckBreak
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
}
