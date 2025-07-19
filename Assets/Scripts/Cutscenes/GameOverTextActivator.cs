using UnityEngine;
using System.Collections;

public class GameOverTextActivator : MonoBehaviour
{
    public GameObject GameOver;
    public GameObject Tap;

    void Start()
    {
        // Start the coroutine to activate GameOver and Tap with delays.
        StartCoroutine(ActivateGameObjects());
    }

    IEnumerator ActivateGameObjects()
    {
        // Wait for 3 seconds before activating the GameOver GameObject.
        yield return new WaitForSeconds(2f);
        GameOver.SetActive(true);

        // Wait for an additional 2 seconds (5 seconds total) before activating the Tap GameObject.
        yield return new WaitForSeconds(5f);
        Tap.SetActive(true);
    }
}
