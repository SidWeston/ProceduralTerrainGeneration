using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;
    public static float maxViewDst;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        //get map gen object
        mapGenerator = FindObjectOfType<MapGenerator>();

        //get the view distance of the lowest detailed lodmesh (furthest away) to find the max view distance
        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        chunkSize = mapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        //update visible chunks on initial start
        UpdateVisibleChunks();
    }

    void Update()
    {
        //set viewer position in relation to the map scale
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

        //if the viewer has moved update the chunks
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        //loop through all the chunks and set them invisible
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        //get the coords of the current chunk the viewer is in
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        //loop through all chunks in the view distance and update them
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    //if the chunk doesnt exist and should be visible, create it
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }

            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        LODMesh collisionLODMesh;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            //add all mesh components for a game object in unity
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            //set scale based on the map scale
            meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                //find what lod mesh should be used for the collider and set that as the collision mesh
                //lower lod mesh will still be effective for collision but more efficient 
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                if (detailLevels[i].useForCollider)
                {
                    collisionLODMesh = lodMeshes[i];
                }
            }
            //request map data
            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            //when recieved...
            this.mapData = mapData;
            mapDataReceived = true;
            //...update the terrain chunk
            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if(mapDataReceived)
            {
                //get the distance from the viewer
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                //check if the distance from the viewer is close enough to be visible
                bool visible = viewerDstFromNearestEdge <= maxViewDst;

                if(visible)
                {
                    int lodIndex = 0;

                    //check what lod mesh should be rendered.
                    for(int i = 0; i < detailLevels.Length - 1; i++)
                    { 
                        if(viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //if the level of detail index has changed
                    if(lodIndex != previousLODIndex)
                    {
                        //check if the mesh for that lodindex needs to be requested
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if(lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if(!lodMesh.hasRequestedMesh)
                        {
                            //get the mesh for the relevant lod index
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    if(lodIndex == 0)
                    {
                        if(collisionLODMesh.hasMesh)
                        {
                            meshCollider.sharedMesh = collisionLODMesh.mesh;
                        }
                        else if(!collisionLODMesh.hasRequestedMesh)
                        {
                            collisionLODMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }

    }

    class LODMesh
    {

        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }

    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
        public bool useForCollider;
    }

}
