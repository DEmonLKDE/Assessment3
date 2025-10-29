using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private string targetScene = "MainScene";

    public void LoadLevel()
    {
        SceneManager.LoadScene(targetScene);
    }
}
