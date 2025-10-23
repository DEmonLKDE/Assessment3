using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void LoadLevel1()
    {
        SceneManager.LoadScene("ManualLevel_Completed");
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("DesignIterationScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

