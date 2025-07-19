using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUICam : MonoBehaviour
{
    public Transform objectPoint; // Ponto onde o objeto será exibido
    private GameObject item; // Referência ao objeto exibido

    void Start()
    {
        item = null;
    }

    void Update()
    {
        if (item != null)
        {
            // Faz o objeto girar lentamente
            item.transform.Rotate(0f, 90f * Time.deltaTime, 0f);
        }
    }

    public void SetAndEnable(GameObject prefab)
    {
        // Instancia o objeto no ponto especificado
        item = Instantiate(prefab, objectPoint);

        // Posiciona o objeto no centro e escala 1.5x maior
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale *= 1f; // Aumenta o objeto em 1.5x
        item.layer = 5; // Define a layer para UI

        // Define a layer do primeiro filho, caso exista
        if (item.transform.childCount != 0)
        {
            item.transform.GetChild(0).gameObject.layer = 5;
        }

        // Destroi o objeto após 3 segundos
        Destroy(item, 5f);
    }
}

