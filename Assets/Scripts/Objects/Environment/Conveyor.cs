
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Conveyor                                                       v0.0_2018.06.21
// 
//  AUTHOR:  Angel Rodriguez Jr.
//  CONTACT: angel.rodriguez.gamedev@gmail.com
//
//  Copyright (C) 2018, Angel Rodriguez Jr. All Rights Reserved.
//-------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]

/// <summary>
/// Handles the conveyor part whcih interfaces with an Actor to propell the in a desired direction
/// while making contact (on trigger).
/// </summary>
public class Conveyor : MonoBehaviour 
{
	#region Classes

	[System.Serializable]
	public class Gizmo
	{
		[Tooltip("if true, will show even when not the selected GameObject.")]
		public bool showWhenNotSelected = true;

		[Tooltip("The color of the animated gizmo lines.")]
		public Color color;

		[Tooltip("The number of gizmo arrows moving along the surface.")]
		public int animCount;

		[Tooltip("A factor multiplied onto the propelForce which determines how quickly the gizmo " +
			"animation plays.")]
		public float animForceFactor;

		[Tooltip("The height of each gizmo arrow.")]
		public float animArrowHeight;
	}

	#endregion

	#region Inspector Fields

	[Header("Settings")]
	[Tooltip("If true, the propell direction is -z. Otherwise it's +z (forward).")]
	[SerializeField]
	private bool flipDirection;

	[Tooltip("The magnitude of the propell vector (how fast it moves the actor).")]
	[SerializeField]
	private float propelForce;

	[Space(10)]

	[Tooltip("Settings for the editor scene view gizmos. Nothing in here affects gameplay.")]
	[SerializeField]
	private Gizmo gizmoSettings;

	#endregion

	#region Run-Time Fields

	// Gizmo
	private float[] gizmoArrowTimes;
	private Vector3 savedLossyScale;
	private double editorSavedTime;
	private float gizmoAnimDuration;
	private float savedAnimForceFactor;
	private float savedPropellForce;
	private int savedAnimCount;

	#endregion

	#region Monobehaviours

	private void OnTriggerStay(Collider other)
	{
		if (other.CompareTag("Actor"))
		{
			Actor otherActor = other.GetComponentInParent<Actor>();
			otherActor.propelVector = ((flipDirection? -1f : 1f) * transform.up) * 
				propelForce;
		}
	}

	private void OnDrawGizmos()
	{
#if UNITY_EDITOR
		if (!UnityEditor.Selection.Contains(gameObject) && gizmoSettings.showWhenNotSelected)
		{
			OnDrawGizmosSelected();
		}
#endif
	}

