using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorInspector : Editor {
    bool autoUpdate = true;

    public override void OnInspectorGUI() {
        autoUpdate = EditorGUILayout.Toggle("Auto Update", autoUpdate);

        TerrainGenerator terrainGenerator = (TerrainGenerator)target;
        if (DrawDefaultInspector()) {
            if (autoUpdate) {
                terrainGenerator.Generate();
            }
        }

        if (GUILayout.Button("Generate")) {
            terrainGenerator.Generate();
        }
    }
}
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour {
    public int width = 8;
    public int height = 8;
    public int depth = 8;
    public float noiseScale = 0.1f;
    public Vector3 offset;

    public float threshold = 0.5f;

    public float dotSize = 0.25f;
    public bool drawDots = false;

    private float[,,] noiseMap;

    private readonly List<Vector3> vertices = new();
    private readonly List<int> triangles = new();

    private MeshFilter meshFilter;
    
    public void Generate() {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        GenerateNoiseMap();
        MarchingCubes();
        GenerateMesh();
    }

    private void Start() {
        meshFilter = GetComponent<MeshFilter>();
        GenerateNoiseMap();
        MarchingCubes();
        GenerateMesh();
    }

    private void Update() {
        // GenerateNoiseMap();
        // MarchingCubes();
        // GenerateMesh();
    }

    private void OnDrawGizmosSelected() {
        if (!drawDots || !Application.isPlaying) {
            return;
        }

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {
                    float value = noiseMap[x, y, z];
                    Vector3 position = new(x, y, z);
                    Gizmos.color = new Color(value, value, value);
                    Gizmos.DrawSphere(position, dotSize);
                }
            }
        }
    }

    private void GenerateMesh() {
        Mesh mesh = new() {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        meshFilter.mesh = mesh;
    }

    private void MarchingCubes() {
        vertices.Clear();
        triangles.Clear();

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

                    MarchCube(new Vector3Int(x, y, z), configIndex);
                }
            }
        }
    }

    private void MarchCube(Vector3Int position, int configIndex) {
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

    private void GenerateNoiseMap() {
        noiseMap = new float[width + 1, height + 1, depth + 1];

        for (int x = 0; x < width + 1; x++) {
            for (int y = 0; y < height + 1; y++) {
                for (int z = 0; z < depth + 1; z++) {
                    // noiseMap[x, y, z] = PerlinNoise3D(x * noiseScale + offset.x, y * noiseScale + offset.y, z * noiseScale + offset.z);
                    noiseMap[x, y, z] = Noises.GeneratePlaneTerrainValue(new Vector3(x, y, z), offset, 1/noiseScale, 2);

                    // float currentHeight = height * Mathf.PerlinNoise(x * noiseScale, z * noiseScale);
                    // float distToSufrace;

                    // if (y <= currentHeight - 0.5f)
                    //     distToSufrace = 0f;
                    // else if (y > currentHeight + 0.5f)
                    //     distToSufrace = 1f;
                    // else if (y > currentHeight)
                    //     distToSufrace = y - currentHeight;
                    // else
                    //     distToSufrace = currentHeight - y;

                    // noiseMap[x, y, z] = distToSufrace;
                }
            }
        }
    }
}
