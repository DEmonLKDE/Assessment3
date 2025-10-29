using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public void ExitToStart()
    {
        SceneManager.LoadScene("StartScene");
    }
}
