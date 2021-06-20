using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWiggle : MonoBehaviour
{ 
	public float wiggleFrequency;
	public float wiggleAmplitude;

	private float wiggleTimer;

	private void Update()
	{
		wiggleTimer += Time.deltaTime;

		transform.eulerAngles = new Vector3
		(
			0f,
			0f,
			Mathf.Sin(wiggleTimer / wiggleFrequency) * wiggleAmplitude
		);
	}
}
