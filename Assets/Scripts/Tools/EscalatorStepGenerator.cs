
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Escalator Step Generator                                       v0.1_2018.07.04
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
/// Editor tool. On inspector button input, generates escalator steps on along a ramp surface to 
/// the desired specifications.
/// </summary>
public class EscalatorStepGenerator : MonoBehaviour
{
	#region Enums

	public enum Mode : int
	{ 
		ASCEND,
		DESCEND,
		STAIRS
	}

	#endregion

	#region Classes

	[System.Serializable]
	public class Step
	{
		public GameObject refObject;
		public float currentTime;

		public Step(GameObject refObject, float currentTime)
		{
			this.refObject = refObject;
			this.currentTime = currentTime;
		}
	}

	#endregion

	#region Inspector Fields

	[Header("Dependancies")]

	[Tooltip("Reference to the step part prefab to build the stairs with.")]
	[SerializeField]
	private GameObject prefabStepPart;

	[Tooltip("The Transform to set as the parent for all generated steps.")]
	[SerializeField]
	private Transform stepsParent;

	[Tooltip("The transform to set as the parent for all visual aspects.")]
	[SerializeField]
	private Transform bodyParent;

	[Tooltip("A reference to the transform for the ramp propel trigger, in order to rotate it to the " +
		"correct angle.")]
	[SerializeField]
	private Transform rampPropel;

	[Space(10)]

	[Header("Generator Settings")]

	[Tooltip("The number of steps to generate along the ramp surface.")]
	[SerializeField]
	private int stepsCount = 20;

	[Tooltip("The amount to shrink the width of the steps in meters, creating a margin.")]
	[SerializeField]
	private float widthMargin = 0.15f;

	[Tooltip("The amount to pad the height of each step downward, to prevent coming up over " +
		"the ramp.")]
	[SerializeField]
	private float heightPadding = 0.2f;

	[Tooltip("The amount to offset the depth outward (move the steps further off the ramp). " +
		"The heightOffset will affect how correct this is, and likely must change together.")]
	[SerializeField]
	private float depthOffset = 0.07f;

	[Tooltip("The amount to offset the height downward (move the steps further into the " +
		"ground). The depthOffset will affect how correct this is, and likely must change " +
		"together.")]
	[SerializeField]
	private float heightOffset = 0.1f;

	[Tooltip("How far in meters, in both top and bottom of the escalator, do the steps extend " +
		"out past the ramp, to make space for the escalator curve. Adjust this if necessary, " +
		"until it looks right in motion.")]
	[SerializeField]
	private float endExpandLength = 1f;

	[Space(10)]

	[Header("Movement Settings")]

	[Tooltip("If true, the animation will play in Edit Mode.")]
	[SerializeField]
	private bool animateInEditMode = true;

	[Tooltip("The mode of the stair movement. Ascend, descend, or stairs (no movement).")]
	[SerializeField]
	private Mode mode = Mode.ASCEND;

	[Tooltip("The time in seconds that it takes for a single step to go from one end to the " +
		"other. Higher is slower. This directly affects the actor propelling settings.")]
	[SerializeField]
	private float escalatorTravelTime = 10f;

	[Tooltip("The height curve of a step over time. Depth is linear.")]
	[SerializeField]
	private AnimationCurve heightCurve;

	#endregion

	#region Run-Time Fields

	private Step[] steps;
	private Vector3 firstPos;
	private Vector3 lastPos;
	private double editorSavedTime;

	#endregion

	#region Monobehaviours

	private void Start()
	{
		GenerateSteps();
	}

	private void Update()
	{
		if (stepsCount < 1)
		{
			stepsCount = 1;
		}

		UpdateSteps();
	}

