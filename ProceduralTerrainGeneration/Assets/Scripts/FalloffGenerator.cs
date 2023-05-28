using UnityEngine;
using System.Collections;

public static class FalloffGenerator
{
    public static float[,] CreateFalloffMap(int size, float falloffStart, float falloffEnd, Vector2 offset)
    {
        float[,] heightMap = new float[size, size];

        //loop through the map size to create a grid off falloff values
        for(int y = 0; y < size; y++)
        {
            for(int x = 0; x < size; x++)
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

                //clamp the values
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
}