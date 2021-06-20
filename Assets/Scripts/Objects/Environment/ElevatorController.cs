
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Elevator Controller                                            v0.0_2018.06.26
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
/// Controls the setup and behaviour of elevators.
/// </summary>
public class ElevatorController : MonoBehaviour 
{
	#region Enums

	private enum ElevatorStage : int
	{
		HOLD,
		OPENING,
		CLOSING,
		ARRIVING,
		DEPARTING,
		MOVING
	}

	#endregion

	#region Classes

	[System.Serializable]
	public class GizmoSettings
	{
		[Tooltip("If true, will show even when not the selected GameObject.")]
		public bool showWhenNotSelected = true;

		[Space(10)]

		[Tooltip("The radius of the spheres at each of the stop points.")]
		public float gizmoStopPointRadius;

		[Tooltip("The alpha of the sphere material at each of the stop points.")]
		public float gizmoStopPointAlpha;

		[Space(10)]

		[Tooltip("The gizmo color for lines at the first stop. Transitions to the end color.")]
		public Color gizmoStartColor;

		[Tooltip("The gizmo color for lines at the last stop. Transitions from the first color.")]
		public Color gizmoEndColor;
	}

	#endregion

	#region Inspector Fields

	[Header("Dependancies")]

	[Tooltip("Reference to the door that will open and close as the elevator travels. All door " +
		"options should be adjsuted on that door script; the elevator itself only opens and " +
		"closes it.")]
	[SerializeField]
	private DoorController door;

	[Space(10)]

	[Tooltip("The list of stops (floors) that the elevator stops at. Shown in the gizmo in edit " +
		"mode. These are world positions (not relative to the elevator's position).")]
	[SerializeField]
	private Vector3[] stopPositions;

	[Tooltip("Optional list of doors to open/close at the same index stop position.")]
	[SerializeField]
	private DoorController[] autoDoors;

	[Space(10)]

	[Tooltip("How long in seconds it takes the elevator to travel from one floor to another. " +
		"Serves as the basis for the floor travel animation curve.")]
	[SerializeField]
	private float floorTravelTime;

	[Tooltip("The behaviour of the lerp as it goes from floor to floor over the duration " +
		"determined by the variable Floor Travel Time.")]
	[SerializeField]
	private AnimationCurve floorTravelCurve;

	[Space(10)]

	[Tooltip("How long after reaching the floor to wait before opening the door.")]
	[SerializeField]
	private float waitTimeBeforeDoorOpen;

	[Tooltip("How long after closing the door before moving on to the next floor.")]
	[SerializeField]
	private float waitTimeAfterDoorClose;

	[Tooltip("How long in seconds the door is held open for at each floor.")]
	[SerializeField]
	private float doorHoldOpenTime;

	[Space(10)]

	[SerializeField]
	private bool isGoingUp = true;

	[Tooltip("The stop, or floor, that the elevator is currently on. In the editor this acts " +
		"as the starting spot. The first stop is 0.")]
	[SerializeField]
	private int currentStop = 0;

	[Tooltip("The elevator's current stage. In the editor this acts as the starting stage.")]
	[SerializeField]
	private ElevatorStage elevatorStage = ElevatorStage.ARRIVING;

	[Space(10)]

	[SerializeField]
	private GizmoSettings gizmoSettings;

	#endregion

	#region Run-Time Fields

	public List<Actor> occupants;

	private float timer;
	private float progress;
	private int nextStop;

	#endregion

	#region Monobehaviours

	private void Start()
	{
		occupants = new List<Actor>();
		door.isAutomatic = false;
		transform.position = stopPositions[currentStop];
	}

	private void Update()
	{
		UpdateTimers();
		UpdateActivity();
		UpdateElevatorStage();
	}

