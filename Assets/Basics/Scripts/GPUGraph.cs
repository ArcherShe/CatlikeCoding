using System;
using UnityEngine;
using UnityEngine.Rendering;


public class GPUGraph : MonoBehaviour
{
    [SerializeField, Range(0, 200)] private int resolution = 50;

    [SerializeField] private FunctionLibrary.FunctionName function;

    [SerializeField, Min(0f)] private float functionDuration = 1f, transitionDuration = 1f;

    [SerializeField] private TransitionMode transitionMode;

    [SerializeField] private ComputeShader computeShader;

    [SerializeField] private Material material;

    [SerializeField] private Mesh mesh;
    
    float duration;
    bool transitioning;
    FunctionLibrary.FunctionName transitionFunction;
    ComputeBuffer positionsBuffer;

    private static readonly int positionsId = Shader.PropertyToID("_Positions");
    private static readonly int resolutionId = Shader.PropertyToID("_Resolution");
    private static readonly int stepId =Shader.PropertyToID("_Step");
    private static readonly int timeId = Shader.PropertyToID("_Time");
    
    void OnEnable ()
    {
        positionsBuffer = new ComputeBuffer(resolution * resolution, 3 * 4);
    }

    private void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

    void Update ()
    {
        UpdateFunctionOnGPU();
    }

    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        
        computeShader.SetBuffer(0, positionsId, positionsBuffer);

        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(0, groups, groups, 1);

        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);
        
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(
            mesh, 0, material, bounds, positionsBuffer.count
        );
    }
}
