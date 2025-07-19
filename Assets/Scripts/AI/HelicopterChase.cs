using UnityEngine;

public class HelicopterChase : MonoBehaviour
{
    public GameObject helicopter;
    public Transform[] waypoints;
    public float speed = 10f;
    public float rotationSpeed = 2f;

    // AudioClips para os diferentes sons
    public AudioClip helicopterClip;
    public AudioClip dialogueClip;
    public AudioClip musicClip;
    public AudioClip arrivingDangerClip;
    public AudioClip entryDialogueClip;
    public AudioClip shootingClip;  // Som de disparo

    private Transform player;
    private PlayerStats playerStats;
    private int currentWaypointIndex;
    private bool isChasing = false;
    private float dialogueTimer = 0f;
    private float dialogueInterval = 45f;

    private AudioSource helicopterSource;
    private AudioSource dialogueSource;
    private AudioSource musicSource;
    private AudioSource arrivingDangerSource;
    private AudioSource entryDialogueSource;
    private AudioSource shootingSource;

    private float shootingTimer = 0f;
    private float shootingInterval = 5f;  // Intervalo para os disparos

    private float shootingRadius = 15f;  // Raio de 15 unidades para o disparo

    void Start()
    {
        // Criar e configurar os AudioSources no objeto do helicóptero
        helicopterSource = helicopter.AddComponent<AudioSource>();
        helicopterSource.clip = helicopterClip;
        helicopterSource.loop = true;
        helicopterSource.playOnAwake = false;
        helicopterSource.volume = 0.5f;  // Ajuste de volume

        dialogueSource = helicopter.AddComponent<AudioSource>();
        dialogueSource.clip = dialogueClip;
        dialogueSource.loop = false;
        dialogueSource.playOnAwake = false;
        dialogueSource.volume = 0.5f;  // Ajuste de volume

        musicSource = helicopter.AddComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.5f;  // Ajuste de volume

        arrivingDangerSource = helicopter.AddComponent<AudioSource>();
        arrivingDangerSource.clip = arrivingDangerClip;
        arrivingDangerSource.loop = false;
        arrivingDangerSource.playOnAwake = false;
        arrivingDangerSource.volume = 0.5f;  // Ajuste de volume

        entryDialogueSource = helicopter.AddComponent<AudioSource>();
        entryDialogueSource.clip = entryDialogueClip;
        entryDialogueSource.loop = false;
        entryDialogueSource.playOnAwake = false;
        entryDialogueSource.volume = 0.5f;  // Ajuste de volume

        shootingSource = helicopter.AddComponent<AudioSource>();
        shootingSource.clip = shootingClip;
        shootingSource.loop = false;
        shootingSource.playOnAwake = false;
        shootingSource.volume = 0.5f;  // Ajuste de volume
    }

    void Update()
    {
        if (isChasing)
        {
            ChasePlayer();
            PlayDialogueSound();
            CheckForShooting();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            playerStats = player.GetComponent<PlayerStats>();  // Referência ao PlayerStats
            isChasing = true;

            arrivingDangerSource.Play();
            helicopterSource.PlayDelayed(2.9f);
            entryDialogueSource.PlayDelayed(2f);
            musicSource.Play();

            FindClosestWaypoint();
        }
    }

    void ChasePlayer()
    {
        if (waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = (targetWaypoint.position - helicopter.transform.position).normalized;
        float distanceToWaypoint = Vector3.Distance(helicopter.transform.position, targetWaypoint.position);

        helicopter.transform.position = Vector3.MoveTowards(helicopter.transform.position, targetWaypoint.position, speed * Time.deltaTime);

        Vector3 playerDirection = (player.position - helicopter.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(playerDirection);
        helicopter.transform.rotation = Quaternion.Slerp(helicopter.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (distanceToWaypoint < 1f)
        {
            FindClosestWaypoint();
        }
    }

    void FindClosestWaypoint()
    {
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float distance = Vector3.Distance(player.position, waypoints[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                currentWaypointIndex = i;
            }
        }
    }

    void PlayDialogueSound()
    {
        dialogueTimer += Time.deltaTime;
        if (dialogueTimer >= dialogueInterval)
        {
            dialogueSource.Play();
            dialogueTimer = 0f;
        }
    }

    void CheckForShooting()
    {
        if (player == null) return;  // Garantir que o player foi detectado

        // Verificar se o player está dentro do raio de detecção
        float distanceToPlayer = Vector3.Distance(helicopter.transform.position, player.position);

        if (distanceToPlayer <= shootingRadius)
        {
            shootingTimer += Time.deltaTime;
            if (shootingTimer >= shootingInterval)
            {
                shootingSource.Play();  // Tocar o som de disparo
                playerStats.DecreaseHealth(10);  // Retirar 10 de Health
                shootingTimer = 0f;  // Reiniciar o temporizador
            }
        }
        else
        {
            shootingTimer = 0f;  // Resetar o temporizador caso o jogador saia do raio
        }
    }
}
