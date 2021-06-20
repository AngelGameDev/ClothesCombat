
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Step Generator                                                 v0.1_2018.06.23
// 
//  AUTHOR:  Angel Rodriguez Jr.
//  CONTACT: angel.rodriguez.gamedev@gmail.com
//
//  Copyright (C) 2018, Angel Rodriguez Jr. All Rights Reserved.
//-------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

/// <summary>
/// Editor tool. On inspector button input, generates steps on along a ramp surface to the desired 
/// specifications.
/// </summary>
public class StepGenerator : MonoBehaviour 
{
	#region Inspector Fields

	[Header("Dependancies")]

	[Tooltip("Reference to the step part prefab to build the stairs with.")]
	[SerializeField]
	private GameObject prefabStepPart;

	[Tooltip("The Transform to set as the parent for all generated steps.")]
	[SerializeField]
	private Transform stepsParent;

	[Space(10)]

	[Header("Generator Settings")]

	[Tooltip("The number of steps to generate along the ramp surface.")]
	[SerializeField]
	private int stepsCount = 10;

	[Tooltip("The amount to shrink the width of the steps in meters, creating a margin.")]
	[SerializeField]
	private float widthMargin = 0.1f;

	[Tooltip("The amount to offset the depth outward (move the steps further off the ramp). " +
		"You probably don't want to change this.")]
	[SerializeField]
	private float depthOffset = 0.07f;

	#endregion

	#region Run-Time Fields

	private List<GameObject> steps;

	#endregion

	#region Monobehaviours

	private void Awake()
	{
		if (Application.isPlaying)
		{
			DestroyImmediate(this);
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Generates steps to specification.
	/// </summary>
	/// <param name="stepsCount">The number of steps to make.</param>
	/// <param name="widthMargin">The width margin on each side of the steps, in meters.</param>
	/// <param name="depthOffset">The depth offset (forward position off ramp) to add to steps.</param>
	public void UpdateSteps(int stepsCount, float widthMargin, float depthOffset)
	{
		this.stepsCount = stepsCount;
		this.widthMargin = widthMargin;
		this.depthOffset = depthOffset;

		DeleteSteps();	
		
		float stepDepth = Mathf.Abs(transform.lossyScale.z) / stepsCount;
		float stepHeight = Mathf.Abs(transform.lossyScale.y) / stepsCount;

		for (int i=0; i<stepsCount; i++)
		{
			steps.Add(Instantiate(prefabStepPart, stepsParent));

			steps[i].transform.localEulerAngles = new Vector3(0f, 0f, 90f);

			steps[i].transform.position = transform.position - 
				(transform.forward * (-stepDepth * i + depthOffset)) + 
				(transform.up * (stepHeight) * i);

			steps[i].transform.localScale = new Vector3
			(
				Mathf.Clamp(1 - widthMargin, 0f, float.MaxValue),
				stepHeight / transform.lossyScale.y,
				stepDepth / transform.lossyScale.z
			);
		}
	}

	/// <summary>
	/// Generates steps to specification.
	/// </summary>
	public void UpdateSteps()
	{
		UpdateSteps(stepsCount, widthMargin, depthOffset);
	}

	/// <summary>
	/// Generates steps to specification.
	/// </summary>
	/// <param name="stepsCount">The number of steps to make.</param>
	public void UpdateSteps(int stepsCount)
	{
		UpdateSteps(stepsCount, widthMargin, depthOffset);
	}

	/// <summary>
	/// Generates steps to specification.
	/// </summary>
	/// <param name="stepsCount">The number of steps to make.</param>
	/// <param name="widthMargin">The width margin on each side of the steps, in meters.</param>
	public void UpdateSteps(int stepsCount, float widthMargin)
	{
		UpdateSteps(stepsCount, widthMargin, depthOffset);
	}

	/// <summary>
	/// Deletes all existing steps.
	/// </summary>
	public void DeleteSteps()
	{
		if (steps != null)
		{
			foreach (GameObject step in steps)
			{ 
				DestroyImmediate(step);
			}
		}

		steps = new List<GameObject>();
	}

	#endregion
}
