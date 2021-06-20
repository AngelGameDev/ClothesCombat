using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponWobble : MonoBehaviour 
{
	public float amplitudeX;
	public float amplitudeY;
	public float frequency;

	private float timeX;
	private float timeY;
	private float startX = 0f;
	private float startY = 0f;
	private float offsetY = 0f;
	private float offsetX = 0f;

	float wobbleFactor = 0f;

	private void Start()
	{
		startX = transform.position.x;
		startY = transform.position.y;
	}

	private void Update()
	{
		if (frequency > 0f)
		{
			timeX += Time.deltaTime / frequency;
			timeY += Time.deltaTime / (frequency * 0.5f);
		}

		UpdatePosition();
	}

	private void UpdatePosition()
	{
		offsetY = Mathf.Sin(timeY) * (amplitudeY * wobbleFactor);
		offsetX = Mathf.Sin(timeX) * (amplitudeX * wobbleFactor);

		transform.position = new Vector3
		(
			startX + offsetX,
			startY + offsetY,
			transform.position.z
		);
	}

	public void Reset()
	{
		timeX = 0f;
		timeY = 0f;
	}

	public void SetWobbleFactor(float wobbleFactor)
	{
		this.wobbleFactor = wobbleFactor;
	}
}
