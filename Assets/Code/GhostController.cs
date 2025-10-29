using UnityEngine;
using System.Collections.Generic;

public class GhostController : MonoBehaviour
{
    public enum GhostState { Normal = 0, Scared = 1, Recovering = 2, Dead = 3 }

    public GhostState CurrentState { get; private set; } = GhostState.Normal;

    [Header("References")]
    [SerializeField] private Animator animator;
    public Transform pacStudent;
    public LayerMask wallMask;
    public Transform spawnPoint;

    [Header("Movement")]
    public Vector3 startPosition;
    private bool canMove = false;
    private float moveSpeed = 2.0f;
    public int ghostID = 1;        
    public float cellSize = 1f;

    private Vector2Int currentDir = Vector2Int.right;
    private Vector2Int lastDir = Vector2Int.left;
    private bool isMoving = false;
    private Vector3 startPos, targetPos;
    private float t = 0f;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (pacStudent == null && GameManager.Instance != null)
            pacStudent = GameManager.Instance.pacStudent.transform;

        if (spawnPoint == null)
            spawnPoint = GameObject.Find("GhostSpawnArea")?.transform;

        startPosition = transform.position;
        SetState(GhostState.Normal);
    }

    void Update()
    {
        if (!canMove) return;
        if (CurrentState == GhostState.Dead)
        {
            transform.position = Vector3.MoveTowards(transform.position, spawnPoint.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, spawnPoint.position) < 0.2f)
            {
                transform.position = spawnPoint.position;
                canMove = false; 
                                  
                if (animator != null) animator.SetInteger("State", (int)GhostState.Dead);
            }
            return;
        }

        if (!isMoving)
            DecideNextMove();
        else
            MoveLerp();
    }

    public void SetState(GhostState newState)
    {
        if (CurrentState == GhostState.Dead && newState != GhostState.Dead)
            return;

        if (CurrentState == newState) return;
        CurrentState = newState;

        if (animator != null)
            animator.SetInteger("State", (int)CurrentState);

        switch (CurrentState)
        {
            case GhostState.Normal:
                moveSpeed = 2.0f;
                break;
            case GhostState.Scared:
                moveSpeed = 1.0f;
                break;
            case GhostState.Recovering:
                moveSpeed = 1.2f;
                break;
            case GhostState.Dead:
                moveSpeed = 3.0f;
                break;
        }
    }

    private void RespawnFromSpawn()
    {
        SetState(GhostState.Normal);
        canMove = true;

        transform.position = spawnPoint.position + Vector3.up * 0.5f;
    }

    void DecideNextMove()
    {
        Vector3 center = RoundToCell(transform.position);
        List<Vector2Int> validDirs = GetValidDirections(center);

        if (validDirs.Count == 0)
        {
            MoveTo(-currentDir);
            return;
        }

        Vector2Int chosenDir = currentDir;

        if (CurrentState == GhostState.Scared || CurrentState == GhostState.Recovering)
        {
            chosenDir = ChooseFurthestDirection(validDirs);
        }
        else
        {
            switch (ghostID)
            {
                case 1:
                    chosenDir = ChooseFurthestDirection(validDirs);
                    break;
                case 2:
                    chosenDir = ChooseClosestDirection(validDirs);
                    break;
                case 3:
                    chosenDir = validDirs[Random.Range(0, validDirs.Count)];
                    break;
                case 4:
                    chosenDir = ChooseClockwiseDirection(validDirs);
                    break;
            }
        }

        MoveTo(chosenDir);
    }

    void MoveLerp()
    {
        t += Time.deltaTime * moveSpeed / cellSize;
        transform.position = Vector3.Lerp(startPos, targetPos, t);
        if (t >= 1f)
        {
            transform.position = targetPos;
            t = 0f;
            isMoving = false;

            lastDir = currentDir;
        }
    }


    List<Vector2Int> GetValidDirections(Vector3 center)
    {
        List<Vector2Int> dirs = new List<Vector2Int>();
        Vector2Int[] all = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var d in all)
        {
            if (d == -lastDir) continue;
            if (!Physics2D.OverlapBox(center + (Vector3)(Vector2)d * cellSize, new Vector2(0.6f, 0.6f), 0, wallMask))
                dirs.Add(d);
        }

        if (dirs.Contains(currentDir))
        {
            if (dirs.Count > 1)
                dirs.Remove(currentDir);
            dirs.Insert(0, currentDir);
        }

        if (dirs.Count == 0)
            dirs.Add(-lastDir);

        return dirs;
    }


    Vector2Int ChooseClosestDirection(List<Vector2Int> dirs)
    {
        float bestDist = Mathf.Infinity;
        Vector2Int bestDir = dirs[0];
        foreach (var d in dirs)
        {
            float noise = Random.Range(-0.05f, 0.05f);
            float dist = Vector2.Distance(pacStudent.position, transform.position + (Vector3)(Vector2)d);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestDir = d;
            }
        }
        return bestDir;
    }

    Vector2Int ChooseFurthestDirection(List<Vector2Int> dirs)
    {
        float bestDist = 0f;
        Vector2Int bestDir = dirs[0];
        foreach (var d in dirs)
        {
            float noise = Random.Range(-0.05f, 0.05f);
            float dist = Vector2.Distance(pacStudent.position, transform.position + (Vector3)(Vector2)d);
            if (dist > bestDist)
            {
                bestDist = dist;
                bestDir = d;
            }
        }
        return bestDir;
    }

    Vector2Int ChooseClockwiseDirection(List<Vector2Int> dirs)
    {
        Vector2Int[] clockwise = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        int idx = System.Array.IndexOf(clockwise, currentDir);
        for (int i = 0; i < 4; i++)
        {
            Vector2Int tryDir = clockwise[(idx + i) % 4];
            if (dirs.Contains(tryDir)) return tryDir;
        }
        return dirs[0];
    }

    void MoveTo(Vector2Int dir)
    {
        startPos = RoundToCell(transform.position);
        targetPos = startPos + (Vector3)(Vector2)dir * cellSize;
        t = 0;
        isMoving = true;

        currentDir = dir;
    }


    Vector3 RoundToCell(Vector3 p)
    {
        float rx = Mathf.Round(p.x / cellSize) * cellSize;
        float ry = Mathf.Round(p.y / cellSize) * cellSize;
        return new Vector3(rx, ry, p.z);
    }

    public void SetCanMove(bool move)
    {
        canMove = move;
    }

    public void ResetToStartNormal()
    {
        transform.position = startPosition;
        SetState(GhostState.Normal);
    }

    public void BeEaten()
    {
        SetState(GhostState.Dead);
    }

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
        }
    }
}



