using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(PlaneMeshGenerator))]
public class PlaneMeshGeneratorInspector : Editor {
    bool autoUpdate = false;

    public override void OnInspectorGUI() {
        autoUpdate = EditorGUILayout.Toggle("Auto Update", autoUpdate);

        PlaneMeshGenerator generator = (PlaneMeshGenerator)target;
        if (DrawDefaultInspector()) {
            if (autoUpdate) {
                generator.GeneratePlaneMesh();
            }
        }

        if (GUILayout.Button("Generate")) {
            generator.GeneratePlaneMesh();
        }
    }
}
#endif

public class PlaneMeshGenerator : MonoBehaviour {
    [SerializeField] Vector2Int size = new(100, 100);
    [SerializeField] string meshName = "PlaneMesh";
    public void GeneratePlaneMesh() {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(size.x + 1) * (size.y + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[size.x * size.y * 6];

        // Generate vertices and UVs
        for (int y = 0; y <= size.y; y++) {
            for (int x = 0; x <= size.x; x++) {
                int index = y * (size.x + 1) + x;
                vertices[index] = new Vector3(x - size.x/2.0f, 0, y - size.y/2.0f);
                uv[index] = new Vector2((float)x / size.x, (float)y / size.y);
            }
        }

        // Generate triangles
        for (int y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++) {
                int startIndex = (y * size.x + x) * 6;
                int vertexIndex = y * (size.x + 1) + x;

                triangles[startIndex] = vertexIndex;
                triangles[startIndex + 1] = vertexIndex + size.x + 1;
                triangles[startIndex + 2] = vertexIndex + 1;

                triangles[startIndex + 3] = vertexIndex + 1;
                triangles[startIndex + 4] = vertexIndex + size.x + 1;
                triangles[startIndex + 5] = vertexIndex + size.x + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.name = meshName;

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
