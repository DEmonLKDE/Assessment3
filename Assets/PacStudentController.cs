using UnityEngine;
using System.Collections;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;                 // 每秒移动速度
    public float cellSize = 1f;                  // 一格大小（手搭地图时一般是1）
    public LayerMask wallMask;                   // 检测墙体的Layer

    [Header("Audio")]
    public AudioSource audioSource;              // 拖入AudioSource
    public AudioClip sfxMove;                    // 移动音效
    public AudioClip sfxPellet;                  // 吃豆音效（可留空）

    [Header("Animation")]
    public Animator animator;                    // 拖入Animator（参数：IsMoving、direction）

    [Header("Effects")]
    public ParticleSystem dustEffect;            // 拖入Dust粒子特效
    public static bool isTeleporting = false;

    [Header("Collision Effects")]
    public GameObject wallHitEffectPrefab;   // 拖入 WallHitEffect 预制体
    public AudioClip sfxWallHit;             // 撞墙音效（可选）
    public float wallHitCooldown = 0.3f;     // 撞墙特效CD时间（秒）

    private float lastWallHitTime = -999f;   // 上一次触发时间（内部变量）
    
    [Header("Control")]
    public bool canControl = true; // 由 GameManager 控制


    private bool isMoving = false;
    private Vector3 startPos, targetPos;
    private float t;
    private Vector2Int dir = Vector2Int.right;   // 当前方向
    private Vector2Int lastInput = Vector2Int.right;   // 玩家最新输入
    private Vector2Int currentInput = Vector2Int.right; // 当前移动方向
    private Vector2Int lastBumpDir = Vector2Int.zero;


    void Start()
    {
        // 不重置位置，使用场景中手放的位置
        if (animator == null)
            animator = GetComponent<Animator>();

        isMoving = false;
        animator.SetBool("IsMoving", false);
        animator.SetInteger("direction", 4); // 初始朝右

        dir = Vector2Int.zero;
        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;
    }

    void Update()
    {
        if (!canControl)
            return;

        HandleInput();

        if (!isMoving)
            TryMove();
        else
            MoveLerp();
    }

    // --------------------------------------
    // 读取键盘输入
    // --------------------------------------
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) lastInput = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.S)) lastInput = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A)) lastInput = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.D)) lastInput = Vector2Int.right;
    }

    // --------------------------------------
    // 尝试移动（格子判断）
    // --------------------------------------
    void TryMove()
    {
        Vector3 center = RoundToCell(transform.position);

        // 优先 lastInput 方向，否则沿 currentInput 继续
        Vector2Int next = CanWalk(center, lastInput) ? lastInput :
                          (CanWalk(center, currentInput) ? currentInput : Vector2Int.zero);

        if (next == Vector2Int.zero)
        {
            if (lastInput != Vector2Int.zero && lastInput != lastBumpDir)
            {
                PlayWallHitEffect(center, lastInput);
                lastBumpDir = lastInput; // 记录上次撞击方向
            }
            return;
        }

        lastBumpDir = Vector2Int.zero;

        currentInput = next;
        dir = next;
        startPos = center;
        targetPos = center + (Vector3)(Vector2)dir * cellSize;
        t = 0;
        isMoving = true;

        animator.SetBool("IsMoving", true);
        SetDirection(dir);
        PlayMoveSound();
        if (dustEffect != null) dustEffect.Play();
    }



    // --------------------------------------
    // 移动过程 (Lerp)
    // --------------------------------------
    void MoveLerp()
    {
        t += Time.deltaTime * moveSpeed / cellSize;
        transform.position = Vector3.Lerp(startPos, targetPos, t);

        if (t >= 1f)
        {
            transform.position = targetPos;
            isMoving = false;
            StartCoroutine(SmoothStopAnimation());
        }
    }

    // --------------------------------------
    // 延迟平滑停止动画（防止闪烁）
    // --------------------------------------
    IEnumerator SmoothStopAnimation()
    {
        yield return new WaitForSeconds(0.05f);
        if (!isMoving)
        {
            animator.SetBool("IsMoving", false);
            StopMoveSound();
            if (dustEffect != null) dustEffect.Stop();
        }
    }

    // --------------------------------------
    // 判断是否能走（碰撞检测）
    // --------------------------------------
    bool CanWalk(Vector3 fromCenter, Vector2Int d)
    {
        if (d == Vector2Int.zero) return false;
        Vector3 toCenter = fromCenter + (Vector3)(Vector2)d * cellSize;

        float box = cellSize * 0.6f; // 检测范围稍小于格子
        return !Physics2D.OverlapBox(toCenter, new Vector2(box, box), 0f, wallMask);
    }

    // --------------------------------------
    // 对齐到格子中心（取整）
    // --------------------------------------
    Vector3 RoundToCell(Vector3 p)
    {
        float rx = Mathf.Round(p.x / cellSize) * cellSize;
        float ry = Mathf.Round(p.y / cellSize) * cellSize;
        return new Vector3(rx, ry, p.z);
    }

    // --------------------------------------
    // 设置动画方向
    // --------------------------------------
    void SetDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.up) animator.SetInteger("direction", 1);
        else if (dir == Vector2Int.down) animator.SetInteger("direction", 2);
        else if (dir == Vector2Int.left) animator.SetInteger("direction", 3);
        else if (dir == Vector2Int.right) animator.SetInteger("direction", 4);
    }

    // --------------------------------------
    // 播放移动音效
    // --------------------------------------
    void PlayMoveSound()
    {
        if (audioSource != null && sfxMove != null)
        {
            audioSource.clip = sfxMove;
            audioSource.loop = true;
            if (!audioSource.isPlaying) audioSource.Play();
        }
    }

    // --------------------------------------
    // 停止移动音效
    // --------------------------------------
    void StopMoveSound()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    // --------------------------------------
    // 碰撞检测（吃豆、吃樱桃）
    // --------------------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pellet"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.OnPelletEaten();
            if (audioSource != null && sfxPellet != null) audioSource.PlayOneShot(sfxPellet);
            GameManager.Instance.OnPelletEaten();
        }
        else if (other.CompareTag("PowerPellet"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.OnPowerPelletEaten();
            if (audioSource != null && sfxPellet != null) audioSource.PlayOneShot(sfxPellet);
            GameManager.Instance.OnPowerPelletEaten();
        }
        else if (other.CompareTag("Cherry"))
        {
            Destroy(other.gameObject);
            if (audioSource != null && sfxPellet != null) audioSource.PlayOneShot(sfxPellet);
            GameManager.Instance.OnCherryEaten();
        }
        else if (other.CompareTag("Ghost"))
        {
            var gc = other.GetComponent<GhostController>();
            if (gc == null) return;

            // 根据幽灵当前状态决定效果
            switch (gc.CurrentState)
            {
                case GhostController.GhostState.Normal:
                    // 正常状态：PacStudent 死亡流程（你已有）
                    GameManager.Instance.OnPlayerDeath();
                    break;

                case GhostController.GhostState.Scared:
                case GhostController.GhostState.Recovering:
                    // Scared/Recovering：吃掉幽灵
                    GameManager.Instance.OnGhostEaten(gc);
                    break;

                case GhostController.GhostState.Dead:
                    // Dead 状态：忽略
                    break;
            }
        }

    }
    // 播放撞墙粒子特效
    void PlayWallHitEffect(Vector3 pos, Vector2Int direction)
    {
        // 检查冷却时间，避免连续触发
        if (Time.time - lastWallHitTime < wallHitCooldown)
            return;

        lastWallHitTime = Time.time; // 记录这次触发时间

        if (wallHitEffectPrefab == null) return;

        Vector3 hitPos = pos + (Vector3)(Vector2)direction * 0.4f;
        GameObject fx = Instantiate(wallHitEffectPrefab, hitPos, Quaternion.identity);
        Destroy(fx, 1f);

        // 播放音效（可选）
        if (audioSource != null && sfxWallHit != null)
            audioSource.PlayOneShot(sfxWallHit);
    }
    // 放在 PacStudentController 类里任意位置（方法区）
    public void ResetMovementImmediate()
    {
        StopAllCoroutines();                     // 终止任何未完成的 lerp
                                                 // 如果你的脚本里 isMoving 是 private，这里也在本类内可以访问
                                                 // 如果变量名是别的（比如 moving），用你自己的名字
        var animatorExists = animator != null;
        // 把 isMoving 设回 false
        var field = GetType().GetField("isMoving", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(this, false);

        // 动画复位为非移动（避免停在“走路”）
        if (animatorExists) animator.SetBool("IsMoving", false);

        // 将角色吸附回格中心，避免卡在半格（如果你已有 RoundToCell 就调用它）
        transform.position = new Vector3(
            Mathf.Round(transform.position.x / cellSize) * cellSize,
            Mathf.Round(transform.position.y / cellSize) * cellSize,
            transform.position.z
        );
    }



}





















