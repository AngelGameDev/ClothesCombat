using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(EscalatorStepGenerator))]
public class EscalatorStepGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.Space();

		EscalatorStepGenerator refScript = (EscalatorStepGenerator)target;

		if (GUILayout.Button("Apply"))
		{
			refScript.GenerateSteps();
		}
	}
}
#endif