using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, 30.0f * Time.deltaTime, 0);
    }

}
