using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class NoiseData : UpdatableData 
{
	[Header("Normalize Mode")]
	[Space(3)]
	public Noise.NormalizeMode normalizeMode;

	[Space(5)]
	[Header("Noise Settings")]
	[Space(3)]

	public float noiseScale;
	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	protected override void OnValidate() 
	{
		//clamp these values to prevent errors
		if (lacunarity < 1) 
		{
			lacunarity = 1;
		}
		if (octaves < 0) 
		{
			octaves = 0;
		}

		base.OnValidate();
	}

}
