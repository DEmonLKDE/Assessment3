using UnityEngine;

public class RandomFlashSpeed : MonoBehaviour
{
    void Start()
    {
        GetComponent<Animator>().speed = Random.Range(0.8f, 1.2f);
    }
}

