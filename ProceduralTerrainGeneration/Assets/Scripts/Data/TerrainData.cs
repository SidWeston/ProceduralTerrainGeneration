using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class TerrainData : UpdatableData {
	
	public float uniformScale = 2.5f;

	public bool useFlatShading;
	public bool useFalloff;
	[Header("Falloff Settings")]
	[Range(0, 1)] public float falloffStart;
	[Range(0, 1)] public float falloffEnd;
	public Vector2 offset;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public float minHeight {
		get {
			return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate (0);
		}
	}

	public float maxHeight {
		get {
			return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate (1);
		}
	}
}
