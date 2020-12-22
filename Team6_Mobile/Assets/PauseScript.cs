using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScript : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject PauseMenuUI;

    public GameObject Canvas;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnMouseUp()
    {
        Debug.Log("Pause");
        if (!GameIsPaused) Pause();
    }
    public void Resume()
    {
        Canvas.GetComponent<Canvas>().sortingOrder = -4;
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        Canvas.GetComponent<Canvas>().sortingOrder = 5;
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        Canvas.GetComponent<Canvas>().sortingOrder = 5;
        GameIsPaused = false;
        SceneManager.LoadScene("WorldMapScene");
    }
}
