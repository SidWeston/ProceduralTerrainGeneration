using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TreeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    [Header("Raycast Settings")]
    [SerializeField] private int density;

    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    [SerializeField] private Vector2 xRange;
    [SerializeField] private Vector2 zRange;

    [Header("Prefab Variation Settings")]
    [SerializeField, Range(0, 1)] private float rotateTowardsNormal;
    [SerializeField] private Vector2 rotationRange;
    [SerializeField] private Vector3 minScale;
    [SerializeField] private Vector3 maxScale;

#if UNITY_EDITOR

    public void Generate()
    {
        Clear();
        for(int i = 0; i < density; i++)
        {

            float sampleX = Random.Range(xRange.x, xRange.y);
            float sampleY = Random.Range(zRange.x, zRange.y);
            Vector3 rayStart = new Vector3(sampleX, maxHeight, sampleY);

            if(!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                continue;
            }
            if(hit.point.y < minHeight)
            {
                continue;
            }

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

    public void Clear()
    {
        while(transform.childCount != 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

#endif
}
