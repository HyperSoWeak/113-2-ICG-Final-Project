using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class RippleManager : MonoBehaviour {
    [Header("Ripple Settings")]
    [SerializeField] ComputeShader rippleShader;
    [SerializeField] Vector2Int resolution;
    [SerializeField] float dispersion;
    [SerializeField] Material waterSurface;

    readonly Vector3Int shaderSize = new(32, 32, 1);
    [SerializeField] RenderTexture NewState, CurrentState, PreviousState;
    [SerializeField] Vector3 dropPos = Vector3.zero;

    void Awake() {
        InitializeTextures(ref NewState);
        InitializeTextures(ref CurrentState);
        InitializeTextures(ref PreviousState);
    }

    void Update() {
        Graphics.CopyTexture(CurrentState, PreviousState);
        Graphics.CopyTexture(NewState, CurrentState);

        rippleShader.SetTexture(0, "Result", NewState);
        rippleShader.SetTexture(0, "Current", CurrentState);
        rippleShader.SetTexture(0, "Previous", PreviousState);
        rippleShader.SetVector("dropPos", dropPos);
        rippleShader.SetFloat("dispersion", dispersion);
        rippleShader.Dispatch(0, resolution.x / shaderSize.x, resolution.y / shaderSize.y, 1);
        
        waterSurface.SetTexture("_RippleDisplacement", NewState);
    }

    void InitializeTextures(ref RenderTexture texture) {
        texture = new(resolution.x, resolution.y, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R16_SNorm) {
            enableRandomWrite = true,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Repeat,
        };
        texture.Create();
    }
}
