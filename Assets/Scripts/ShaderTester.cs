using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

#if UNITY_EDITOR
[CustomEditor(typeof(ShaderTester))]
public class ShaderTesterInspector : Editor {
    // bool autoUpdate = true;

    public override void OnInspectorGUI() {
        // autoUpdate = EditorGUILayout.Toggle("Auto Update", autoUpdate);

        ShaderTester terrainGenerator = (ShaderTester)target;
        if (DrawDefaultInspector()) {
            // if (autoUpdate) {
            //     terrainGenerator.Generate();
            // }
        }

        if (GUILayout.Button("Test")) {
            terrainGenerator.Test();
        }
    }
}
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShaderTester : MonoBehaviour {
    [SerializeField] Vector3Int chunkSize;
    /// <summary>
    /// The size of the shader dispatch group.
    /// This should be the same as the size assigned in the compute shader.
    /// </summary>
    readonly Vector3Int shaderSize = new(8, 8, 8);
    [SerializeField] Vector3Int offset;

    [SerializeField] ComputeShader shader;
    [SerializeField] RenderTexture texture;

    public void Test() {
        if (chunkSize.x % shaderSize.x != 0 || chunkSize.y % shaderSize.y != 0 || chunkSize.z % shaderSize.z != 0) {
            Debug.LogWarning("Size is not divisible by shader size");
        }
        texture = new(chunkSize.x, chunkSize.y, 0, GraphicsFormat.R16_SFloat) {
            dimension = TextureDimension.Tex3D,
            volumeDepth = chunkSize.z,
            enableRandomWrite = true,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };
        texture.Create();
        int kernelHandle = shader.FindKernel("CSMain");
        shader.SetTexture(kernelHandle, "Result", texture);
        shader.SetInts("ChunkSize", chunkSize.x, chunkSize.y, chunkSize.z);
        shader.SetFloats("Offset", offset.x, offset.y, offset.z);
        shader.Dispatch(kernelHandle, chunkSize.x / shaderSize.x, chunkSize.y / shaderSize.y, chunkSize.z / shaderSize.z);
    }
}

