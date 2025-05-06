using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorInspector : Editor {
    Vector3Int testCoord = new Vector3Int(0, 0, 0);

    public override void OnInspectorGUI() {

        TerrainGenerator terrainGenerator = (TerrainGenerator)target;
        DrawDefaultInspector();
        testCoord = EditorGUILayout.Vector3IntField("Test Coord", testCoord);
        
        if (GUILayout.Button("Test")) {
            TestGenerateTerrain(terrainGenerator);
        }
    }
    
    void TestGenerateTerrain(TerrainGenerator generator) {
        generator.GenerateTerrain(testCoord, generator.transform);
    }
}
#endif

public class TerrainGenerator : MonoBehaviour {
    public int width = 8;
    public int height = 8;
    public int depth = 8;
    public float noiseScale = 0.1f;
    public Vector3 offset;

    public float threshold = 0.5f;
    [SerializeField] Material material;
    
    public GameObject GenerateTerrain(Vector3Int coord, Transform parent = null) {
        GameObject meshObject = new GameObject($"Terrain {coord.x}_{coord.y}_{coord.z}");
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        var noiseMap = GenerateNoiseMap(coord);
        Mesh mesh = GenerateMesh(noiseMap);
        meshFilter.mesh = mesh;
        meshObject.transform.position = new Vector3(coord.x * width, coord.y * height, coord.z * depth);
        meshObject.transform.parent = parent;
        return meshObject;
    }
    
    float[,,] GenerateNoiseMap(Vector3Int coord) {
        float[,,] noiseMap = new float[width + 1, height + 1, depth + 1];
        Vector3 position = new Vector3(
            coord.x * width,
            coord.y * height,
            coord.z * depth
        );

        for (int x = 0; x < width + 1; x++) {
            for (int y = 0; y < height + 1; y++) {
                for (int z = 0; z < depth + 1; z++) {
                    noiseMap[x, y, z] = Noises.GeneratePlaneTerrainValue(position + new Vector3(x, y, z), offset, 1/noiseScale, 2);
                }
            }
        }
        return noiseMap;
    }
    
    Mesh GenerateMesh(float[,,] noiseMap) {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {
                    int configIndex = 0;
                    for (int i = 0; i < 8; i++) {
                        Vector3Int corner = new Vector3Int(x, y, z) + MarchingCubesTables.Corners[i];
                        if (noiseMap[corner.x, corner.y, corner.z] > threshold) {
                            configIndex |= 1 << i;
                        }
                    }
                    ProcessMarchingCube(new Vector3Int(x, y, z), configIndex, noiseMap, vertices, triangles);
                }
            }
        }
        Mesh mesh = new() {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        return mesh;
    }

    private void ProcessMarchingCube(Vector3Int position, int configIndex, float[,,] noiseMap, List<Vector3> vertices, List<int> triangles) {
        if (configIndex == 0 || configIndex == 255) return;

        for (int t = 0; t < 5; t++) {
            for (int v = 0; v < 3; v++) {
                int triangleValue = MarchingCubesTables.Triangles[configIndex, t * 3 + v];
                if (triangleValue == -1) return;

                Vector3Int edgeStart = MarchingCubesTables.Edges[triangleValue, 0] + position;
                Vector3Int edgeEnd = MarchingCubesTables.Edges[triangleValue, 1] + position;

                float StartValue = noiseMap[edgeStart.x, edgeStart.y, edgeStart.z] - threshold;
                float EndValue = noiseMap[edgeEnd.x, edgeEnd.y, edgeEnd.z] - threshold;
                float ratio = StartValue / (StartValue - EndValue);
                // Debug.Log(ratio);
                Vector3 vertex = Vector3.Lerp(edgeStart, edgeEnd, ratio);

                vertices.Add(vertex);
                triangles.Add(vertices.Count - 1);
            }
        }
    }
}
