
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Reticle (Controller)                                           v0.0_2018.07.07
// 
//  AUTHOR:  Angel Rodriguez Jr.
//  CONTACT: angel.rodriguez.gamedev@gmail.com
//
//  Copyright (C) 2018, Angel Rodriguez Jr. All Rights Reserved.
//-------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows outside influence of the reticle sprite state.
/// </summary>
public class Reticle : MonoBehaviour 
{
	public Sprite[] chargeSprites;

	[HideInInspector]
	public int chargeLevel;

	private Image refRend;

	private void Start()
	{
		refRend = GetComponent<Image>();
		ChangeChargeLevel(0);
	}

	public void ChangeChargeLevel(int chargeLevel)
	{
		if (chargeLevel < 0 || chargeLevel >= chargeSprites.Length)
		{
			Debug.LogError("RETICLE ERROR: Tried to charge to a sprite that doesn't exist.");
		}

		this.chargeLevel = chargeLevel;
		refRend.sprite = chargeSprites[chargeLevel];
	}

	public void ChangeColor(Color targetColor)
	{
		if (refRend == null)
		{
			refRend = GetComponent<Image>();
		}
		refRend.color = targetColor;
	}
}
