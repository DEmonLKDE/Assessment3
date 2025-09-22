using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PacStudentMovement : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 2f;
    private int currentWaypoint = 0;

    private Animator animator;
    private AudioSource audioSource;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        transform.position = waypoints[0].position;

        StartCoroutine(MoveLoop());
    }

    IEnumerator MoveLoop()
    {
        while (true)
        {
            Transform target = waypoints[currentWaypoint];
            Vector3 startPos = transform.position;
            Vector3 endPos = target.position;

            Vector3 dir = (endPos - startPos).normalized;
            SetAnimation(dir);

            if (!audioSource.isPlaying)
                audioSource.Play();

            float distance = Vector3.Distance(startPos, endPos);
            float travelTime = distance / speed;

            float elapsed = 0f;
            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / travelTime;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            transform.position = endPos;

            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }

    void SetAnimation(Vector3 dir)
    {
        if (animator == null) return;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
                animator.Play("Pac_Walk_Right");
            else
                animator.Play("Pac_Walk_Left");
        }
        else
        {
            if (dir.y > 0)
                animator.Play("Pac_Walk_Up");
            else
                animator.Play("Pac_Walk_Down");
        }
    }
}