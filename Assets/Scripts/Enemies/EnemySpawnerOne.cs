using UnityEngine;

public class EnemySpawnerOne : MonoBehaviour
{
    // Prefab do inimigo
    public GameObject enemyPrefab;
    
    // Local onde os inimigos serão spawnados (você pode também gerar uma posição aleatória)
    public Transform spawnPoint;
    
    // Tempo de espera antes do primeiro spawn (em segundos)
    public float spawnDelay = 180f; // 3 minutos

    // Função chamada no início
    void Start()
    {
        // Chama o método SpawnEnemy após o delay especificado
        Invoke("SpawnEnemy", spawnDelay);
    }

    // Método para spawnar o inimigo
    void SpawnEnemy()
    {
        // Instancia o inimigo na posição e rotação do spawnPoint
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // (Opcional) Se quiser continuar spawnando inimigos, pode chamar Invoke ou iniciar uma nova coroutine aqui
    }
}
