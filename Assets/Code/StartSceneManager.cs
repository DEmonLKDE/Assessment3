using UnityEngine;
using TMPro;

public class StartSceneManager : MonoBehaviour
{
    public TMP_Text highScoreText;
    public TMP_Text bestTimeText;

    void Start()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0);

        highScoreText.text = $"High Score: {highScore:D6}";

        if (bestTime > 0)
        {
            int min = Mathf.FloorToInt(bestTime / 60f);
            int sec = Mathf.FloorToInt(bestTime % 60f);
            int centi = Mathf.FloorToInt((bestTime - Mathf.Floor(bestTime)) * 100f);
            bestTimeText.text = $"Best Time: {min:00}:{sec:00}:{centi:00}";
        }
        else
        {
            bestTimeText.text = "Best Time: --:--:--";
        }
    }
}

