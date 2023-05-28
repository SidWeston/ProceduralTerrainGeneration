using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TreeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    [Header("Raycast Settings")]
    [SerializeField] private int density;

    [Space(5)]

    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    [SerializeField] private Vector2 xRange;    //x is lower value y is upper value, will find point in between lower and upper values
    [SerializeField] private Vector2 zRange;    //x is lower value y is upper value, will find point in between lower and upper values

    [Header("Prefab Variation Settings")]
    [SerializeField, Range(0, 1)] private float rotateTowardsNormal;
    [SerializeField] private Vector2 rotationRange; //x is lower value y is upper value, will find point in between lower and upper values
    [SerializeField] private Vector3 minScale;
    [SerializeField] private Vector3 maxScale;

    [Space(10)]

    [SerializeField] private bool clearOnGenerate = false;



#if UNITY_EDITOR

    public void Generate()
    {
        if(clearOnGenerate)
        {
            Clear();
        }

        //loop through how many trees it should attempt to spawn
        for(int i = 0; i < density; i++)
        {
            //get random points in the range
            float sampleX = Random.Range(xRange.x, xRange.y);
            float sampleY = Random.Range(zRange.x, zRange.y);
            Vector3 rayStart = new Vector3(sampleX, maxHeight, sampleY);

            //fire a ray from the max height downwards at the random point
            if(!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                //if it doesnt hit, go to the next ray
                continue;
            }
            if(hit.point.y < minHeight)
            {
                //if it hits below the minimum height, go to the next ray
                continue;
            }

            //if here it hit in a valid position.
            //instantiate the prefab under the terrain mesh object
            //set the position to the hit point and randomly set the rotation and scale based on the input values to create variation
            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
            instantiatedPrefab.transform.position = hit.point;
            instantiatedPrefab.transform.Rotate(Vector3.up, Random.Range(rotationRange.x, rotationRange.y), Space.Self);
            instantiatedPrefab.transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation * Quaternion.FromToRotation(instantiatedPrefab.transform.up, hit.normal), rotateTowardsNormal);
            instantiatedPrefab.transform.localScale = new Vector3
                (
                    Random.Range(minScale.x, maxScale.x),
                    Random.Range(minScale.y, maxScale.y),
                    Random.Range(minScale.z, maxScale.z)
                );
        }
    }

    //destroy all the trees that are a child of this object
    public void Clear()
    {
        while(transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

#endif
}
