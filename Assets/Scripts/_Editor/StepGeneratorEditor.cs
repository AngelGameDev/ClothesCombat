using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(StepGenerator))]
public class StepGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.Space();

		StepGenerator refScript = (StepGenerator)target;

		if (GUILayout.Button("Apply"))
		{
			refScript.UpdateSteps();
		}
	}
}
#endif