using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    [Header("Main Texts")]
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text levelNameText;
    public TMP_Text ghostTimerText;

    [Header("Lives")]
    public Transform livesGroup;  
    public GameObject lifeIconPrefab;  

    [Header("Panels")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;
    public GameObject gameOverPanel;

    private List<GameObject> lifeIcons = new List<GameObject>();
    private int shownLives = -1;

    public void InitLives(int lives)
    {
        if (livesGroup == null || lifeIconPrefab == null)
        {
            Debug.LogWarning("HUDController: LivesGroup »ò LifeIconPrefab Î´°ó¶¨£¡");
            return;
        }

        foreach (var icon in lifeIcons)
            if (icon != null) Destroy(icon);
        lifeIcons.Clear();

        for (int i = 0; i < lives; i++)
        {
            GameObject icon = Instantiate(lifeIconPrefab, livesGroup);
            lifeIcons.Add(icon);
        }

        shownLives = lives;
    }

    public void SetLives(int lives)
    {
        if (lifeIcons.Count == 0)
            InitLives(lives);

        for (int i = 0; i < lifeIcons.Count; i++)
            lifeIcons[i].SetActive(i < lives);

        shownLives = lives;
    }

    public void SetScore(int value)
    {
        if (scoreText)
            scoreText.text = value.ToString("D6");
    }

    public void SetTimer(float time)
    {
        if (!timerText) return;
        int min = Mathf.FloorToInt(time / 60f);
        int sec = Mathf.FloorToInt(time % 60f);
        int centi = Mathf.FloorToInt((time - Mathf.Floor(time)) * 100f);
        timerText.text = $"{min:00}:{sec:00}:{centi:00}";
    }

    public void ShowGhostTimer(bool show)
    {
        if (ghostTimerText)
            ghostTimerText.gameObject.SetActive(show);
    }

    public void SetGhostTimer(float remaining)
    {
        if (ghostTimerText)
            ghostTimerText.text = Mathf.CeilToInt(remaining).ToString();
    }

    public void SetLevelName(string name)
    {
        if (levelNameText)
            levelNameText.text = name;
    }

    public IEnumerator Countdown321GO(float perSecond = 1f)
    {
        if (countdownPanel) countdownPanel.SetActive(true);

        string[] texts = { "3", "2", "1", "GO!" };
        foreach (string t in texts)
        {
            if (countdownText) countdownText.text = t;
            yield return new WaitForSeconds(perSecond);
        }

        if (countdownPanel) countdownPanel.SetActive(false);
    }

    public void ShowCountdownPanel(bool show)
    {
        if (countdownPanel)
            countdownPanel.SetActive(show);
    }

    public void ShowGameOver(bool show)
    {
        if (gameOverPanel)
            gameOverPanel.SetActive(show);
    }

    public void OnExitPressed()
    {
        if (GameManager.instance != null)
            GameManager.instance.ExitToStartScene();
    }
}