	private void OnDrawGizmosSelected()
	{
		// Outlines
		Gizmos.color = gizmoSettings.color;
		Gizmos.DrawLine
		(
			transform.position - 
				(transform.up * transform.lossyScale.y / 2f) -
				(transform.right * transform.lossyScale.x / 2f),
			transform.position -
				(transform.up * transform.lossyScale.y / 2f) +
				(transform.right * transform.lossyScale.x / 2f)
		);
		Gizmos.DrawLine
		(
			transform.position +
				(transform.up * transform.lossyScale.y / 2f) -
				(transform.right * transform.lossyScale.x / 2f),
			transform.position +
				(transform.up * transform.lossyScale.y / 2f) +
				(transform.right * transform.lossyScale.x / 2f)
		);
		Gizmos.DrawLine
		(
			transform.position +
				(transform.up * transform.lossyScale.y / 2f) +
				(transform.right * transform.lossyScale.x / 2f),
			transform.position -
				(transform.up * transform.lossyScale.y / 2f) +
				(transform.right * transform.lossyScale.x / 2f)
		);
		Gizmos.DrawLine
		(
			transform.position +
				(transform.up * transform.lossyScale.y / 2f) -
				(transform.right * transform.lossyScale.x / 2f),
			transform.position -
				(transform.up * transform.lossyScale.y / 2f) -
				(transform.right * transform.lossyScale.x / 2f)
		);

		// Check for rebuild flags
		if (gizmoArrowTimes == null || 
			gizmoSettings.animCount != savedAnimCount ||
			gizmoSettings.animForceFactor != savedAnimForceFactor ||
			propelForce != savedPropellForce ||
			transform.lossyScale.y != savedLossyScale.y)
		{
			savedAnimCount = gizmoSettings.animCount;
			savedAnimForceFactor = gizmoSettings.animForceFactor;
			savedPropellForce = propelForce;
			savedLossyScale = transform.lossyScale;

			GizmoBuildArrows();
		}

		// Update arrows
		for (int i=0; i<gizmoArrowTimes.Length; i++)
		{
			// ASCEND
			if (!flipDirection)
			{
				if (!Application.isPlaying)
				{
#if UNITY_EDITOR
					if (editorSavedTime != 0)
					{
						gizmoArrowTimes[i] += (float)(EditorApplication.timeSinceStartup - editorSavedTime);
					}
#endif
				}
				else
				{
					gizmoArrowTimes[i] += Time.deltaTime;
				}

				if (gizmoArrowTimes[i] >= gizmoAnimDuration)
				{
					gizmoArrowTimes[i] -= gizmoAnimDuration;
				}
			}
			// DESCEND
			else
			{
				if (!Application.isPlaying)
				{
#if UNITY_EDITOR
					if (editorSavedTime != 0)
					{
						gizmoArrowTimes[i] -= (float)(EditorApplication.timeSinceStartup - editorSavedTime);
					}
#endif
				}
				else
				{
					gizmoArrowTimes[i] -= Time.deltaTime;
				}

				if (gizmoArrowTimes[i] < 0)
				{
					gizmoArrowTimes[i] += gizmoAnimDuration;
				}
			}
		}

		// Draw arrows
		Vector3 centerPos = Vector3.zero;

		foreach (float arrowTime in gizmoArrowTimes)
		{
			float heightOffset = Mathf.Lerp
			(
				transform.lossyScale.y / -2f,
				transform.lossyScale.y / 2f,
				arrowTime / gizmoAnimDuration
			);

			centerPos = transform.position + 
				(transform.up * (heightOffset + (gizmoSettings.animArrowHeight / 2f)));

			Gizmos.DrawLine
			(
				centerPos,
				centerPos + 
					(transform.right * (transform.lossyScale.x / -2f)) +
					(transform.up * gizmoSettings.animArrowHeight * (flipDirection? 1 : -1))
			);
			Gizmos.DrawLine
			(
				centerPos,
				centerPos +
					(transform.right * (transform.lossyScale.x / 2f)) +
					(transform.up * gizmoSettings.animArrowHeight * (flipDirection ? 1 : -1))
			);

			// Second line for thickness
			centerPos += transform.up * 0.01f;

			Gizmos.DrawLine
			(
				centerPos,
				centerPos +
					(transform.right * (transform.lossyScale.x / -2f)) +
					(transform.up * gizmoSettings.animArrowHeight * (flipDirection ? 1 : -1))
			);
			Gizmos.DrawLine
			(
				centerPos,
				centerPos +
					(transform.right * (transform.lossyScale.x / 2f)) +
					(transform.up * gizmoSettings.animArrowHeight * (flipDirection ? 1 : -1))
			);
		}

#if UNITY_EDITOR
		editorSavedTime = EditorApplication.timeSinceStartup;
#endif
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Builds the array of gizmo arrows to specfication. Called whenever changes are made to the
	/// arrow specifications.
	/// </summary>
	private void GizmoBuildArrows()
	{
		gizmoArrowTimes = new float[gizmoSettings.animCount];

		gizmoAnimDuration = transform.lossyScale.y / (propelForce * gizmoSettings.animForceFactor);

		float durationStep = gizmoAnimDuration / gizmoSettings.animCount;

		for (int i=0; i<gizmoSettings.animCount; i++)
		{
			gizmoArrowTimes[i] = durationStep * i;
		}
	}

	#endregion
}
