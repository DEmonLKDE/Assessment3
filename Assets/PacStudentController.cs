using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public float moveSpeed = 3f;                 // 每秒移动速度
    public float cellSize = 1f;                  // 网格间距
    public LayerMask wallMask;                   // 检测墙的层
    public AudioSource audioSource;              // 播放音效
    public AudioClip sfxPellet;                  // 吃豆声音
    public AudioClip sfxMove;                    // 移动声音
    public Animator animator;                    // 控制动画

    private bool IsMoving = false;
    private Vector3 startPos, targetPos;
    private float t;
    private Vector2Int dir = Vector2Int.right;
    private Vector2Int lastInput = Vector2Int.right;
    private Vector2Int currentInput = Vector2Int.right;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        IsMoving = false;
        animator.SetBool("IsMoving", false);
        animator.SetInteger("direction", 4); // 初始朝右
    }

    void Update()
    {
        HandleInput();

        if (!IsMoving)
            TryMove();
        else
            MoveLerp();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) lastInput = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.S)) lastInput = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A)) lastInput = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.D)) lastInput = Vector2Int.right;
    }

    void TryMove()
    {
        Vector3 center = RoundToCell(transform.position);

        // 优先尝试上次输入方向
        Vector2Int next = CanWalk(center, lastInput) ? lastInput :
                          (CanWalk(center, currentInput) ? currentInput : Vector2Int.zero);

        if (next == Vector2Int.zero) return;

        currentInput = next;
        dir = next;
        startPos = center;
        targetPos = center + (Vector3)(Vector2)dir * cellSize;
        t = 0;
        IsMoving = true;

        animator.SetBool("IsMoving", true);
        SetDirection(dir);
        PlayMoveSound();
    }

    void MoveLerp()
    {
        t += Time.deltaTime * moveSpeed / cellSize;
        transform.position = Vector3.Lerp(startPos, targetPos, t);

        if (t >= 1f)
        {
            transform.position = targetPos;
            IsMoving = false;
            animator.SetBool("IsMoving", false);
        }
    }

    bool CanWalk(Vector3 fromCenter, Vector2Int d)
    {
        if (d == Vector2Int.zero) return false;

        Vector3 toCenter = fromCenter + (Vector3)(Vector2)d * cellSize;
        float box = cellSize * 0.6f;
        return !Physics2D.OverlapBox(toCenter, new Vector2(box, box), 0f, wallMask);
    }

    Vector3 RoundToCell(Vector3 p)
    {
        float rx = Mathf.Round(p.x / cellSize) * cellSize;
        float ry = Mathf.Round(p.y / cellSize) * cellSize;
        return new Vector3(rx, ry, p.z);
    }

    void SetDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.up) animator.SetInteger("direction", 1);
        else if (dir == Vector2Int.down) animator.SetInteger("direction", 2);
        else if (dir == Vector2Int.left) animator.SetInteger("direction", 3);
        else if (dir == Vector2Int.right) animator.SetInteger("direction", 4);
    }

    void PlayMoveSound()
    {
        if (audioSource != null && sfxMove != null)
        {
            audioSource.clip = sfxMove;
            audioSource.loop = true;
            if (!audioSource.isPlaying) audioSource.Play();
        }
    }

    void StopMoveSound()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

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
            // TODO: 触发幽灵进入“害怕”状态
        }
        else if (other.CompareTag("Cherry"))
        {
            Destroy(other.gameObject);
            // TODO: 播放吃樱桃音效（如果有）
        }
    }
}

















