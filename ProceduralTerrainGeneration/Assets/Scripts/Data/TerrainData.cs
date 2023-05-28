using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class TerrainData : UpdatableData {
	
	[Header("Mesh Scale")]
	[Space(3)]
	public float uniformScale = 2.5f;

	[Header("Falloff Settings")]
	[Space(3)]
	public bool useFalloff;
	[Range(0, 1)] public float falloffStart;
	[Range(0, 1)] public float falloffEnd;
	public Vector2 offset;

	[Space(10)]

	[Header("Height Settings")]
	[Space(3)]
	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public float minHeight 
	{
		get 
		{
			return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
		}
	}

	public float maxHeight 
	{
		get 
		{
			return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
		}
	}
}
