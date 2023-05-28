using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TreeSpawner))]
public class TreeSpawnerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		TreeSpawner spawner = (TreeSpawner)target;

		DrawDefaultInspector();

		if (GUILayout.Button("Generate"))
		{
			spawner.Generate();
		}

		if(GUILayout.Button("Clear"))
        {
			spawner.Clear();
        }
	}
}
