
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Door Controller                                                v0.0_2018.06.25
// 
//  AUTHOR:  Angel Rodriguez Jr.
//  CONTACT: angel.rodriguez.gamedev@gmail.com
//
//  Copyright (C) 2018, Angel Rodriguez Jr. All Rights Reserved.
//-------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the behaviour of automatically sliding doors.
/// </summary>
public class DoorController : MonoBehaviour 
{
	#region Inspector Fields
	
	[Header("Dependancies")]

	[Tooltip("Reference for the left door.")]
	[SerializeField]
	private Transform leftDoor;

	[Tooltip("Reference for the right door.")]
	[SerializeField]
	private Transform rightDoor;

	[Space(10)]

	[Header("Settings")]

	[Tooltip("If true the door is opening or opened. If false the door is closing or " +
		"closed. In editor this is the starting state of the door.")]
	public bool isOpen = false;

	[Tooltip("If true the door can open automatically from its trigger colliders. If false, " +
		"the door can only be opened or closed by another script.")]
	public bool isAutomatic = true;

	[Tooltip("The local x position of a door when closed. Left door is right * -1.")]
	[SerializeField]
	private float closedDoorLocalX = 0.5f;

	[Tooltip("The local x position of a door when open. Left door is right * -1.")]
	[SerializeField]
	private float openDoorLocalX = 1.5f;

	[Tooltip("The local x scale of a door when closed..")]
	[SerializeField]
	private float closedDoorLocalXScale = 1f;

	[Tooltip("The local x scale of a door when open.")]
	[SerializeField]
	private float openDoorLocalXScale = 1f;

	[Tooltip("The time it takes to go from one state to the other.")]
	[SerializeField]
	private float transitionDuration;

	[Space(5)]

	[Tooltip("The animation curve across the 0f - 1f lerp animation of the door. Test " +
		"with progress meter.")]
	[SerializeField]
	private AnimationCurve animationCurve;

	[Tooltip("The percentage progress towards open. In editor this is the starting " +
		"movement state of the door.")]
	[Range(0f, 1f)]
	public float progress = 0f;

	#endregion

	#region Run-Time Fields

	private int occupancy = 0;

	#endregion

	#region Monobehaviours

	private void Update()
	{
		UpdateProgress();
		UpdateVisual();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isAutomatic || 
			other.gameObject.layer == 14 ||
			other.gameObject.layer == 15 ||
			other.gameObject.layer == 16)
		{
			return;
		}

		occupancy++;
	}

	private void OnTriggerExit(Collider other)
	{
		if (!isAutomatic ||
			other.gameObject.layer == 14 ||
			other.gameObject.layer == 15 ||
			other.gameObject.layer == 16)
		{
			return;
		}

		occupancy--;
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{ 
			UpdateVisual();
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Updates the rate over time.
	/// </summary>
	private void UpdateProgress()
	{
		if (isAutomatic)
		{
			isOpen = occupancy > 0;
		}

		float rate = (1f / transitionDuration) * Time.deltaTime;

		if (isOpen)
		{ 
			progress += rate;
			if (progress > 1f)
			{
				progress = 1f;
			}
		}
		else
		{
			progress -= rate;
			if (progress < 0f)
			{
				progress = 0f;
			}
		}
	}

	/// <summary>
	/// Using the progress and lerp curve, sets the positions of the doors.
	/// </summary>
	private void UpdateVisual()
	{
		leftDoor.localPosition = new Vector3
		(
			Mathf.Lerp(-closedDoorLocalX, -openDoorLocalX, animationCurve.Evaluate(progress)),
			leftDoor.localPosition.y,
			leftDoor.localPosition.z
		);

		rightDoor.localPosition = new Vector3
		(
			Mathf.Lerp(closedDoorLocalX, openDoorLocalX, animationCurve.Evaluate(progress)),
			leftDoor.localPosition.y,
			leftDoor.localPosition.z
		);

		// Scale
		leftDoor.localScale = new Vector3
		(
			Mathf.Lerp
			(
				-closedDoorLocalXScale, 
				-openDoorLocalXScale, 
				animationCurve.Evaluate(progress)
			),
			leftDoor.localScale.y,
			leftDoor.localScale.z
		);

		rightDoor.localScale = new Vector3
		(
			Mathf.Lerp
			(
				-closedDoorLocalXScale,
				-openDoorLocalXScale,
				animationCurve.Evaluate(progress)
			),
			rightDoor.localScale.y,
			rightDoor.localScale.z
		);
	}

	#endregion
}