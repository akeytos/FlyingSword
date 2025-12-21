using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Düþman Ayarlarý")]
    public string[] enemyTags;       // Örn: "Sliceable", "FlyingEnemy"
    public int enemyCountPerChunk = 3;
    public float minHeight = 2f;     // Düþmanlar yerden ne kadar yüksekte olsun?
    public float maxHeight = 5f;     // Uçan düþmanlar için max yükseklik

    // Hangi zeminde hangi düþmanlar var?
    private Dictionary<GameObject, List<GameObject>> chunkEnemyMap = new Dictionary<GameObject, List<GameObject>>();

    public void SpawnEnemies(GameObject chunkObj, float chunkSize)
    {
        List<GameObject> activeEnemies = new List<GameObject>();

        for (int i = 0; i < enemyCountPerChunk; i++)
        {
            float randomX = Random.Range(-chunkSize / 2f, chunkSize / 2f);
            float randomZ = Random.Range(-chunkSize / 2f, chunkSize / 2f);

            // Düþmanlar için rastgele yükseklik (Uçan kýlýç oyunu sonuçta)
            float randomY = Random.Range(minHeight, maxHeight);

            Vector3 spawnPos = chunkObj.transform.position + new Vector3(randomX, randomY, randomZ);

            string randomTag = enemyTags[Random.Range(0, enemyTags.Length)];

            GameObject enemy = ObjectPooler.Instance.SpawnFromPool(randomTag, spawnPos, Quaternion.identity);

            if (enemy != null)
            {
                activeEnemies.Add(enemy);
            }
        }

        if (!chunkEnemyMap.ContainsKey(chunkObj))
        {
            chunkEnemyMap.Add(chunkObj, activeEnemies);
        }
    }

    public void RecycleEnemies(GameObject chunkObj)
    {
        if (chunkEnemyMap.ContainsKey(chunkObj))
        {
            foreach (GameObject enemy in chunkEnemyMap[chunkObj])
            {
                // Düþman ölmemiþse ve hala sahnedeyse havuza geri çek
                // (Ölenler zaten EnemyBase scriptiyle havuza dönmüþtü, sorun olmaz)
                if (enemy.activeInHierarchy)
                {
                    ObjectPooler.Instance.ReturnToPool(enemy);
                }
            }
            chunkEnemyMap.Remove(chunkObj);
        }
    }
}