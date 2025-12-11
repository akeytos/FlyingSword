using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public LevelSettings levelSettings; 
    public Transform player;

    private Dictionary<Vector2, GameObject> activeChunks = new Dictionary<Vector2, GameObject>();
    private Vector2 currentPlayerCoord;

    void Start()
    {
        if (levelSettings == null)
        {
            Debug.LogError("MapGenerator: Level Settings dosyasý atanmamýþ inspector'dan atayýn.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (player == null) return;

        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        int currentX = Mathf.RoundToInt(player.position.x / levelSettings.chunkSize);
        int currentZ = Mathf.RoundToInt(player.position.z / levelSettings.chunkSize);
        currentPlayerCoord = new Vector2(currentX, currentZ);

        for (int xOffset = -levelSettings.viewDistance; xOffset <= levelSettings.viewDistance; xOffset++)
        {
            for (int zOffset = -levelSettings.viewDistance; zOffset <= levelSettings.viewDistance; zOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentX + xOffset, currentZ + zOffset);

                if (!activeChunks.ContainsKey(viewedChunkCoord))
                {
                    SpawnChunk(viewedChunkCoord);
                }
            }
        }

        CleanupChunks();
    }

    void SpawnChunk(Vector2 coord)
    {
        Vector3 position = new Vector3(
            coord.x * levelSettings.chunkSize,
            0,
            coord.y * levelSettings.chunkSize
        );

        GameObject newChunk = ObjectPooler.Instance.SpawnFromPool(levelSettings.groundTag, position, Quaternion.identity);

        if (newChunk != null)
        {
            activeChunks.Add(coord, newChunk);
        }
    }

    void CleanupChunks()
    {
        List<Vector2> keysToRemove = new List<Vector2>();

        foreach (var item in activeChunks)
        {
            float distance = Vector2.Distance(currentPlayerCoord, item.Key);

            if (distance > levelSettings.viewDistance + 1)
            {
                ObjectPooler.Instance.ReturnToPool(item.Value);
                keysToRemove.Add(item.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            activeChunks.Remove(key);
        }
    }

    void OnDrawGizmos()
    {
        if (levelSettings == null || !levelSettings.showGizmos) return;

        Gizmos.color = levelSettings.gizmoColor;

        Vector3 center = (player != null) ? player.position : transform.position;

        
        int gridX = Mathf.RoundToInt(center.x / levelSettings.chunkSize);
        int gridZ = Mathf.RoundToInt(center.z / levelSettings.chunkSize);

      
        int size = levelSettings.viewDistance * 2 + 1; 
        float totalSize = size * levelSettings.chunkSize;

        Vector3 snapPos = new Vector3(gridX * levelSettings.chunkSize, 0, gridZ * levelSettings.chunkSize);

        Gizmos.DrawWireCube(snapPos, new Vector3(totalSize, 1, totalSize));
    }
}