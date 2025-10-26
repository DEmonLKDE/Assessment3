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

    private bool isMoving = false;
    private Vector3 startPos, targetPos;
    private float t;
    private Vector2Int dir = Vector2Int.right;   // 当前方向
    private Vector2Int lastInput = Vector2Int.right;   // 玩家最新输入
    private Vector2Int currentInput = Vector2Int.right; // 当前移动方向

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

        if (next == Vector2Int.zero) return;

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
            if (audioSource != null && sfxPellet != null)
                audioSource.PlayOneShot(sfxPellet);
        }
        else if (other.CompareTag("PowerPellet"))
        {
            Destroy(other.gameObject);
            if (audioSource != null && sfxPellet != null)
                audioSource.PlayOneShot(sfxPellet);
        }
        else if (other.CompareTag("Cherry"))
        {
            Destroy(other.gameObject);
            // 播放吃樱桃音效（若有）
            if (audioSource != null && sfxPellet != null)
                audioSource.PlayOneShot(sfxPellet);
            // 调用 HUD 或 GameManager 增加分数
            Debug.Log("Cherry eaten! +500 points");
        }

    }
}


















