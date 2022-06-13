using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    public CharacterMover thisChar;
    public GameObject startPanel;
    public GameObject pausePanel;
    public GameObject failPanel;
    public GameObject completePanel;
    public static bool fail;
    public static bool complete;
    // Start is called before the first frame update
    void Start()
    {
        fail = false;
        complete = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (fail) {
            failPanel.SetActive(true);
            Time.timeScale = 0;
        }

        if (complete) {
            Time.timeScale = 0;
            completePanel.SetActive(true);
        }
    }

    public void PauseButton() {
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }

    public void ResumeButton()
    {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }

    public void RestartButton()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void TapToStart() {
        startPanel.SetActive(false);
        Time.timeScale = 1;
        thisChar.gameStart = true;
    }
}
