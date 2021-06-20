
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Spawnpoint                                                     v0.0_2018.06.20
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
/// Manages spawnpoints for players or ammo. Runs in editor.
/// </summary>
public class Spawnpoint : MonoBehaviour 
{
	#region Constants

	private const string TAG_SPAWN_PLAYER = "Spawnpoint Player";
	private const string TAG_SPAWN_AMMO = "Spawnpoint Ammo";

	#endregion

	#region Enums

	public enum Type : int
	{
		PLAYER,
		AMMO
	}

	#endregion

	#region Classes

	[System.Serializable]
	public class GizmoSettings
	{
		[Tooltip("If true, will show even when not the selected GameObject.")]
		public bool showWhenNotSelected = true;

		[Tooltip("If true, the projector mat is automatically loaded when the type is changed in " +
			"editor mode.")]
		public bool automaticallySetProjectorMat = true;

		[Space(10)]

		[Tooltip("The color of the spawnpoint sphere for a player spawnPoint. This does not affect " +
			"gameplay.")]
		public Color gizmoColorPointPlayer;

		[Tooltip("The color of the lines of the spawn point gizmo for a player spawnPoint. This does not " +
			"affect gameplay.")]
		public Color gizmoColorLinesPlayer;

		[Space(5)]

		[Tooltip("The color of the spawnpoint sphere for an ammo spawnPoint. This does not affect " +
		"gameplay.")]
		public Color gizmoColorPointAmmo;

		[Tooltip("The color of the lines of the spawn point gizmo for an ammo spawnPoint. This does not " +
			"affect gameplay.")]
		public Color gizmoColorLinesAmmo;

		[Space(5)]

		[Tooltip("The radius of the spawn point gizmo. This does not affect gameplay.")]
		public float gizmoPointRadius;

		[Tooltip("The width of the base square gizmo. This does not affect gameplay.")]
		public float gizmoBaseWidth;
	}

	#endregion

	#region Inspector Fields

	[Header("Dependancies")]
	[Tooltip("A reference to the projector material for a player spawnPoint. Probably don't " +
		"change this.")]
	[SerializeField]
	private Material refProjectorMatPlayer;
	[Tooltip("A reference to the projector material for an ammo spawnPoint. Probably don't " +
		"change this.")]
	[SerializeField]
	private Material refProjectorMatAmmo;

	[Space(10)]

	[Header("General Settings")]
	[Tooltip("The type of Spawner. This changes the run-time behaviour.")]
	public Type type;

	[Tooltip("The player spawn height. Probably don't change this.")]
	[SerializeField]
	private float playerHeight = 0.86f;

	[Space(10)]

	[SerializeField]
	private GameObject ammoPrefab;

	[SerializeField]
	private float ammoSpawnTimeMin;
	[SerializeField]
	private float ammoSpawnTimeMax;

	[Space(10)]

	[Tooltip("Settings for the editor scene view gizmos. Nothing in here affects gameplay.")]
	[SerializeField]
	private GizmoSettings gizmoSettings;

	#endregion

	#region Run-Time Fields

	private Projector refProjector;
	private Type activeType;
	private float ammoTimer;
	private float ammoSpawnTime;

	#endregion

	#region Monobehaviours

	private void Start()
	{
		SetupType();

		if (Application.isPlaying)
		{
			if (refProjector == null)
			{
				refProjector = GetComponentInChildren<Projector>();

				if (refProjector == null)
				{ 
					Destroy(gameObject);
				}
			}
			refProjector.enabled = false;
		}

		ammoSpawnTime = Random.Range(ammoSpawnTimeMin, ammoSpawnTimeMax);
	}

	private void Update()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (UnityEditor.Selection.Contains(gameObject) && 
				gizmoSettings.automaticallySetProjectorMat && activeType != type)
			{
				SetupType();
			}

			return;
		}
#endif

		if (type == Type.AMMO)
		{
			UpdateAmmo();
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
		Gizmos.color = (type == Type.PLAYER ? gizmoSettings.gizmoColorPointPlayer : 
			gizmoSettings.gizmoColorPointAmmo);
		Gizmos.DrawSphere(transform.position + (Vector3.up * playerHeight), gizmoSettings.gizmoPointRadius);

		Gizmos.color = (type == Type.PLAYER ? gizmoSettings.gizmoColorLinesPlayer :
			gizmoSettings.gizmoColorLinesAmmo);
		Gizmos.DrawWireSphere(transform.position + (Vector3.up * playerHeight), gizmoSettings.gizmoPointRadius);

		Gizmos.DrawLine(transform.position + (Vector3.up * playerHeight), transform.position);

		float w = gizmoSettings.gizmoBaseWidth / 2f;
		Gizmos.DrawLine
		(
			transform.position + (transform.right * w) + (transform.forward * w),
			transform.position + (transform.right * -w) + (transform.forward * w)
		);
		Gizmos.DrawLine
		(
			transform.position + (transform.right * -w) + (transform.forward * -w),
			transform.position + (transform.right * w) + (transform.forward * -w)
		);
		Gizmos.DrawLine
		(
			transform.position + (transform.right * -w) + (transform.forward * w),
			transform.position + (transform.right * -w) + (transform.forward * -w)
		);
		Gizmos.DrawLine
		(
			transform.position + (transform.right * w) + (transform.forward * w),
			transform.position + (transform.right * w) + (transform.forward * -w)
		);
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Sets the projector materials based on the type. Called when type change is detected 
	/// and at Start().
	/// </summary>
	private void SetupType()
	{
		if (refProjector == null)
		{
			refProjector = GetComponentInChildren<Projector>();

			if (refProjector == null)
			{
				return;
			}
		}

		switch (type)
		{
			case Type.PLAYER:
				refProjector.material = refProjectorMatPlayer;
				tag = TAG_SPAWN_PLAYER;
				break;
			case Type.AMMO:
				refProjector.material = refProjectorMatAmmo;
				tag = TAG_SPAWN_AMMO;
				break;
		}

		activeType = type;
	}

	private void UpdateAmmo()
	{
		ammoTimer += Time.deltaTime;
		if (ammoTimer >= ammoSpawnTime)
		{
			ammoTimer = 0f;
			ammoSpawnTime = Random.Range(ammoSpawnTimeMin, ammoSpawnTimeMax);
			Instantiate(ammoPrefab, transform.position + (Vector3.up * 1f), Quaternion.identity);
		}
	}

	#endregion

	#region Public Methods

	public Vector3 GetSpawnPos()
	{
		return transform.position + (Vector3.up * playerHeight);
	}

	#endregion
}
