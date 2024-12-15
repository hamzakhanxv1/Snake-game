using UnityEngine;
using UnityEngine.UI;

public class GamePauseManager : MonoBehaviour
{
    public Button PauseButton;
    public Button ResumeButton;
    private bool isPaused = false;



    private void Start()
    {
        // Add listeners to the buttons
        PauseButton.onClick.AddListener(PauseGame);
        ResumeButton.onClick.AddListener(ResumeGame);
        
        // Ensure resumeButton is hidden at the start
        ResumeButton.gameObject.SetActive(false);
    }

    public void PauseGame()
    {
        // Set the game to paused
        Time.timeScale = 0f;
        isPaused = true;
        
        // Show the resume button and hide the pause button
        PauseButton.gameObject.SetActive(false);
        ResumeButton.gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        // Set the game to running
        Time.timeScale = 1f;
        isPaused = false;

        // Show the pause button and hide the resume button
        PauseButton.gameObject.SetActive(true);
        ResumeButton.gameObject.SetActive(false);
    }
}
