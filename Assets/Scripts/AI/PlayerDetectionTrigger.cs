using UnityEngine;

public class PlayerDetectionTrigger : MonoBehaviour
{
    public CopParisAI copAI; // Referência ao script do AI
    private Animator playerAnimator; // Referência ao Animator do player

    private void Start()
    {
        // Obtém a referência ao Animator do player no Start
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que entrou no trigger tem a tag "Player"
        if (other.CompareTag("Player"))
        {
            // Verifica se o player está no modo Stealth
            bool isPlayerStealth = playerAnimator.GetBool("isStealth");

            // Se o player NÃO estiver em modo Stealth, informa ao AI que ele foi detectado fora do raio de detecção
            if (!isPlayerStealth)
            {
                copAI.PlayerDetectedOutsideDetectionRadius();
            }
        }
    }
}
