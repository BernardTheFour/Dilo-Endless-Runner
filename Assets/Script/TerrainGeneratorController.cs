using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorController : MonoBehaviour
{
    [Header("Templates")]
    public List<TerrainTemplateController> terrainTemplatesA;
    public List<TerrainTemplateController> terrainTemplatesB;
    public List<TerrainTemplateController> terrainTemplatesC;
    public float terrainTemplateWidth;

    [Header("Generator Area")]
    public Camera gameCamera;
    public float areaStartOffset;
    public float areaEndOffset;
    public ScoreController score;

    [Header("Force Early Template")]
    public List<TerrainTemplateController> earlyTerrainTemplates;

    private const float debugLineHeight = 10.0f;

    private float GetHorizontalPositionStart()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(0f, 0f)).x + areaStartOffset;
    }

    private float GetHorizontalPositionEnd()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(1f, 0f)).x + areaEndOffset;
    }

    private List<GameObject> spawnedTerrain;

    private float lastGeneratedPositionX;


    private float lastRemovedPositionX;

    // pool list
    private Dictionary<string, List<GameObject>> pool;

    private void Start()
    {
        // init pool
        pool = new Dictionary<string, List<GameObject>>();

        spawnedTerrain = new List<GameObject>();

        lastGeneratedPositionX = GetHorizontalPositionStart();
        lastRemovedPositionX = lastGeneratedPositionX - terrainTemplateWidth;


        foreach (TerrainTemplateController terrain in earlyTerrainTemplates)
        {
            GenerateTerrain(lastGeneratedPositionX, terrain);
            lastGeneratedPositionX += terrainTemplateWidth;
        }


        while (lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }
    }

    private void Update()
    {
        while (lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }

        while (lastRemovedPositionX + terrainTemplateWidth < GetHorizontalPositionStart())
        {
            lastRemovedPositionX += terrainTemplateWidth;
            RemoveTerrain(lastRemovedPositionX);
        }
    }

    private void GenerateTerrain(float posX, TerrainTemplateController forceterrain = null)
    {
        GameObject newTerrain = null;

        // change the map every 10 score
        int currScore = (int)score.GetCurrentScore() % 30;

        if (currScore <= 10)
        {
            newTerrain = Instantiate(terrainTemplatesA[Random.Range(0, terrainTemplatesA.Count)].gameObject, transform);
        }

        else if (currScore <= 20)
        {
            newTerrain = Instantiate(terrainTemplatesB[Random.Range(0, terrainTemplatesB.Count)].gameObject, transform);
        }

        else if (currScore <= 30)
        {
            newTerrain = Instantiate(terrainTemplatesC[Random.Range(0, terrainTemplatesC.Count)].gameObject, transform);
        }

        newTerrain.transform.position = new Vector2(posX, 0f);

        spawnedTerrain.Add(newTerrain);
    }

    private void RemoveTerrain(float posX)
    {
        GameObject terrainToRemove = null;

        // find terrain at posX
        foreach (GameObject item in spawnedTerrain)
        {
            if (item.transform.position.x == posX)
            {
                terrainToRemove = item;
                break;
            }
        }
        // after found;
        if (terrainToRemove != null)
        {
            spawnedTerrain.Remove(terrainToRemove);
            Destroy(terrainToRemove);
        }
    }

    // pool function
    private GameObject GenerateFromPool(GameObject item, Transform parent)
    {
        if (pool.ContainsKey(item.name))
        {
            // if item available in pool
            if (pool[item.name].Count > 0)
            {
                GameObject newItemFromPool = pool[item.name][0];
                pool[item.name].Remove(newItemFromPool);
                newItemFromPool.SetActive(true);
                return newItemFromPool;
            }
        }
        else
        {
            // if item list not defined, create new one
            pool.Add(item.name, new List<GameObject>());
        }
        // create new one if no item available in pool
        GameObject newItem = Instantiate(item, parent);
        newItem.name = item.name;
        return newItem;
    }

    private void ReturnToPool(GameObject item)
    {
        if (!pool.ContainsKey(item.name))
        {
            Debug.LogError("INVALID POOL ITEM!!");
        }

        pool[item.name].Add(item);
        item.SetActive(false);
    }

    // debug
    private void OnDrawGizmos()
    {
        Vector3 areaStartPosition = transform.position;
        Vector3 areaEndPosition = transform.position;

        areaStartPosition.x = GetHorizontalPositionStart();
        areaEndPosition.x = GetHorizontalPositionEnd();

        Debug.DrawLine(areaStartPosition + Vector3.up * debugLineHeight / 2, areaStartPosition + Vector3.down * debugLineHeight / 2, Color.red);
        Debug.DrawLine(areaEndPosition + Vector3.up * debugLineHeight / 2, areaEndPosition + Vector3.down * debugLineHeight / 2, Color.red);
    }
}
