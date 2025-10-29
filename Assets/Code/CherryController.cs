using UnityEngine;
using System.Collections;

public class CherryController : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject cherryPrefab;   
    public float moveDuration = 8f;   
    public float spawnDelay = 5f;        
    public float outMargin = 2f;       

    [Header("Map Settings")]
    public Vector2 center = new Vector2(-10.5f, 3.5f); 
    public Vector2 mapSize = new Vector2(30f, 30f); 

    private GameObject currentCherry;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(spawnDelay);
        while (true)
        {
            SpawnCherry();
            yield return new WaitUntil(() => currentCherry == null);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnCherry()
    {
        if (cherryPrefab == null || currentCherry != null)
            return;

        int side = Random.Range(0, 4);
        Vector3 start = Vector3.zero;
        switch (side)
        {
            case 0: 
                start = new Vector3(center.x - mapSize.x / 2f - outMargin, Random.Range(center.y - mapSize.y / 2f, center.y + mapSize.y / 2f), 0);
                break;
            case 1: 
                start = new Vector3(center.x + mapSize.x / 2f + outMargin, Random.Range(center.y - mapSize.y / 2f, center.y + mapSize.y / 2f), 0);
                break;
            case 2: 
                start = new Vector3(Random.Range(center.x - mapSize.x / 2f, center.x + mapSize.x / 2f), center.y - mapSize.y / 2f - outMargin, 0);
                break;
            case 3: 
                start = new Vector3(Random.Range(center.x - mapSize.x / 2f, center.x + mapSize.x / 2f), center.y + mapSize.y / 2f + outMargin, 0);
                break;
        }

        // 让它穿过中心点
        Vector3 end = (Vector3)(2f * center) - start;

        currentCherry = Instantiate(cherryPrefab, start, Quaternion.identity);
        var sr = currentCherry.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 100;

        StartCoroutine(MoveCherry(currentCherry, start, end));
    }

    IEnumerator MoveCherry(GameObject cherry, Vector3 start, Vector3 end)
    {
        float t = 0;
        while (cherry != null && t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, moveDuration);
            cherry.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        if (cherry != null)
            Destroy(cherry);
        currentCherry = null;
    }
}

