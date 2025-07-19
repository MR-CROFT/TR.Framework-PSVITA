using System.Collections;
using UnityEngine;

public class SparkleEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public float blinkInterval = 5.0f; // Tempo entre os brilhos
    public float blinkDuration = 1.5f; // Tempo total do efeito de brilho (subida e descida do brilho)

    public AnimationCurve brightnessCurve; // Curva para definir a intensidade do brilho

    public GameObject childObject;

    void Start()
    {
        spriteRenderer = childObject.GetComponent<SpriteRenderer>();
        StartCoroutine(BlinkSprite());
    }

    private IEnumerator BlinkSprite()
    {
        while (true)
        {
            // Espera até o próximo ciclo de brilho
            yield return new WaitForSeconds(blinkInterval);

            // Anima o brilho conforme a curva
            float timer = 0f;
            while (timer < blinkDuration)
            {
                timer += Time.deltaTime;
                float curveValue = brightnessCurve.Evaluate(timer / blinkDuration);
                
                SetSpriteAlpha(curveValue);

                yield return null; // Aguarda o próximo frame
            }

            // Garanta que o sprite fique invisível após o ciclo
            SetSpriteAlpha(0f);
        }
    }

    // Ajusta o alpha do sprite conforme o valor da curva
    private void SetSpriteAlpha(float alpha)
    {
        spriteRenderer.color = new Color(
            spriteRenderer.color.r,
            spriteRenderer.color.g,
            spriteRenderer.color.b,
            alpha
        );
    }
}