	private void OnDrawGizmos()
	{
		if (animateInEditMode && !Application.isPlaying && steps != null)
		{				
			UpdateSteps();
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Sets up all the generator fields. A value of -1 means no change (use the current).
	/// </summary>
	public void Setup
	(
		int stepsCount, 
		float widthMargin, 
		float heightPadding, 
		float depthOffset, 
		float heightOffset, 
		float endExpandLength)
	{
		if (stepsCount != -1)
		{
			this.stepsCount = stepsCount;
			if (stepsCount < 2)
			{ 
				stepsCount = 2;
			}
		}

		if (widthMargin != -1)
		{
			this.widthMargin = Mathf.Clamp(widthMargin, 0, float.MaxValue);
		}

		if (heightPadding != -1)
		{
			this.heightPadding = Mathf.Clamp(heightPadding, 0, float.MaxValue);
		}

		if (depthOffset != -1f)
		{
			this.depthOffset = Mathf.Clamp(depthOffset, 0, float.MaxValue);
		}

		if (heightOffset != -1f)
		{
			this.heightOffset = heightOffset;
		}

		if (endExpandLength != -1)
		{
			this.endExpandLength = Mathf.Clamp(endExpandLength, 0, float.MaxValue);
		}
	}

	/// <summary>
	/// Updates step positions based on their t value, using a lerp.
	/// </summary>
	public void UpdateSteps()
	{
		if (escalatorTravelTime < 0.001f)
		{
			escalatorTravelTime = 0.001f;
		}

		if (mode == Mode.STAIRS || steps == null)
		{
			return;
		}

		foreach (Step step in steps)
		{
			if (step.refObject != null)
			{
				// ASCEND
				if (mode == Mode.ASCEND)
				{ 
					if (!Application.isPlaying)
					{
#if UNITY_EDITOR
						if (editorSavedTime != 0)
						{
							step.currentTime += (float)(EditorApplication.timeSinceStartup - editorSavedTime);
						}
#endif
					}
					else
					{
						step.currentTime += Time.deltaTime;
					}

					if (step.currentTime >= escalatorTravelTime)
					{
						step.currentTime -= escalatorTravelTime;
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
							step.currentTime -= (float)(EditorApplication.timeSinceStartup - editorSavedTime);
						}
#endif
					}
					else
					{
						step.currentTime -= Time.deltaTime;
					}

					if (step.currentTime < 0)
					{
						step.currentTime += escalatorTravelTime;
					}
				}

				float t = step.currentTime / escalatorTravelTime;

				step.refObject.transform.localPosition = new Vector3
				(
					0f,
					Mathf.Lerp
					(
						firstPos.y,
						lastPos.y,
						heightCurve.Evaluate(t)
					) + (heightPadding / 2f) + heightOffset,
					Mathf.Lerp(firstPos.z, lastPos.z, t)
				);
			}
		}
#if UNITY_EDITOR
		editorSavedTime = EditorApplication.timeSinceStartup;
#endif
	}

	/// <summary>
	/// Generates steps to specification.
	/// </summary>
	/// <param name="stepsCount">The number of steps to make.</param>
	/// <param name="widthMargin">The width margin on each side of the steps, in meters.</param>
	/// <param name="depthOffset">The depth offset (forward position off ramp) to add to steps.</param>
	public void GenerateSteps(int stepsCount, float widthMargin, float depthOffset)
	{
		if (stepsCount < 1)
		{
			return;
		}

		this.stepsCount = stepsCount;
		this.widthMargin = widthMargin;
		this.depthOffset = depthOffset;

		DeleteSteps();

		float stepTime = escalatorTravelTime / stepsCount;
		float stepDepth = Mathf.Abs(transform.lossyScale.z) / stepsCount;
		float stepHeight = Mathf.Abs(transform.lossyScale.y) / stepsCount;
		Vector3 stepScale = new Vector3
		(
			Mathf.Clamp(1 - widthMargin, 0f, float.MaxValue),
			stepHeight / transform.lossyScale.y + heightPadding,
			stepDepth / transform.lossyScale.z + (endExpandLength / (stepsCount * 2f))
		);

		// First (bottom)
		GameObject newStepObj = Instantiate(prefabStepPart, stepsParent);

		//newStepObj.transform.localEulerAngles = new Vector3(0f, 0f, 90f);

		newStepObj.transform.position = transform.position -
			(transform.forward * (depthOffset + endExpandLength)) +
			(transform.up * (-stepHeight + heightOffset));

		newStepObj.transform.localScale = stepScale;

		steps[0] = new Step(newStepObj, 0f);

		// Last (top)
		newStepObj = Instantiate(prefabStepPart, stepsParent);

		//newStepObj.transform.localEulerAngles = new Vector3(0f, 0f, 90f);

		// Set to last position, but will update to one before immediately after saving that position.
		newStepObj.transform.position = transform.position -
				(transform.forward * (-stepDepth * (stepsCount-1) + depthOffset - endExpandLength)) +
				(transform.up * (stepHeight) * ((stepsCount-1) + heightOffset));

		newStepObj.transform.localScale = stepScale;

		steps[stepsCount-1] = new Step(newStepObj, escalatorTravelTime - (escalatorTravelTime / stepsCount));

		// Save first and last positions.
		firstPos = steps[0].refObject.transform.localPosition;
		lastPos = steps[stepsCount-1].refObject.transform.localPosition;

		// Intermediate steps
		for (int i = 1; i < stepsCount-1; i++)
		{
			newStepObj = Instantiate(prefabStepPart, stepsParent);
			//newStepObj.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
			newStepObj.transform.localScale = stepScale;
			steps[i] = new Step(newStepObj, stepTime * i);
		}

		// Ramp propel angle
		float rampLength = Mathf.Sqrt(Mathf.Pow(transform.lossyScale.z, 2f) +
			Mathf.Pow(transform.lossyScale.y, 2f));

		rampPropel.localEulerAngles = new Vector3
		(
			Mathf.Asin(transform.lossyScale.z / rampLength) * (180f / Mathf.PI),
			rampPropel.localEulerAngles.y,
			rampPropel.localEulerAngles.z
		);

		UpdateSteps();
	}

	/// <summary>
	/// Generates steps to specification.
	/// </summary>
	public void GenerateSteps()
	{
		GenerateSteps(stepsCount, widthMargin, depthOffset);
	}

	/// <summary>
	/// Generates steps to specification.
	/// </summary>
	/// <param name="stepsCount">The number of steps to make.</param>
	public void GenerateSteps(int stepsCount)
	{
		GenerateSteps(stepsCount, widthMargin, depthOffset);
	}

	/// <summary>
	/// Generates steps to specification.
	/// </summary>
	/// <param name="stepsCount">The number of steps to make.</param>
	/// <param name="widthMargin">The width margin on each side of the steps, in meters.</param>
	public void GenerateSteps(int stepsCount, float widthMargin)
	{
		GenerateSteps(stepsCount, widthMargin, depthOffset);
	}

	/// <summary>
	/// Deletes all existing steps.
	/// </summary>
	public void DeleteSteps()
	{
		steps = new Step[stepsCount];

		DestroyImmediate(stepsParent.gameObject);

		GameObject newParent = new GameObject("Steps");
		newParent.transform.parent = bodyParent;
		newParent.transform.localPosition = Vector3.zero;
		newParent.transform.localRotation = Quaternion.identity;
		newParent.transform.localScale = Vector3.one;
		stepsParent = newParent.transform;
	}

	#endregion
}
