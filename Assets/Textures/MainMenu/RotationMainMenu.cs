using UnityEngine;

public class RotationMainMenu : MonoBehaviour
{
    // Velocidade de rotação em graus por segundo
    public float rotationSpeed = 10f;

    void Update()
    {
        // Calcula a rotação a ser aplicada com base no tempo desde o último frame
        float rotationAmount = rotationSpeed * Time.deltaTime;

        // Aplica a rotação ao eixo Z
        transform.Rotate(0, 0, rotationAmount);
    }
}
