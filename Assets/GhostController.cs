using UnityEngine;

public class GhostController : MonoBehaviour
{
    public enum GhostState { Normal = 0, Scared = 1, Recovering = 2, Dead = 3 }

    public GhostState CurrentState { get; private set; } = GhostState.Normal;

    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    public Vector3 startPosition;
    private bool canMove = false;
    private float moveSpeed = 2.0f; // 可调整速度（Normal 时 2.0f，Scared 时可减速）

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        startPosition = transform.position;
        SetState(GhostState.Normal);
    }

    // -------------------------
    // 状态切换：同步 Animator 参数
    // -------------------------
    public void SetState(GhostState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;

        if (animator != null)
            animator.SetInteger("State", (int)CurrentState);

        // 根据状态调整属性（速度、颜色、行为等）
        switch (CurrentState)
        {
            case GhostState.Normal:
                moveSpeed = 2.0f;
                break;

            case GhostState.Scared:
                moveSpeed = 1.2f; // 慢一些
                break;

            case GhostState.Recovering:
                moveSpeed = 1.4f;
                break;

            case GhostState.Dead:
                moveSpeed = 3.5f; // 可选择更快（回巢）
                break;
        }
    }

    // -------------------------
    // 控制能否移动
    // -------------------------
    public void SetCanMove(bool move)
    {
        canMove = move;
    }

    // -------------------------
    // Reset 到起点（用于复活或新回合）
    // -------------------------
    public void ResetToStartNormal()
    {
        transform.position = startPosition;
        SetState(GhostState.Normal);
    }

    // -------------------------
    // 被吃掉时的逻辑
    // -------------------------
    public void BeEaten()
    {
        SetState(GhostState.Dead);
        // 立即移动到地图中心（或任意指定点）
        transform.position = new Vector3(-10.5f, 3.5f, 0f);
    }

    // -------------------------
    // 碰撞检测
    // -------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        switch (CurrentState)
        {
            case GhostState.Normal:
                GameManager.Instance.OnPlayerDeath();
                break;

            case GhostState.Scared:
            case GhostState.Recovering:
                GameManager.Instance.OnGhostEaten(this);
                break;

            case GhostState.Dead:
                // 忽略碰撞
                break;
        }
    }

    // -------------------------
    // （可选）移动逻辑：由 canMove 控制
    // -------------------------
    void Update()
    {
        if (!canMove) return;

        // 示例：向当前方向平移（具体可接入你的导航逻辑）
        // transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }
}


