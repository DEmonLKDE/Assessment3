using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    public GameObject outsideCornerPrefab;
    public GameObject outsideWallPrefab;
    public GameObject insideCornerPrefab;
    public GameObject insideWallPrefab;
    public GameObject pelletPrefab;
    public GameObject powerPelletPrefab;
    public GameObject tjunctionPrefab;
    public GameObject ghostGatePrefab;
    public GameObject emptyPrefab;
    public float overlap = 0.48f;

    int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7,0},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4,0},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4,0},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4,0},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3,0},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5,0},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4,0},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3,0},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4,0},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4,0},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3,0},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8,0},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
    };

    void Start()
    {
        GenerateTopLeft();
        MirrorOtherQuadrants();
        FitCameraToFourQuadrants();
    }

    void GenerateTopLeft()
    {
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);
        var tlParent = new GameObject("Quadrant_TL").transform;
        tlParent.SetParent(transform, false);
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int tile = levelMap[y, x];
                if (tile == 0) continue;
                Vector3 pos = new Vector3(-(cols - x - 1 - overlap), -(rows - y - 1 - overlap), 0f);
                GameObject prefab = GetPrefab(tile);
                if (prefab == null) continue;
                GameObject go = Instantiate(prefab, pos, Quaternion.identity, tlParent);
                Quaternion baseRot = GetRotation(x, y, tile);
                go.transform.rotation = baseRot * Quaternion.Euler(0, 0, 180);
            }
        }
    }

    void MirrorOtherQuadrants()
    {
        Transform tl = transform.Find("Quadrant_TL");
        if (tl == null) return;
        var tr = new GameObject("Quadrant_TR").transform;
        var bl = new GameObject("Quadrant_BL").transform;
        var br = new GameObject("Quadrant_BR").transform;
        tr.SetParent(transform, false);
        bl.SetParent(transform, false);
        br.SetParent(transform, false);
        var toClone = new List<Transform>();
        foreach (Transform child in tl) toClone.Add(child);
        foreach (var child in toClone)
        {
            Vector3 p = child.position;
            Quaternion r = child.rotation;
            Vector3 s = child.localScale;
            var goTR = Instantiate(child.gameObject, new Vector3(-p.x, p.y, p.z), r, tr);
            goTR.transform.localScale = s;
            var goBL = Instantiate(child.gameObject, new Vector3(p.x, -p.y, p.z), r, bl);
            goBL.transform.localScale = s;
            var goBR = Instantiate(child.gameObject, new Vector3(-p.x, -p.y, p.z), r, br);
            goBR.transform.localScale = s;
        }
    }

    GameObject GetPrefab(int tile)
    {
        switch (tile)
        {
            case 0: return emptyPrefab;
            case 1: return outsideCornerPrefab;
            case 2: return outsideWallPrefab;
            case 3: return insideCornerPrefab;
            case 4: return insideWallPrefab;
            case 5: return pelletPrefab;
            case 6: return powerPelletPrefab;
            case 7: return tjunctionPrefab;
            case 8: return ghostGatePrefab;
            default: return null;
        }
    }

    Quaternion GetRotation(int x, int y, int tile)
    {
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);
        int up = (y > 0) ? levelMap[y - 1, x] : -1;
        int down = (y < rows - 1) ? levelMap[y + 1, x] : -1;
        int left = (x > 0) ? levelMap[y, x - 1] : -1;
        int right = (x < cols - 1) ? levelMap[y, x + 1] : -1;
        bool upWall = IsWall(up);
        bool downWall = IsWall(down);
        bool leftWall = IsWall(left);
        bool rightWall = IsWall(right);
        switch (tile)
        {
            case 1:
                if (downWall && rightWall) return Quaternion.Euler(0, 0, 180);
                if (downWall && leftWall) return Quaternion.Euler(0, 0, 270);
                if (upWall && leftWall) return Quaternion.Euler(0, 0, 0);
                if (upWall && rightWall) return Quaternion.Euler(0, 0, 90);
                break;
            case 2:
                if ((leftWall && rightWall) || (!upWall && !downWall))
                    return Quaternion.Euler(0, 0, 90);
                else
                    return Quaternion.Euler(0, 0, 0);
            case 3:
                if (upWall && rightWall) return Quaternion.Euler(0, 0, 270);
                if (upWall && leftWall) return Quaternion.Euler(0, 0, 0);
                if (downWall && leftWall) return Quaternion.Euler(0, 0, 90);
                if (downWall && rightWall) return Quaternion.Euler(0, 0, 180);
                break;
            case 4:
                if ((leftWall && rightWall) || (!upWall && !downWall))
                    return Quaternion.Euler(0, 0, 90);
                else
                    return Quaternion.Euler(0, 0, 0);
            case 7:
                if (!upWall) return Quaternion.Euler(0, 0, 0);
                if (!rightWall) return Quaternion.Euler(0, 0, 90);
                if (!downWall) return Quaternion.Euler(0, 0, 180);
                if (!leftWall) return Quaternion.Euler(0, 0, 270);
                break;
        }
        return Quaternion.identity;
    }

    bool IsWall(int tile)
    {
        return tile == 1 || tile == 2 || tile == 3 || tile == 4 || tile == 7 || tile == 8;
    }

    void FitCameraToFourQuadrants()
    {
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);
        var cam = Camera.main;
        if (cam == null) return;
        cam.orthographic = true;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        float needH = rows;
        float needW = cols / cam.aspect;
        cam.orthographicSize = Mathf.Ceil(Mathf.Max(needH, needW));
    }
}







