using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using float3x4 = Unity.Mathematics.float3x4;
using quaternion = Unity.Mathematics.quaternion;

public class FractalUnity : MonoBehaviour
{
	struct FractalPart
	{
		public Vector3 direction, worldPosition;
		public Quaternion rotation, worldRotation;
		public float spinAngle;
	}

	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast,CompileSynchronously = true)]
	struct UpdateFractalLevelJob : IJobFor
	{
		public float spinAngleDelta;
		public float scale;

		[ReadOnly] public NativeArray<FractalPart> parents;
		public NativeArray<FractalPart> parts;

		[WriteOnly] public NativeArray<float3x4> matrices;

		public void Execute(int i)
		{
			FractalPart parent = parents[i / 5];
			FractalPart part = parts[i];

			part.spinAngle += spinAngleDelta;
			part.worldRotation = mul(parent.worldRotation, mul(part.rotation, quaternion.RotateY(part.spinAngle)));

			part.worldPosition = (float3)parent.worldPosition + mul(parent.worldRotation, 1.5f * scale * part.direction);
			parts[i] = part;

			float3x3 r = float3x3(part.worldRotation) * scale;
			matrices[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
			
			
			// matrices[i] = Matrix4x4.TRS(part.worldPosition, part.worldRotation, float3(scale));
		}
	}

	private NativeArray<FractalPart>[] parts;

	private NativeArray<float3x4>[] matrices;

	[SerializeField] private Mesh mesh;
	[SerializeField] private Material material;
	[SerializeField, Range(1, 8)] private int depth = 4;

	static float3[] directions =
	{
		up(), right(), left(), forward(), back()
	};

	static quaternion[] rotations =
	{
		quaternion.identity,
		quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI),
		quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI)
	};

	private static MaterialPropertyBlock propertyBlock;

	static readonly int matricesId = Shader.PropertyToID("_Matrices");

	ComputeBuffer[] matricesBuffers;

	void OnEnable()
	{
		parts = new NativeArray<FractalPart>[depth];
		matrices = new NativeArray<float3x4>[depth];
		matricesBuffers = new ComputeBuffer[depth];

		int stride = 12 * 4;
		for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
		{
			parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
			
			matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
			matricesBuffers[i] = new ComputeBuffer(length, stride);
		}

		parts[0][0] = CreatePart(0);
		for (int li = 1; li < parts.Length; li++)
		{
			NativeArray<FractalPart> levelParts = parts[li];
			for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
			{
				for (int ci = 0; ci < 5; ci++)
				{
					levelParts[fpi + ci] = CreatePart(ci);
				}
			}
		}

		propertyBlock ??= new();
	}

	void OnDisable()
	{
		for (int i = 0; i < matricesBuffers.Length; i++)
		{
			matricesBuffers[i].Release();
			parts[i].Dispose();
			matrices[i].Dispose();
		}

		parts = null;
		matrices = null;
		matricesBuffers = null;
	}

	void OnValidate()
	{
		if (parts != null && enabled)
		{
			OnDisable();
			OnEnable();
		}
	}

	void Update()
	{
		float spinAngleDelta = 0.125f * PI * Time.deltaTime;

		FractalPart rootPart = parts[0][0];
		rootPart.spinAngle += spinAngleDelta;
		rootPart.worldRotation = mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle));
		parts[0][0] = rootPart;

		float objectScale = transform.lossyScale.x;
		float3x3 r = float3x3(rootPart.worldRotation) * objectScale;
		matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);

		float scale = 1f;
		JobHandle jobHandle = default;
		for (int li = 1; li < parts.Length; li++)
		{
			scale *= 0.5f;
			// NativeArray<FractalPart> parentParts = parts[li - 1];
			// NativeArray<FractalPart> levelParts = parts[li];
			// NativeArray<Matrix4x4> levelMatrices = matrices[li];
			jobHandle = new UpdateFractalLevelJob()
			{
				spinAngleDelta = spinAngleDelta,
				scale = scale,
				parents = parts[li - 1],
				parts = parts[li],
				matrices = matrices[li]
			}.ScheduleParallel(parts[li].Length, 5, jobHandle);
			// jobHandle = job.Schedule(parts[li].Length, jobHandle);
			// for (int fpi = 0; fpi < parts[li].Length; fpi++)
			// {
			//  job.Execute(fpi);

			// FractalPart parent = parentParts[fpi / 5];
			// FractalPart part = levelParts[fpi];
			//
			// part.spinAngle += spinAngleDelta;
			// part.worldRotation = parent.worldRotation * (part.rotation * Quaternion.Euler(0f, part.spinAngle, 0f));
			// part.worldPosition = parent.worldPosition + parent.worldRotation * (1.5f * scale * part.direction);
			// levelParts[fpi] = part;
			//
			// levelMatrices[fpi] = Matrix4x4.TRS(
			// 	part.worldPosition, part.worldRotation, scale * Vector3.one
			// );
			// }
			jobHandle.Complete();
		}

		var bounds = new Bounds(Vector3.zero, 3f * Vector3.one);
		for (int i = 0; i < matricesBuffers.Length; i++)
		{
			ComputeBuffer buffer = matricesBuffers[i];
			buffer.SetData(matrices[i]);
			propertyBlock.SetBuffer(matricesId, buffer);
			material.SetBuffer(matricesId, buffer);
			Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, buffer.count, propertyBlock);
		}
	}

	FractalPart CreatePart(int childIndex) => new FractalPart
	{
		direction = directions[childIndex],
		rotation = rotations[childIndex]
	};
}
