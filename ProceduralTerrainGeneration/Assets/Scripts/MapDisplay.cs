using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour
{
    //components needed to display the different maps
    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, float scale)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshFilter.transform.localScale = Vector3.one * scale;
    }

}
