using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

#if UNITY_EDITOR
[CustomEditor(typeof(GeneratorGPU))]
public class GeneratorGPUInspector : Editor {
    bool autoUpdate = true;

    public override void OnInspectorGUI() {
        autoUpdate = EditorGUILayout.Toggle("Auto Update", autoUpdate);

        GeneratorGPU terrainGenerator = (GeneratorGPU)target;
        if (DrawDefaultInspector()) {
            if (autoUpdate) {
                terrainGenerator.Test();
            }
        }

        if (GUILayout.Button("Test")) {
            terrainGenerator.Test();
        }
    }
}
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GeneratorGPU : MonoBehaviour {
    [SerializeField] Vector3Int chunkSize;
    /// <summary>
    /// The size of the shader dispatch group.
    /// This should be the same as the size assigned in the compute shader.
    /// </summary>
    readonly Vector3Int shaderSize = new(8, 8, 8);
    [SerializeField] Vector3Int offset;
    [SerializeField] float threshold = 0.5f;

    [SerializeField] ComputeShader noiseShader, marchingCubesShader;
    [SerializeField] RenderTexture noiseMap;
    int maxVertexCount = 65535;

    struct TriangleData {
        public Vector3 v0;
        public Vector3 v1;
        public Vector3 v2;
    }

    public void Test() {
        GenerateNoise();
        GenerateMesh();
    }

    void GenerateNoise() {
        if (chunkSize.x % shaderSize.x != 0 || chunkSize.y % shaderSize.y != 0 || chunkSize.z % shaderSize.z != 0) {
            Debug.LogWarning("Size is not divisible by shader size");
        }
        noiseMap = new(chunkSize.x, chunkSize.y, 0, GraphicsFormat.R16_SFloat) {
            dimension = TextureDimension.Tex3D,
            volumeDepth = chunkSize.z,
            enableRandomWrite = true,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };
        noiseMap.Create();
        noiseShader.SetTexture(0, "Result", noiseMap);
        noiseShader.SetFloats("Offset", offset.x, offset.y, offset.z);
        noiseShader.Dispatch(0, chunkSize.x / shaderSize.x, chunkSize.y / shaderSize.y, chunkSize.z / shaderSize.z);
    }

    void GenerateMesh() {
        ComputeBuffer vertexBuffer = new(maxVertexCount / 3, sizeof(float) * 9, ComputeBufferType.Append);
        vertexBuffer.SetCounterValue(0);
        marchingCubesShader.SetFloat("Threshold", threshold);
        marchingCubesShader.SetTexture(0, "NoiseMap", noiseMap);
        marchingCubesShader.SetBuffer(0, "VertexBuffer", vertexBuffer);
        marchingCubesShader.Dispatch(0, chunkSize.x / shaderSize.x, chunkSize.y / shaderSize.y, chunkSize.z / shaderSize.z);

        ComputeBuffer countBuffer = new (1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer.CopyCount(vertexBuffer, countBuffer, 0);
        int[] countArray = { 0 };
        countBuffer.GetData(countArray);
        int triangleCount = countArray[0];
        countBuffer.Release();

        TriangleData[] triangleData = new TriangleData[triangleCount];
        vertexBuffer.GetData(triangleData, 0, 0, triangleCount);

        // Debug.Log("count: " + triangleCount);

        Vector3[] vertices = new Vector3[triangleCount * 3];
        int[] triangles = new int[triangleCount * 3];
        for (int i = 0; i < triangleCount; i++) {
            for (int j = 0; j < 3; j++) {
                triangles[i * 3 + j] = i * 3 + j;
            }
            vertices[i * 3 + 0] = triangleData[i].v0;
            vertices[i * 3 + 1] = triangleData[i].v1;
            vertices[i * 3 + 2] = triangleData[i].v2;
        }

        Mesh mesh = new() {
            vertices = vertices,
            triangles = triangles,
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }
}

