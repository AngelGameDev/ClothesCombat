using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(InclineGenerator))]
public class InclineGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		InclineGenerator refScript = (InclineGenerator)target;
		if (refScript.transform.localScale != Vector3.one)
		{
			EditorGUILayout.HelpBox
			(
				"Local scale must be set to (1, 1, 1). Use size instead.",
				MessageType.Error,
				true
			);

			if (GUILayout.Button("Reset Scale"))
			{
				refScript.transform.localScale = Vector3.one;
			}
		}
		else if (refScript.transform.lossyScale != Vector3.one)
		{
			EditorGUILayout.HelpBox
			(
				"Do not child to a scaled parent.\nScale must be (1, 1, 1). Use size instead.",
				MessageType.Error,
				true
			);
		}
		else
		{
			DrawDefaultInspector();

			EditorGUILayout.Space();

			if (GUILayout.Button("Generate Ramp"))
			{
				refScript.GenerateRamp();
			}
			if (GUILayout.Button("Generate Stairs"))
			{
				refScript.GenerateStairs();
			}
			if (GUILayout.Button("Generate Escalator"))
			{
				refScript.GenerateEscalator();
			}

			EditorGUILayout.Space();

			GUIStyle style = new GUIStyle(GUI.skin.button);
			style.normal.textColor = Color.red;

			if (GUILayout.Button("Delete Previous", style))
			{
				refScript.DeleteLastIncline();
			}
		}
	}
}
#endif