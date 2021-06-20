
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Incline Generator                                              v0.1_2018.06.22
// 
//  AUTHOR:  Angel Rodriguez Jr.
//  CONTACT: angel.rodriguez.gamedev@gmail.com
//
//  Copyright (C) 2018, Angel Rodriguez Jr. All Rights Reserved.
//-------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: Can't be scaled. Should never be the child of something scaled.

[ExecuteInEditMode]

/// <summary>
/// In edit mode handles the setup and generation of inclines like ramps, stairs, and escalators.
/// </summary>
public class InclineGenerator : MonoBehaviour 
{
	#region Classes

	[System.Serializable]
	public class Dependancies
	{
		public GameObject prefabRamp;
		public GameObject prefabStairs;
		public GameObject prefabEscalator;
	}

	[System.Serializable]
	public class GizmoSettings
	{
		[Tooltip("If true, will show even when not the selected GameObject.")]
		public bool showWhenNotSelected = true;

		[Space(10)]

		[Tooltip("The gizmo color for the ramp lines.")]
		public Color gizmoColorLines;

		[Tooltip("The gizmo color for the ramp.")]
		public Color gizmoColorRamp;
	}

	[System.Serializable]
	public class StairsSettings
	{
		[Tooltip("The number of steps to generate along the ramp surface.")]
		public int stepsCount = 10;

		[Tooltip("The amount to shrink the width of the steps in meters, creating a margin.")]
		public float widthMargin = 0.1f;

		[Tooltip("The amount to offset the depth outward (move the steps further off the ramp). " +
			"You probably don't want to change this.")]
		public float depthOffset = 0.07f;
	}

	[System.Serializable]
	public class EscalatorSettings
	{
		[Tooltip("The number of steps to generate along the ramp surface.")]
		public int stepsCount = 20;

		[Tooltip("The amount to shrink the width of the steps in meters, creating a margin.")]
		public float widthMargin = 0.15f;

		[Tooltip("The amount to pad the height of each step downward, to prevent coming up over " +
			"the ramp.")]
		public float heightPadding = 0.2f;

		[Tooltip("The amount to offset the depth outward (move the steps further off the ramp). " +
			"The heightOffset will affect how correct this is, and likely must change together.")]
		public float depthOffset = 0.07f;

		[Tooltip("The amount to offset the height downward (move the steps further into the " +
			"ground). The depthOffset will affect how correct this is, and likely must change " +
			"together.")]
		public float heightOffset = 0.1f;

		[Tooltip("How far in meters, in both top and bottom of the escalator, do the steps extend " +
			"out past the ramp, to make space for the escalator curve. Adjust this if necessary, " +
			"until it looks right in motion.")]
		public float endExpandLength = 1f;
	}

	#endregion

	#region Inspector Fields

	[Tooltip("If true, a new incline deletes the previous.")]
	[SerializeField]
	private bool deletePrevious;

	[Space(10)]

	[Tooltip("The width, height, and depth of the escalator.")]
	[SerializeField]
	private Vector3 size;

	[Space(10)]

	[SerializeField]
	private StairsSettings stairsSettings;

	[Space(10)]

	[SerializeField]
	private EscalatorSettings escalatorSettings;

	[Space(10)]

	[Tooltip("Maybe don't change any of this.")]
	[SerializeField]
	private Dependancies dependancies;

	[Space(10)]

	[Tooltip("Adjusts the gizmo colors and settings for how the gizmos are viewed in scene view.")]
	[SerializeField]
	private GizmoSettings gizmoSettings;

	#endregion

	#region Run-Time Fields

	private GameObject activeProduct;

	#endregion

	#region Monobehaviours

