using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    [System.Serializable]
    public struct PoolItem
    {
        public string enemyTypeID;  // Örn: "Goblin", "Goril"
        public GameObject prefab;   // Prefab dosyasý
        public int poolSize;        // Kaç tane hazýrda beklesin? (Örn: 150)
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
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < item.poolSize; i++)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.SetActive(false); // Baþta gizli
                obj.transform.SetParent(this.transform); // Hiyerarþi temiz kalsýn
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(item.enemyTypeID, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Havuzda {tag} diye bir düþman yok!");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // Eðer havuzdaki obje zaten aktifse (Yani ekrandaki limit dolduysa)
        // Ýki seçeneðin var: Ya doðurmazsýn ya da en uzaktakini çalýp buraya getirirsin.
        // Biz basitçe: Aktifse bile kapatýp yeni yerine taþýyýp açacaðýz (Reuse).
        objectToSpawn.SetActive(false);

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        // Kullandýðýmýzý sýranýn en sonuna atýyoruz
        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    // Düþman ölünce Destroy() yerine bunu çaðýracaðýz!
    public void ReturnToPool(GameObject enemy)
    {
        enemy.SetActive(false);
    }
}