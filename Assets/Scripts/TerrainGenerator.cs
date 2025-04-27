using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    public int width = 8;
    public int height = 8;
    public int depth = 8;
    public float noiseScale = 0.1f;
    public float noiseAmplitude = 1f;
    public Vector3 offset;

    public GameObject pointPrefab;
    public float dotSize = 0.25f;
    private GameObject[,,] points;

    private void Start() {
        RenderPoints();
    }

    private void Update() {
        RenderPoints();
    }

    void RenderPoints() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        GenerateNoise();
    }

    void GenerateNoise() {
        points = new GameObject[width, height, depth];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {
                    float xCoord = (x + offset.x) * noiseScale;
                    float yCoord = (y + offset.y) * noiseScale;
                    float zCoord = (z + offset.z) * noiseScale;

                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    float value = sample * noiseAmplitude;

                    Vector3 position = new Vector3(x, y, z);

                    points[x, y, z] = Instantiate(pointPrefab, position, Quaternion.identity, transform);
                    points[x, y, z].transform.localScale = Vector3.one * dotSize;

                    Renderer renderer = points[x, y, z].GetComponent<Renderer>();
                    renderer.material.color = new Color(value, value, value);
                }
            }
        }
    }
}
