using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(TextureScaler))]
public class TextureScalerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.Space();

		TextureScaler refScript = (TextureScaler)target;
		if (GUILayout.Button("Refresh Material"))
		{
			refScript.RefreshMaterial();
		}

		if (GUILayout.Button("Reset Fields"))
		{
			refScript.ResetFields();
		}
	}
}
#endif