	private void OnTriggerEnter(Collider other)
	{
		Actor newOccupant = other.gameObject.GetComponentInParent<Actor>();

		if (newOccupant != null)
		{
			occupants.Add(newOccupant);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Actor leavingOccupant = other.gameObject.GetComponentInParent<Actor>();

		if (leavingOccupant != null)
		{

			for (int i=0; i<occupants.Count; i++)
			{
				if (leavingOccupant == occupants[i])
				{
					occupants.RemoveAt(i);
					i--;
				}
			}
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
		for (int i=0; i<stopPositions.Length; i++)
		{
			// Color gradient
			float h1, h2, s1, s2, v1, v2;

			Color.RGBToHSV(gizmoSettings.gizmoStartColor, out h1, out s1, out v1);
			Color.RGBToHSV(gizmoSettings.gizmoEndColor, out h2, out s2, out v2);

			Gizmos.color = Color.HSVToRGB
			(
				Mathf.Lerp(h1, h2, (float)i / (stopPositions.Length-1)), 
				s1, 
				v1
			);

			// Stop Points
			Gizmos.color = new Color
			(
				Gizmos.color.r, 
				Gizmos.color.g, 
				Gizmos.color.b, 
				gizmoSettings.gizmoStopPointAlpha
			);

			Gizmos.DrawSphere
			(
				stopPositions[i],
				gizmoSettings.gizmoStopPointRadius
			);

			Gizmos.color = new Color
			(
				Gizmos.color.r,
				Gizmos.color.g,
				Gizmos.color.b,
				1f
			);

			Gizmos.DrawWireSphere
			(
				stopPositions[i],
				gizmoSettings.gizmoStopPointRadius
			);

			// Line between
			if (i > 0)
			{
				Gizmos.DrawLine
				(
					stopPositions[i-1],
					stopPositions[i]
				);
			}
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Updates timers and stage progress.
	/// </summary>
	private void UpdateTimers()
	{
		timer += Time.deltaTime;

		door.isOpen = 
			elevatorStage == ElevatorStage.OPENING || 
			elevatorStage == ElevatorStage.HOLD;
	}

	/// <summary>
	/// Uses timer progress to lerp the elevator motion.
	/// </summary>
	private void UpdateActivity()
	{
		if (stopPositions.Length < 2)
		{
			return;
		}
		
		if (elevatorStage == ElevatorStage.MOVING)
		{
			Vector3 oldPosition = transform.position;

			nextStop = currentStop + 1;
			if (!isGoingUp)
			{
				nextStop = currentStop - 1;

				if (nextStop < 0)
				{
					isGoingUp = true;
					nextStop = 1;
				}
			}
			else if (nextStop > stopPositions.Length - 1)
			{
				isGoingUp = false;
				nextStop = currentStop - 1;
			}

			transform.position = Vector3.Lerp
			(
				stopPositions[currentStop],
				stopPositions[nextStop],
				floorTravelCurve.Evaluate(timer / floorTravelTime)
			);

			foreach (Actor occupant in occupants)
			{
				occupant.passengerVector = transform.position - oldPosition;
			}
		}
	}

	/// <summary>
	/// Uses timer progress to check for stage transitions.
	/// </summary>
	private void UpdateElevatorStage()
	{
		switch (elevatorStage)
		{
			case ElevatorStage.ARRIVING:
				if (timer >= waitTimeBeforeDoorOpen)
				{
					elevatorStage = ElevatorStage.OPENING;
					timer = 0f;
					return;
				}
				break;
			case ElevatorStage.DEPARTING:
				if (timer >= waitTimeAfterDoorClose)
				{
					elevatorStage = ElevatorStage.MOVING;
					timer = 0f;
					return;
				}
				break;
			case ElevatorStage.MOVING:
				if (timer >= floorTravelTime)
				{
					elevatorStage = ElevatorStage.ARRIVING;
					currentStop = nextStop;
					timer = 0f;
					return;
				}
				break;
			case ElevatorStage.OPENING:
				if (door.progress >= 1f)
				{
					elevatorStage = ElevatorStage.HOLD;
					timer = 0f;

					if (autoDoors.Length > currentStop && autoDoors[currentStop] != null)
					{
						autoDoors[currentStop].isOpen = true;
					}

					return;
				}
				break;
			case ElevatorStage.HOLD:
				if (timer >= doorHoldOpenTime)
				{
					elevatorStage = ElevatorStage.CLOSING;
					timer = 0f;
					return;
				}
				break;
			case ElevatorStage.CLOSING:
				if (door.progress <= 0f)
				{
					elevatorStage = ElevatorStage.DEPARTING;
					timer = 0f;

					if (autoDoors.Length > currentStop && autoDoors[currentStop] != null)
					{
						autoDoors[currentStop].isOpen = false;
					}

					return;
				}
				break;
		}
	}

	#endregion
}
