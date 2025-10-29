using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static GameManager instance; 

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
    private string currentLevelKey;
    private int lives;
    private float gameTimer = 0f;

    private float scaredTimer = 0f;
    private bool ghostsScared = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            instance = this; 
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentLevelKey = SceneManager.GetActiveScene().name == "InnovationScene" ? "Level2" : "Level1";

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
                    {
                        if (g.CurrentState == GhostController.GhostState.Scared ||
                            g.CurrentState == GhostController.GhostState.Recovering)
                        {
                            g.SetState(GhostController.GhostState.Normal);
                        }
                    }
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
                audioManager.PlayScaredBGM();
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


    public void OnPlayerDeath()
    {
        if (!isGameRunning) return; 
        isGameRunning = false;

        if (pacStudent != null)
            pacStudent.canControl = false;
        foreach (var g in ghosts)
            g.SetCanMove(false);

        lives--;
        hud.SetLives(lives);

        if (audioManager != null)
            audioManager.StopAllSFX();

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

    IEnumerator GameOver()
    {
        hud.ShowGameOver(true);
        isGameRunning = false;

        int prevHigh = PlayerPrefs.GetInt($"HighScore_{currentLevelKey}", 0);
        float prevTime = PlayerPrefs.GetFloat($"BestTime_{currentLevelKey}", 9999f);

        if (score > prevHigh || (score == prevHigh && gameTimer < prevTime))
        {
            PlayerPrefs.SetInt($"HighScore_{currentLevelKey}", score);
            PlayerPrefs.SetFloat($"BestTime_{currentLevelKey}", gameTimer);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.Log($"No new record saved. Keeping HighScore = {prevHigh}");
        }

        yield return new WaitForSeconds(3f);

        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }


    void AddScore(int value)
    {
        score += value;
        hud.SetScore(score);
    }

    public void ExitToStartScene()
    {
        isGameRunning = false;

        int currentScore = score;
        float currentTime = gameTimer;

        int highScore = PlayerPrefs.GetInt($"HighScore_{currentLevelKey}", 0);
        float bestTime = PlayerPrefs.GetFloat($"BestTime_{currentLevelKey}", 9999f);


        if (currentScore > highScore || (currentScore == highScore && currentTime < bestTime))
        {
            PlayerPrefs.SetInt($"HighScore_{currentLevelKey}", currentScore);
            PlayerPrefs.SetFloat($"BestTime_{currentLevelKey}", currentTime);
            PlayerPrefs.Save(); 
        }
        else
        {
            Debug.Log("No new record via Exit.");
        }

        StartCoroutine(ReturnToStartAfterDelay(3f));
    }
    private IEnumerator ReturnToStartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        PlayerPrefs.Save();

        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }

}







