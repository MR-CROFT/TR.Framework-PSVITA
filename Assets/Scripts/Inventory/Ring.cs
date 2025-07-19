using UnityEngine;

public class Ring : MonoBehaviour
{
    public bool moving;
    public Transform targetObject; // Objeto alvo que será usado para verificar a proximidade.
    private float tim = 2f;
    private int direction = 0;
    private int type = 0;
    private float rotDirection = 0;
    private float curRot = 0;
    private float tarRot = 0;
    private float curPos = 0;
    private float tarPos = 0;
    private Transform closestChild; // Filho mais próximo

    public void Init()
    {
        curRot = 0;
        curPos = 0;
        tarRot = 0;
        tarPos = 0;
        closestChild = null;
    }

    void Update()
    {
        tim += Time.deltaTime;

        if (Mathf.Abs(curPos - tarPos) > 0.05f)
        {
            int childCount = transform.childCount;

            // Aumentar a velocidade de transição vertical
            float verticalSpeed = 10f * Time.deltaTime;  // Original: 7f
            transform.Translate(0f, Mathf.Min(Mathf.Abs(verticalSpeed), Mathf.Abs(tarPos - curPos)) * Mathf.Sign(tarPos - curPos), 0f);
            curPos += Mathf.Min(Mathf.Abs(verticalSpeed), Mathf.Abs(tarPos - curPos)) * Mathf.Sign(tarPos - curPos);

            // Rotação contínua
            transform.Rotate(0f, 240f * Time.deltaTime, 0f);  // Original: 180f
            curRot += 240f * Time.deltaTime;
            curRot %= 360;
            rotDirection = 1;
            float radius = 0;
            switch (type)
            {
                case 1: radius = Mathf.Max(0.1f, (1 - tim * 2.8f)) * 2f; break;
                case 2: radius = Mathf.Min(2f, tim * 2.8f); break;
                default: break;
            }
            for (int i = 0; i < childCount; i++)
            {
                transform.GetChild(i).localPosition = new Vector3(radius * Mathf.Sin(2 * i * Mathf.PI / childCount), 0f, radius * Mathf.Cos(2 * i * Mathf.PI / childCount));
            }
        }
        else
        {
            direction = 0;
            if (rotDirection == 0) moving = false;
        }

        // Aumentar a velocidade de rotação horizontal
        float rotationSpeed = 4.6f * rotDirection;  // Original: 3.6f
        if (Mathf.Abs(rotationSpeed) > Mathf.Abs(tarRot - curRot))
        {
            transform.Rotate(0f, tarRot - curRot, 0f);
            curRot = tarRot;
            rotDirection = 0;
            if (direction == 0) moving = false;
            return;
        }
        transform.Rotate(0f, -rotationSpeed, 0f);
        curRot -= rotationSpeed;
        curRot %= 360;

        // Verifica o filho mais próximo e aplica a rotação contínua
        if (targetObject != null)
        {
            closestChild = GetClosestChild();

            if (closestChild != null)
            {
                closestChild.Rotate(0f, 100f * Time.deltaTime, 0f); // Animação de rotação contínua
            }
        }
    }

    private Transform GetClosestChild()
    {
        float minDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (Transform child in transform)
        {
            float distance = Vector3.Distance(child.position, targetObject.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = child;
            }
        }

        return closest;
    }

    public void Move(int pDirection, int pType)
    {
        direction = pDirection;
        tarPos += pDirection * 5f;
        tarRot = 0;
        type = pType;
        tim = 0f;
        moving = true;
    }

    public void RotateOneStep(int pRotRirection)
    {
        int childCount = transform.childCount;
        rotDirection = pRotRirection;
        tarRot -= rotDirection * 360f / childCount;
        tarRot %= 360;
        moving = true;
        tim = 0f;
    }
}