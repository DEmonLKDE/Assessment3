using UnityEngine;
using System.Collections;

public class DashAbility : MonoBehaviour
{
    public float dashDuration = 2f;
    public float dashSpeedMultiplier = 2f;
    public float dashCooldown = 5f;

    private bool canDash = false;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;
    private PacStudentController pac;

    private AudioSource audioSource;
    public AudioClip dashSound;

    void Start()
    {
        pac = FindObjectOfType<PacStudentController>();
        audioSource = gameObject.AddComponent<AudioSource>();

    }


    void Update()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (canDash && !isDashing && dashCooldownTimer <= 0f && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(DoDash());
        }
    }

    IEnumerator DoDash()
    {
        isDashing = true;
        canDash = false;
        dashCooldownTimer = dashCooldown;

        float originalSpeed = pac.moveSpeed;
        pac.moveSpeed *= dashSpeedMultiplier;


        if (audioSource && dashSound)
            audioSource.PlayOneShot(dashSound);

        Collider2D playerCollider = pac.GetComponent<Collider2D>();
        int originalLayer = pac.gameObject.layer;
        pac.gameObject.layer = LayerMask.NameToLayer("IgnoreGhost");

        yield return new WaitForSeconds(dashDuration);

        pac.moveSpeed = originalSpeed;
        pac.gameObject.layer = originalLayer;
        isDashing = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DashOrb"))
        {
            canDash = true;
            Destroy(other.gameObject);
            Debug.Log("Dash ready!");
        }
    }
}


