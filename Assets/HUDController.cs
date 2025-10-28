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
    public Transform livesGroup;        // LivesPanel
    public GameObject lifeIconPrefab;   // 可选，用来动态生成生命图标

    [Header("Panels")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;
    public GameObject gameOverPanel;

    // 内部变量
    private List<GameObject> lifeIcons = new List<GameObject>();
    private int shownLives = -1;

    // ===============================
    // 初始化生命图标
    // ===============================
    public void InitLives(int lives)
    {
        if (livesGroup == null || lifeIconPrefab == null)
        {
            Debug.LogWarning("HUDController: LivesGroup 或 LifeIconPrefab 未绑定！");
            return;
        }

        // 清除旧图标
        foreach (var icon in lifeIcons)
            if (icon != null) Destroy(icon);
        lifeIcons.Clear();

        // 生成新图标
        for (int i = 0; i < lives; i++)
        {
            GameObject icon = Instantiate(lifeIconPrefab, livesGroup);
            lifeIcons.Add(icon);
        }

        shownLives = lives;
    }

    // ===============================
    // 更新生命数量（扣血）
    // ===============================
    public void SetLives(int lives)
    {
        if (lifeIcons.Count == 0)
            InitLives(lives);

        for (int i = 0; i < lifeIcons.Count; i++)
            lifeIcons[i].SetActive(i < lives);

        shownLives = lives;
    }

    // ===============================
    // 分数显示
    // ===============================
    public void SetScore(int value)
    {
        if (scoreText)
            scoreText.text = value.ToString("D6"); // 显示六位数，如 000120
    }

    // ===============================
    // 游戏计时器
    // ===============================
    public void SetTimer(float time)
    {
        if (!timerText) return;
        int min = Mathf.FloorToInt(time / 60f);
        int sec = Mathf.FloorToInt(time % 60f);
        int centi = Mathf.FloorToInt((time - Mathf.Floor(time)) * 100f);
        timerText.text = $"{min:00}:{sec:00}:{centi:00}";
    }

    // ===============================
    // 幽灵计时器显示
    // ===============================
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

    // ===============================
    // 关卡名
    // ===============================
    public void SetLevelName(string name)
    {
        if (levelNameText)
            levelNameText.text = name;
    }

    // ===============================
    // 倒计时 3 2 1 GO!
    // ===============================
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

    // ===============================
    // GameOver 显示
    // ===============================
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




