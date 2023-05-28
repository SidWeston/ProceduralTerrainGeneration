using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode 
    {
        NoiseMap, 
        Mesh, 
        FalloffMap 
    };

    public DrawMode drawMode;

    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public Material terrainMaterial;

    [Range(0, 6)]
    public int editorPreviewLOD;

    public bool autoUpdate;

    float[,] falloffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private void Awake()
    {
        //update the mesh heights on game start to avoid issues
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
    }
 
    //callback to when any data script is edited when auto update is turned on
    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
            //update textures too when noise and terrain values updated to stop texture not working
            OnTextureValuesUpdated();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public int mapChunkSize
    {
        get
        {
            return 239;
        }
    }
#if UNITY_EDITOR
    public void DrawMapInEditor()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if(drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD), terrainData.uniformScale);
        }
        else if(drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.CreateFalloffMap(mapChunkSize, terrainData.falloffStart, terrainData.falloffEnd, terrainData.offset)));
        }
    }

#endif
    //multi threading for performance
    //only during runtime
    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock(mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod);
        lock(meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        //run through the threads
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode);

        if(terrainData.useFalloff)
        {

            falloffMap = FalloffGenerator.CreateFalloffMap(mapChunkSize + 2, terrainData.falloffStart, terrainData.falloffEnd, terrainData.offset);

            for(int y = 0; y < mapChunkSize + 2; y++)
            {
                for(int x = 0; x < mapChunkSize + 2; x++)
                {
                    if(terrainData.useFalloff)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                    }
                }
            }

        }



        return new MapData(noiseMap);
    }

    void OnValidate()
    {
        //subscribe to relevant update functions for data update events
        //unsubscribe first so functions cant be called twice
        if(terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if(noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if(textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }

    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }

}


public struct MapData
{
    public readonly float[,] heightMap;


    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}
