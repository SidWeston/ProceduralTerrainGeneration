using UnityEngine;
using System.Collections;

public static class FalloffGenerator
{

    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    public static float[,] CreateFalloffMap(int size, float falloffStart, float falloffEnd, Vector2 offset)
    {
        float[,] heightMap = new float[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                //get the position of the falloff map, 
                //the offset it divided by 10 to give greater control in the editor
                Vector2 position = new Vector2
                (
                    (float)x / size * 2 - 1 + (offset.x / 10),
                    (float)y / size * 2 - 1 + (offset.y / 10)
                );

                //find which value is closer to the edge
                float t = Mathf.Max(Mathf.Abs(position.x), Mathf.Abs(position.y));

                if (t < falloffStart)
                {
                    heightMap[x, y] = 0;
                }
                else if (t > falloffEnd)
                {
                    heightMap[x, y] = 1;
                }
                else
                {
                    heightMap[x, y] = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(falloffStart, falloffEnd, t));
                }
            }
        }

        return heightMap;
    }

    static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}