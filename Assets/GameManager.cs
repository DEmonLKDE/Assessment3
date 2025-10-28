using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static GameManager instance; // 兼容旧调用

    [Header("References")]
    public PacStudentController pacStudent;
    public GhostController[] ghosts;
    public HUDController hud;
    public AudioManager audioManager;
    public CherryController cherryManager;

    [Header("Game Settings")]
    public int startLives = 3;
    public string levelName = "Level 1";

    [Header("Game State")]
    public bool isGameRunning = false;

    private int score = 0;
    private int lives;
    private float gameTimer = 0f;

    private float scaredTimer = 0f;
    private bool ghostsScared = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            instance = this; // 保留旧兼容
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初始化生命与 HUD（防止 lives = 0）
        lives = startLives;
        if (hud != null)
        {
            hud.InitLives(lives);
            hud.SetLives(lives);
            hud.SetScore(0);
            hud.SetLevelName(levelName);
            hud.ShowGameOver(false);
            hud.ShowGhostTimer(false);
        }

        // 启动倒计时
        StartCoroutine(RoundStartRoutine());
    }

    void Update()
    {
        if (isGameRunning)
        {
            gameTimer += Time.deltaTime;
            hud.SetTimer(gameTimer);

            if (ghostsScared)
            {
                scaredTimer -= Time.deltaTime;
                hud.SetGhostTimer(scaredTimer);

                if (scaredTimer <= 3f)
                {
                    foreach (var g in ghosts)
                        if (g.CurrentState == GhostController.GhostState.Scared)
                            g.SetState(GhostController.GhostState.Recovering);
                }

                if (scaredTimer <= 0f)
                {
                    ghostsScared = false;
                    hud.ShowGhostTimer(false);
                    foreach (var g in ghosts)
                        if (g.CurrentState != GhostController.GhostState.Dead)
                            g.SetState(GhostController.GhostState.Normal);
                    audioManager.PlayNormalBGM();
                }
            }
            
            if (GameObject.FindGameObjectsWithTag("Pellet").Length == 0 &&
        GameObject.FindGameObjectsWithTag("PowerPellet").Length == 0)
            {
                Debug.Log("All pellets eaten! Game Over triggered.");
                StartCoroutine(GameOver());
            }
        }
    }

    // ------------------------------------
    // 启动与开局流程
    // ------------------------------------
    private IEnumerator RoundStartRoutine()
    {
        isGameRunning = false;
        if (pacStudent) pacStudent.canControl = false;
        foreach (var g in ghosts) g.SetCanMove(false);

        if (audioManager) audioManager.PlayCountdownBGM();
        if (hud) yield return StartCoroutine(hud.Countdown321GO(1f));

        if (audioManager) audioManager.PlayNormalBGM();
        if (pacStudent) pacStudent.canControl = true;
        foreach (var g in ghosts) g.SetCanMove(true);

        isGameRunning = true;
    }

    void StartGame()
    {
        isGameRunning = true;
        if (pacStudent)
        {
            pacStudent.canControl = true;
            pacStudent.ResetMovementImmediate(); 
        }
        audioManager.PlayNormalBGM();
        foreach (var g in ghosts)
            g.SetCanMove(true);
    }


    // ------------------------------------
    // 加分事件
    // ------------------------------------
    public void OnPelletEaten() => AddScore(10);
    public void OnPowerPelletEaten()
    {
        AddScore(50);
        StartCoroutine(ScareGhosts());
    }
    public void OnCherryEaten() => AddScore(100);
    
    public void OnGhostEaten(GhostController g)
    {
        AddScore(300);
        audioManager.PlayDeadGhostBGM();
        g.BeEaten();
        StartCoroutine(RecoverGhostAfterDelay(g, 3f));
    }

    // ------------------------------------
    // 幽灵状态：惊吓、恢复
    // ------------------------------------
    IEnumerator ScareGhosts()
    {
        audioManager.PlayScaredBGM();
        ghostsScared = true;
        scaredTimer = 10f;
        hud.ShowGhostTimer(true);
        hud.SetGhostTimer(scaredTimer);

        foreach (var g in ghosts)
            g.SetState(GhostController.GhostState.Scared);

        yield return null;
    }

    IEnumerator RecoverGhostAfterDelay(GhostController g, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ghostsScared)
        {
            if (scaredTimer > 3f)
            {
                g.SetState(GhostController.GhostState.Scared);
                audioManager.PlayScaredBGM();
            }
            else if (scaredTimer > 0f)
            {
                g.SetState(GhostController.GhostState.Recovering);
                audioManager.PlayScaredBGM(); // Recovering 期间通常保持惊吓BGM
            }
            else
            {
                g.SetState(GhostController.GhostState.Normal);
                audioManager.PlayNormalBGM();
            }
        }
        else
        {
            g.SetState(GhostController.GhostState.Normal);
            audioManager.PlayNormalBGM();
        }
    }


    // ------------------------------------
    // 玩家死亡与复活
    // ------------------------------------
    public void OnPlayerDeath()
    {
        if (!isGameRunning) return;  // 防止重复触发
        isGameRunning = false;

        // 立即禁用玩家控制与幽灵移动
        if (pacStudent != null)
            pacStudent.canControl = false;
        foreach (var g in ghosts)
            g.SetCanMove(false);

        // 扣血并更新 UI
        lives--;
        hud.SetLives(lives);

        // 播放死亡特效 / 音效（如果 AudioManager 里有）
        if (audioManager != null)
            audioManager.StopAllSFX();

        // 启动死亡流程
        StartCoroutine(PlayerDeathSequence());
    }


    IEnumerator PlayerDeathSequence()
    {
        audioManager.StopAllSFX();
        yield return new WaitForSeconds(2f);

        if (lives > 0)
        {
            ResetPositions();
            yield return StartCoroutine(hud.Countdown321GO());
            StartGame();
        }
        else
        {
            StartCoroutine(GameOver());
        }
    }

    void ResetPositions()
    {
        pacStudent.transform.position = new Vector3(-23f, 17f, 0f);
        foreach (var g in ghosts)
            g.ResetToStartNormal();
    }

    // ------------------------------------
    // Game Over
    // ------------------------------------
    // ====================== GAME OVER ======================
    IEnumerator GameOver()
    {
        hud.ShowGameOver(true);
        isGameRunning = false;

        // 读取旧记录
        int prevHigh = PlayerPrefs.GetInt("HighScore", 0);
        float prevTime = PlayerPrefs.GetFloat("BestTime", 9999f);

        Debug.Log($"Current Score = {score}, Time = {gameTimer:F2}");
        Debug.Log($"Previous Record: High = {prevHigh}, Time = {prevTime:F2}");

        // 比较逻辑：高分优先；同分比时间
        if (score > prevHigh || (score == prevHigh && gameTimer < prevTime))
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.SetFloat("BestTime", gameTimer);
            PlayerPrefs.Save();
            Debug.Log($"New High Score Saved!  Score = {score}, Time = {gameTimer:F2}");
        }
        else
        {
            Debug.Log($"No new record saved. Keeping HighScore = {prevHigh}");
        }

        yield return new WaitForSeconds(3f);

        // 确保保存完成后再切场景
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }


    // ------------------------------------
    // 分数
    // ------------------------------------
    void AddScore(int value)
    {
        score += value;
        hud.SetScore(score);
    }

    // ------------------------------------
    // 主动退出（Exit 按钮）
    // ------------------------------------
    // ====================== EXIT BUTTON ======================
    public void ExitToStartScene()
    {
        isGameRunning = false;

        // 当前成绩
        int currentScore = score;
        float currentTime = gameTimer;

        // 旧记录
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", 9999f);

        Debug.Log($" Exit pressed. Current = {currentScore}, {currentTime:F2} | Old = {highScore}, {bestTime:F2}");

        // 判断是否刷新记录
        if (currentScore > highScore || (currentScore == highScore && currentTime < bestTime))
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.SetFloat("BestTime", currentTime);
            PlayerPrefs.Save(); //
            Debug.Log($"New High Score Saved via Exit!  Score = {currentScore}, Time = {currentTime:F2}");
        }
        else
        {
            Debug.Log("No new record via Exit.");
        }

        // 3 秒后回 StartScene
        StartCoroutine(ReturnToStartAfterDelay(3f));
    }
    private IEnumerator ReturnToStartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 确保保存完毕
        PlayerPrefs.Save();

        // 重新加载 StartScene
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }

}







