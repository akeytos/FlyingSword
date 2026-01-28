using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    [System.Serializable]
    public struct PoolItem
    {
        public string enemyTypeID;  // �rn: "Goblin", "Goril"
        public GameObject prefab;   // Prefab dosyas�
        public int poolSize;        // Ka� tane haz�rda beklesin? (�rn: 150)
    }

    public List<PoolItem> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    void InitializePool()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (PoolItem item in pools)
        {
            if (string.IsNullOrWhiteSpace(item.enemyTypeID))
            {
                Debug.LogWarning("EnemyPool: Bos enemyTypeID bulundu, atlandi.");
                continue;
            }

            if (item.prefab == null)
            {
                Debug.LogWarning($"EnemyPool: '{item.enemyTypeID}' prefab atanmamis, atlandi.");
                continue;
            }

            if (!poolDictionary.TryGetValue(item.enemyTypeID, out Queue<GameObject> objectPool))
            {
                objectPool = new Queue<GameObject>();
                poolDictionary.Add(item.enemyTypeID, objectPool);
            }
            else
            {
                Debug.LogWarning($"EnemyPool: '{item.enemyTypeID}' icin tekrarli kayit var. Havuz birlestiriliyor.");
            }

            for (int i = 0; i < item.poolSize; i++)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.SetActive(false); // Ba�ta gizli
                obj.transform.SetParent(this.transform); // Hiyerar�i temiz kals�n
                objectPool.Enqueue(obj);
            }
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Havuzda {tag} diye bir d��man yok!");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // E�er havuzdaki obje zaten aktifse (Yani ekrandaki limit dolduysa)
        // �ki se�ene�in var: Ya do�urmazs�n ya da en uzaktakini �al�p buraya getirirsin.
        // Biz basit�e: Aktifse bile kapat�p yeni yerine ta��y�p a�aca��z (Reuse).
        objectToSpawn.SetActive(false);

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        // Kulland���m�z� s�ran�n en sonuna at�yoruz
        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    // D��man �l�nce Destroy() yerine bunu �a��raca��z!
    public void ReturnToPool(GameObject enemy)
    {
        enemy.SetActive(false);
    }
}