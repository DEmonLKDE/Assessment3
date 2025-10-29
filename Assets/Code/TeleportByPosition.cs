using UnityEngine;

public class TeleportByPosition : MonoBehaviour
{
    [Header("PacStudent")]
    public Transform pacStudent;  

    [Header("Setting")]
    public Vector3 leftGatePos = new Vector3(-24f, 4f, 0f);
    public Vector3 rightGatePos = new Vector3(3f, 4f, 0f);
    public Vector3 leftGatePos2 = new Vector3(-24f, 3f, 0f);
    public Vector3 rightGatePos2 = new Vector3(3f, 3f, 0f);
    public float triggerRange = 0.5f;  

    [Header("TP")]
    public bool bidirectional = true; 

    void Update()
    {
        if (pacStudent == null) return;

        Vector3 pos = pacStudent.position;

        if (Vector3.Distance(pos, leftGatePos) < triggerRange)
        {
            pacStudent.position = rightGatePos;
            return;
        }

        if (bidirectional && Vector3.Distance(pos, rightGatePos) < triggerRange)
        {
            pacStudent.position = leftGatePos;
            return;
        }
        
        if (pacStudent == null) return;

        Vector3 pos2 = pacStudent.position;

        if (Vector3.Distance(pos2, leftGatePos2) < triggerRange)
        {
            pacStudent.position = rightGatePos2;
            return;
        }

        if (bidirectional && Vector3.Distance(pos2, rightGatePos2) < triggerRange)
        {
            pacStudent.position = leftGatePos2;
            return;
        }
    }
}

  


