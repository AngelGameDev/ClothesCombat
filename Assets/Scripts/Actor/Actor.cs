using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Actor base class. Handles billboard shader communication.
/// </summary>
public class Actor : MonoBehaviour 
{
	#region Inspector Fields

	[Header("Actor Settings")]
	[Tooltip("A reference to the base MeshRenderer that contains the directional " +
		"material.")]
	[SerializeField]
	protected Renderer meshRenderer;

	[Tooltip("A reference to the target gameobject's transform, to extract an active facing " +
		"angle and prevent local rotation. This may end up being the same gameobject that " +
		"holds the MeshRenderer.")]
	[SerializeField]
	protected Transform gameobjectTranformReference;

	[Tooltip("A reference to the directional billboard material to use when the actor is still.")]
	[SerializeField]
	protected Material materialStanding;

	[Tooltip("A reference to the animated directional billboard material to use when the actor " +
		"is moving.")]
	[SerializeField]
	protected Material materialMoving;

	[Tooltip("The offset to apply to the shader rotation. Adjust this until " +
	"visuall correct (the player sprite matches the forward direction).")]
	[SerializeField]
	private float rotationOffset;

	#endregion

	#region Monobehaviours

	protected virtual void Update()
	{
		ClearRotation();
		UpdateMaterialAngle();
	}

	#endregion

	#region Protected Methods

	/// <summary>
	/// Zeros out the global euler of the visual element.
	/// </summary>
	protected void ClearRotation()
	{
		gameobjectTranformReference.eulerAngles = Vector3.zero;
	}

	/// <summary>
	/// Updates the _Angle property of the directional material.
	/// </summary>
	protected virtual void UpdateMaterialAngle()
	{
		float angle = transform.localEulerAngles.y;
		angle = (angle > 180) ? angle - 360 : angle;

		meshRenderer.material.SetFloat("_Angle", angle + rotationOffset);
	}

	/// <summary>
	/// Sets the base sprite renderer's material to the desired material. Opptionally, updates
	/// the material angle field.
	/// </summary>
	/// <param name="material">The material to change to.</param>
	/// <param name="setMaterial">If true, updates the angle field in the new material.</param>
	protected void SetMaterial(Material material, bool setMaterial = false)
	{
		meshRenderer.material = material;

		if (setMaterial)
		{ 
			UpdateMaterialAngle();
		}
	}

	#endregion
}
