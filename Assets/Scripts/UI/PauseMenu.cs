using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject bgMainScreen;
    private bool isPaused = false;

    void Update()
    {
        InputPause();
    }

    private void InputPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }


    private void Pause()
    {
        bgMainScreen.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        bgMainScreen.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Options()
    {
        Debug.Log("Options Menu");
    }

    public void Exit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}