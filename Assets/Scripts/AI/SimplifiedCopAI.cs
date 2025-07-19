using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SimplifiedCopAI : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    private Transform player;
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    public float detectionRadius = 10f;
    public float arrestDistance = 2f;

    private enum AIState { Idle, Arrest }
    private AIState currentState;

    public AudioClip arrestSound;
    public AudioClip detectionSound;
    public AudioClip radioSound;
    public AudioClip eliminationSound;

    private AudioClip lastPlayedClip;
    private bool playerDetected = false;

    public string sceneToLoad = "GameOverMenu";

    private bool isInNeckBreakZone = false;
    private bool enemyEliminated = false;

    // Adicionar referência ao item que será dropado
    public GameObject itemToDrop;

    // Adicionar o ponto de referência para posicionar o jogador
    public Transform neckBreakPosition;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerAnimator = player.GetComponent<Animator>();
            playerRigidbody = player.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogError("Player com tag 'Player' não encontrado!");
        }

        currentState = AIState.Idle;
        StartCoroutine(PlayRadioSound());
    }

    private void Update()
    {
        if (player == null) return;

        DetectPlayer();
        HandleState();

        if (isInNeckBreakZone && Input.GetButtonDown("Action") && !enemyEliminated && playerAnimator.GetBool("isStealth"))
        {
            StartCoroutine(PerformNeckBreak());
        }
    }

    private void DetectPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRadius)
        {
            bool isPlayerStealth = playerAnimator.GetBool("isStealth");
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (!isPlayerStealth && angleToPlayer <= 300f)
            {
                if (currentState != AIState.Arrest)
                {
                    currentState = AIState.Arrest;

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
            case AIState.Arrest:
                HandleArrestState();
                break;
        }
    }

    private void HandleIdleState()
    {
        animator.Play("Idle");
    }

    private void HandleArrestState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("CopDrawGun"))
        {
            // Faz o policial se virar imediatamente para o jogador
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            transform.rotation = lookRotation; // Rotaciona imediatamente para o jogador

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

        // Desabilitar o comportamento de IA durante a execução da animação
        this.enabled = false;

        // Posicionar o jogador no ponto de referência antes da animação
        player.position = neckBreakPosition.position;
        player.rotation = neckBreakPosition.rotation;

        // Adicionar um leve atraso para garantir que o jogador esteja bem posicionado
        yield return new WaitForSeconds(0.2f);

        // Iniciar a animação de "NeckBreak"
        playerAnimator.Play("NeckBreak");
        animator.Play("NeckBreak");

        yield return new WaitForSeconds(1f);

        PlaySound(eliminationSound);

        // Instanciar o item na posição do inimigo após a eliminação
        if (itemToDrop != null)
        {
            Instantiate(itemToDrop, transform.position, Quaternion.identity);
        }

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