	private void Awake()
	{
		if (Application.isPlaying)
		{
			DestroyImmediate(gameObject);
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (!UnityEditor.Selection.Contains(gameObject) && gizmoSettings.showWhenNotSelected)
		{
			OnDrawGizmosSelected();
		}
	}
#endif

	private void OnDrawGizmosSelected()
	{
		//
		//         _. G -------- H        +Y
		//     _.-'   .    _.-'  :         |    +Z
		//   C --------- D       :         |  .'
		//   :        .  :       :         |.'  
		//   :        E  : .   . F         "------- +X
		//   :  .  '     :  _.-'
		//   A --- O --- B '
		//         |
		//      (origin)

		Vector3 X = transform.right * (size.x / 2f);
		Vector3 Y = transform.up * size.y;
		Vector3 Z = transform.forward * size.z;

		Vector3 A = transform.position - X;
		Vector3 B = transform.position + X;
		Vector3 C = transform.position - X + Y;
		Vector3 D = transform.position + X + Y;
		Vector3 E = A + Z;
		Vector3 F = B + Z;
		Vector3 G = C + Z;
		Vector3 H = D + Z;

		// Lines
		{ 
			Gizmos.color = gizmoSettings.gizmoColorLines;

			// Wire Cube
			{
				// Front
				Gizmos.DrawLine(A, B);
				Gizmos.DrawLine(B, D);
				Gizmos.DrawLine(D, C);
				Gizmos.DrawLine(C, A);

				// Back
				Gizmos.DrawLine(E, F);
				Gizmos.DrawLine(F, H);
				Gizmos.DrawLine(H, G);
				Gizmos.DrawLine(G, E);

				// Sides Top
				Gizmos.DrawLine(D, H);
				Gizmos.DrawLine(G, C);

				// Sides Bottom
				Gizmos.DrawLine(B, F);
				Gizmos.DrawLine(E, A);
			}

			// Diagonals
			{ 
				Gizmos.DrawLine(A, G);
				Gizmos.DrawLine(B, H);
			}
		}

		// Ramp
		{ 
			Gizmos.color = gizmoSettings.gizmoColorRamp;

			Mesh mesh = new Mesh();

			Vector3[] vertices = new Vector3[4];

			vertices[0] = A;
			vertices[1] = B;
			vertices[2] = G;
			vertices[3] = H;

			mesh.vertices = vertices;

			int[] tri = new int[6];

			tri[0] = 0;
			tri[1] = 2;
			tri[2] = 1;

			tri[3] = 2;
			tri[4] = 3;
			tri[5] = 1;

			mesh.triangles = tri;

			Vector3[] normals = new Vector3[4];

			normals[0] = -transform.forward;
			normals[1] = -transform.forward;
			normals[2] = -transform.forward;
			normals[3] = -transform.forward;

			mesh.normals = normals;

			Gizmos.DrawMesh(mesh);
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Generates a generic ramp with appropriate colliders and materials.
	/// </summary>
	public void GenerateRamp()
	{
		if (deletePrevious)
		{
			DeleteLastIncline();
		}

		// Place ramp part prefab to size
		activeProduct = Instantiate
		(
			dependancies.prefabRamp, 
			transform.position,
			transform.rotation
		);

		activeProduct.name = "Ramp";

		activeProduct.transform.localScale = size;
	}

	/// <summary>
	/// Generates a generic staircase with appropriate colliders and materials.
	/// </summary>
	public void GenerateStairs()
	{
		if (deletePrevious)
		{
			DeleteLastIncline();
		}

		// Place stairs prefab to size
		activeProduct = Instantiate
		(
			dependancies.prefabStairs,
			transform.position,
			transform.rotation
		);

		activeProduct.name = "Stairs";

		StepGenerator stepGenerator = activeProduct.GetComponent<StepGenerator>();
		stepGenerator.UpdateSteps
		(
			stairsSettings.stepsCount,
			stairsSettings.widthMargin,
			stairsSettings.depthOffset
		);

		activeProduct.transform.localScale = size;
	}

	/// <summary>
	/// Generates a generic escalator with appropriate colliders, materials, and scripts.
	/// </summary>
	public void GenerateEscalator()
	{
		if (deletePrevious)
		{
			DeleteLastIncline();
		}

		// Place stairs prefab to size
		activeProduct = Instantiate
		(
			dependancies.prefabEscalator,
			transform.position,
			transform.rotation
		);

		activeProduct.name = "Escalator";

		EscalatorStepGenerator stepGenerator = 
			activeProduct.GetComponent<EscalatorStepGenerator>();

		stepGenerator.Setup
		(
			escalatorSettings.stepsCount,
			escalatorSettings.widthMargin,
			escalatorSettings.heightPadding,
			escalatorSettings.depthOffset,
			escalatorSettings.heightOffset,
			escalatorSettings.endExpandLength
		);
		stepGenerator.UpdateSteps();

		activeProduct.transform.localScale = size;
	}

	/// <summary>
	/// Deletes the last generated incline product if one exists.
	/// </summary>
	public void DeleteLastIncline()
	{
		if (activeProduct == null)
		{
			return;
		}

		DestroyImmediate(activeProduct);
	}

	#endregion
}
