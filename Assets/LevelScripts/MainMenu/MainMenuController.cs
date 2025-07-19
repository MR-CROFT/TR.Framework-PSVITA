using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject optionsMenu;
    public GameObject mainMenu;

    void Start ()
    {
		
    }
	
    void Update ()
    {
		
    }

    public void GameOver()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Level1()
    {
        SceneManager.LoadScene("EndTutorial");
    }

    public void Level2()
    {
        SceneManager.LoadScene("ParisFinal");
    }

    public void WatchTrailer()
    {
        SceneManager.LoadScene("VonCroyBegins");
    }

    
    public void ShowOptions()
    {
        optionsMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
