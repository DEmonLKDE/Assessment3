using UnityEngine;
using System.Collections;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;                
    public float cellSize = 1f;              
    public LayerMask wallMask;                 

    [Header("Audio")]
    public AudioSource audioSource;            
    public AudioClip sfxMove;                  
    public AudioClip sfxPellet;                 

    [Header("Animation")]
    public Animator animator;                   

    [Header("Effects")]
    public ParticleSystem dustEffect;           
    public static bool isTeleporting = false;

    [Header("Collision Effects")]
    public GameObject wallHitEffectPrefab;  
    public AudioClip sfxWallHit;            
    public float wallHitCooldown = 0.3f;    

    private float lastWallHitTime = -999f;   
    
    [Header("Control")]
    public bool canControl = true; 


    private bool isMoving = false;
    private Vector3 startPos, targetPos;
    private float t;
    private Vector2Int dir = Vector2Int.right;   
    private Vector2Int lastInput = Vector2Int.right; 
    private Vector2Int currentInput = Vector2Int.right;
    private Vector2Int lastBumpDir = Vector2Int.zero;


    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        isMoving = false;
        animator.SetBool("IsMoving", false);
        animator.SetInteger("direction", 4);

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

        Vector2Int next = CanWalk(center, lastInput) ? lastInput :
                          (CanWalk(center, currentInput) ? currentInput : Vector2Int.zero);

        if (next == Vector2Int.zero)
        {
            if (lastInput != Vector2Int.zero && lastInput != lastBumpDir)
            {
                PlayWallHitEffect(center, lastInput);
                lastBumpDir = lastInput;
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

            switch (gc.CurrentState)
            {
                case GhostController.GhostState.Normal:
                    GameManager.Instance.OnPlayerDeath();
                    break;

                case GhostController.GhostState.Scared:
                case GhostController.GhostState.Recovering:
                   
                    GameManager.Instance.OnGhostEaten(gc);
                    break;

                case GhostController.GhostState.Dead:
                    break;
            }
        }

    }
 
    void PlayWallHitEffect(Vector3 pos, Vector2Int direction)
    {
        if (Time.time - lastWallHitTime < wallHitCooldown)
            return;

        lastWallHitTime = Time.time; 

        if (wallHitEffectPrefab == null) return;

        Vector3 hitPos = pos + (Vector3)(Vector2)direction * 0.4f;
        GameObject fx = Instantiate(wallHitEffectPrefab, hitPos, Quaternion.identity);
        Destroy(fx, 1f);

        if (audioSource != null && sfxWallHit != null)
            audioSource.PlayOneShot(sfxWallHit);
    }
    public void ResetMovementImmediate()
    {
        StopAllCoroutines();                   
                                               
                                                 
        var animatorExists = animator != null;
        var field = GetType().GetField("isMoving", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(this, false);

        if (animatorExists) animator.SetBool("IsMoving", false);

        transform.position = new Vector3(
            Mathf.Round(transform.position.x / cellSize) * cellSize,
            Mathf.Round(transform.position.y / cellSize) * cellSize,
            transform.position.z
        );
    }
}





















