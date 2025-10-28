using UnityEngine;

public class TeleportByPosition : MonoBehaviour
{
    [Header("PacStudent ¶ÔÏó")]
    public Transform pacStudent;   // ÍÏÈë PacStudent

    [Header("´«ËÍÉèÖÃ")]
    public Vector3 leftGatePos = new Vector3(-24f, 4f, 0f);
    public Vector3 rightGatePos = new Vector3(3f, 4f, 0f);
    public Vector3 leftGatePos2 = new Vector3(-24f, 3f, 0f);
    public Vector3 rightGatePos2 = new Vector3(3f, 3f, 0f);
    public float triggerRange = 0.5f;  // ´¥·¢·¶Î§£¨ÈÝ²î£©

    [Header("¾µÏñ´«ËÍ£¨¿ÉÑ¡£©")]
    public bool bidirectional = true;  // ÊÇ·ñË«Ïò´«ËÍ

    void Update()
    {
        if (pacStudent == null) return;

        Vector3 pos = pacStudent.position;

        // --- ×ó -> ÓÒ ---
        if (Vector3.Distance(pos, leftGatePos) < triggerRange)
        {
            pacStudent.position = rightGatePos;
            Debug.Log($"Teleported from Left ({leftGatePos}) ¡ú Right ({rightGatePos})");
            return;
        }

        // --- ÓÒ -> ×ó ---
        if (bidirectional && Vector3.Distance(pos, rightGatePos) < triggerRange)
        {
            pacStudent.position = leftGatePos;
            Debug.Log($"Teleported from Right ({rightGatePos}) ¡ú Left ({leftGatePos})");
            return;
        }
        
        if (pacStudent == null) return;

        Vector3 pos2 = pacStudent.position;

        // --- ×ó -> ÓÒ ---
        if (Vector3.Distance(pos2, leftGatePos2) < triggerRange)
        {
            pacStudent.position = rightGatePos2;
            Debug.Log($"Teleported from Left ({leftGatePos2}) ¡ú Right ({rightGatePos2})");
            return;
        }

        // --- ÓÒ -> ×ó ---
        if (bidirectional && Vector3.Distance(pos2, rightGatePos2) < triggerRange)
        {
            pacStudent.position = leftGatePos2;
            Debug.Log($"Teleported from Right ({rightGatePos2}) ¡ú Left ({leftGatePos2})");
            return;
        }
    }
}

  


