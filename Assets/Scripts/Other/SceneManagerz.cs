using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerz : MonoBehaviour
{
    public void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadLoseMenu()
    {
        SceneManager.LoadScene("LoseMenu");
    }

    public void LoadWinMenu()
    {
        SceneManager.LoadScene("WinMenu");
    }

    public void HowToPlayScene()
    {
        SceneManager.LoadScene("HowToPlay");
    }

    public void CreditsScene()
    {
        SceneManager.LoadScene("Credits");
    }
}
