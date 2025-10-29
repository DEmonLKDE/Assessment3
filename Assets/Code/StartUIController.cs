using UnityEngine;
using TMPro;

public class StartUIController : MonoBehaviour
{
    [Header("Level 1 UI")]
    public TMP_Text highScore1Text;
    public TMP_Text bestTime1Text;

    [Header("Level 2 UI")]
    public TMP_Text highScore2Text;
    public TMP_Text bestTime2Text;

    void Start()
    {
        int high1 = PlayerPrefs.GetInt("HighScore_Level1", 0);
        float time1 = PlayerPrefs.GetFloat("BestTime_Level1", 0f);

        int high2 = PlayerPrefs.GetInt("HighScore_Level2", 0);
        float time2 = PlayerPrefs.GetFloat("BestTime_Level2", 0f);

        string score1 = high1.ToString("D6");
        string score2 = high2.ToString("D6");

        string time1Str = FormatTime(time1);
        string time2Str = FormatTime(time2);

        highScore1Text.text = $"Level 1 High Score: {score1}";
        bestTime1Text.text = $"Level 1 Best Time: {time1Str}";

        highScore2Text.text = $"Level 2 High Score: {score2}";
        bestTime2Text.text = $"Level 2 Best Time: {time2Str}";
    }

    private string FormatTime(float time)
    {
        int min = Mathf.FloorToInt(time / 60f);
        int sec = Mathf.FloorToInt(time % 60f);
        int centi = Mathf.FloorToInt((time - Mathf.Floor(time)) * 100f);
        return $"{min:00}:{sec:00}:{centi:00}";
    }
}

