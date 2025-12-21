using System.Collections.Generic;
using UnityEngine;

public class PropSpawner : MonoBehaviour
{
    [Header("Çevre Objeleri")]
    public string[] propTags;       // Örn: "Rock", "Wall", "Tree"
    public int densityPerChunk = 5;
    public float heightOffset = 0f;

    // Hangi zeminde hangi taþlar var?
    private Dictionary<GameObject, List<GameObject>> chunkPropMap = new Dictionary<GameObject, List<GameObject>>();

    public void SpawnProps(GameObject chunkObj, float chunkSize)
    {
        List<GameObject> activeProps = new List<GameObject>();

        for (int i = 0; i < densityPerChunk; i++)
        {
            float randomX = Random.Range(-chunkSize / 2f, chunkSize / 2f);
            float randomZ = Random.Range(-chunkSize / 2f, chunkSize / 2f);
            Vector3 spawnPos = chunkObj.transform.position + new Vector3(randomX, heightOffset, randomZ);

            string randomTag = propTags[Random.Range(0, propTags.Length)];

            // Havuzdan çek
            GameObject prop = ObjectPooler.Instance.SpawnFromPool(randomTag, spawnPos, Quaternion.identity);

            if (prop != null)
            {
                activeProps.Add(prop);
            }
        }

        if (!chunkPropMap.ContainsKey(chunkObj))
        {
            chunkPropMap.Add(chunkObj, activeProps);
        }
    }

    public void RecycleProps(GameObject chunkObj)
    {
        if (chunkPropMap.ContainsKey(chunkObj))
        {
            foreach (GameObject prop in chunkPropMap[chunkObj])
            {
                ObjectPooler.Instance.ReturnToPool(prop);
            }
            chunkPropMap.Remove(chunkObj);
        }
    }
}