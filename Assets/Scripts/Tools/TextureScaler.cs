
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Texture Scaler                                                 v1.0_2018.07.07
// 
//  AUTHOR:  Angel Rodriguez Lozada
//  CONTACT: angel.rodriguez.gamedev@gmail.com
//
//  Copyright (C) 2018, Angel Rodriguez Lozada
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
//  and associated documentation files (the "Software"), to deal in the Software without 
//  restriction, including without limitation the rights to use, copy, modify, merge, publish, 
//  distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
//  Software is furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all copies or 
//  substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
//  BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
//  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
//  DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//-------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

/// <summary>
/// Scales texture tiling in edit mode to match their in-game scale. Only works with normalized
/// meshes (1m max in all directions). Executes in edit mode. Must be on the GameObject with the
/// desired MeshRenderer, which has on it the desired texture to scale.
/// </summary>
public class TextureScaler : MonoBehaviour 
{
	#region Inspector Fields
	
	[Header("Prefab Settings")]
	[Tooltip("The default material that the object will have when instantiated from a prefab.")]
	[SerializeField]
	private Material defaultMaterial;

	[Header("Auto Settings")]

	[Tooltip("If true, automatically offset's x the tiling based on x scale.")]
	[SerializeField]
	private bool autoAdjustTilingX = true;

	[Tooltip("If true, automatically offset's y the tiling based on y scale.")]
	[SerializeField]
	private bool autoAdjustTilingY = true;

	[Tooltip("A factor to multiply the auto scaling by (Default: 1, 1).")]
	[SerializeField]
	private Vector2 autoTilingScaleFactor = Vector2.one;

	[Space(10)]

	[Header("Manual Settings")]

	[Tooltip("Adjusts the texture tiling on top of the autmatic behaviour, if any. Use this " +
		"instead of manually chaning the tiling or offset in the material.")]
	[SerializeField]
	private Vector2 manualTiling = Vector2.one;

	[Tooltip("Adjusts the texture offset on top of the autmatic behaviour, if any. Use this " +
		"instead of manually chaning the tiling or offset in the material.")]
	[SerializeField]
	private Vector2 manualOffset = Vector2.zero;

	#endregion

	#region Run-Time Fields

	private Material activeMaterial;
	private MeshRenderer refRend;
	private Vector2 autoTiling;

	#endregion

	#region Monobehaviour

	private void Start()
	{
		if (!CheckRenderer())
		{
			return;
		}

		AutoAdjust();
		ApplyAdjustment();

		if (Application.isPlaying)
		{ 
			DestroyImmediate(this);
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			return;
		}

		if (!CheckRenderer())
		{
			return;
		}

		CheckMaterial();
		AutoAdjust();
		ApplyAdjustment();
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Checks if a renderer is referenced and exists.
	/// </summary>
	private bool CheckRenderer()
	{
		if (refRend != null)
		{
			return true;
		}

		refRend = GetComponent<MeshRenderer>();

		if (refRend != null)
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// Checks if an active material is referenced, and if not, resets it to set a new one.
	/// </summary>
	private void CheckMaterial()
	{
		if (activeMaterial == null)
		{
			RefreshMaterial();
		}
	}

	/// <summary>
	/// Calculates the auto tiling adjustment based on the scale and sacle factors, if desired.
	/// </summary>
	private void AutoAdjust()
	{
		autoTiling = new Vector2
		(
			autoAdjustTilingX ? (transform.lossyScale.x * (1f / autoTilingScaleFactor.x)) - 1f : 0f,
			autoAdjustTilingY ? (transform.lossyScale.y * (1f / autoTilingScaleFactor.y)) - 1f : 0f
		);
	}

	/// <summary>
	/// Applies the texture adjustment including manual adjustments.
	/// </summary>
	private void ApplyAdjustment()
	{
		if (activeMaterial != null)
		{ 
			activeMaterial.SetTextureScale("_MainTex", autoTiling + manualTiling);
			activeMaterial.SetTextureOffset("_MainTex", manualOffset);
		}
		else
		{
			CheckMaterial();
		}
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Resets the material instance on this object to an instance of the current shared material.
	/// Called by the editor script button in editor mode to reset the material after changing it.
	/// </summary>
	public void RefreshMaterial()
	{
		if (refRend.sharedMaterial == null)
		{
			refRend.material = defaultMaterial;
		}

		activeMaterial = new Material(refRend.sharedMaterial);
		refRend.material = activeMaterial;
	}

	/// <summary>
	/// Resets all the inspector values to the default.
	/// </summary>
	public void ResetFields()
	{
		autoTilingScaleFactor = Vector2.one;
		manualTiling = Vector2.one;
		manualOffset = Vector2.zero;
	}

	#endregion
}