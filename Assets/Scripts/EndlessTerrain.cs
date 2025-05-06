using System;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
    [SerializeField] Transform viewer;
    [SerializeField] float maxViewDistance;
    [SerializeField] Vector3 chunkSize;
    Dictionary<Vector3Int, TerrainChunk> chunks = new();
    HashSet<TerrainChunk> activeChunks = new();
    TerrainGenerator terrainGenerator;

    Vector3Int ToTerrainCoord(Vector3 position) {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / chunkSize.x),
            Mathf.FloorToInt(position.y / chunkSize.y),
            Mathf.FloorToInt(position.z / chunkSize.z)
        );
    }

    void Awake() {
        terrainGenerator = GetComponent<TerrainGenerator>();
    }

    void Update() {
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks() {
        Vector3Int currentChunkCoord = ToTerrainCoord(viewer.position);
        Vector3Int range = new Vector3Int(
            Mathf.CeilToInt(maxViewDistance / chunkSize.x),
            Mathf.CeilToInt(maxViewDistance / chunkSize.y),
            Mathf.CeilToInt(maxViewDistance / chunkSize.z)
        );
        Func<Vector3, bool> isInRange = (Vector3 coord) => {
            return Math.Abs(coord.x - currentChunkCoord.x) <= range.x &&
                   Math.Abs(coord.y - currentChunkCoord.y) <= range.y &&
                   Math.Abs(coord.z - currentChunkCoord.z) <= range.z;
        };
        foreach (TerrainChunk chunk in activeChunks) {
            if (!isInRange(chunk.coord)) {
                chunk.SetActive(false);
            }
        }
        activeChunks.Clear();
        for (int x = -range.x; x <= range.x; x++) {
            for (int y = -range.y; y <= range.y; y++) {
                for (int z = -range.z; z <= range.z; z++) {
                    Vector3Int chunkCoord = new Vector3Int(
                        currentChunkCoord.x + x,
                        currentChunkCoord.y + y,
                        currentChunkCoord.z + z
                    );
                    if (!chunks.ContainsKey(chunkCoord)) {
                        chunks[chunkCoord] = new TerrainChunk(chunkCoord, terrainGenerator, transform);
                    }
                    chunks[chunkCoord].SetActive(true);
                    activeChunks.Add(chunks[chunkCoord]);
                }
            }
        }
    }
    
    class TerrainChunk {
        GameObject meshObject;
        public Vector3 coord {get; private set; }

        public TerrainChunk(Vector3Int coord, TerrainGenerator generator, Transform parent = null) {
            meshObject = generator.GenerateTerrain(coord, parent);
        }
        
        public void SetActive(bool isActive) {
            meshObject.SetActive(isActive);
        }
    }
}
