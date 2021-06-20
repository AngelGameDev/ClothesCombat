using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowTransition : MonoBehaviour 
{
	[SerializeField]
	private float growTime;

	[SerializeField]
	private AnimationCurve growCurve;

	private Vector3 endScale;

	private void Start()
	{
		endScale = transform.localScale;

		StartCoroutine(GrowRoutine());
	}

	private IEnumerator GrowRoutine()
	{
		float startTime = Time.time;
		while (Time.time - startTime < growTime)
		{
			float t = growCurve.Evaluate((Time.time - startTime) / growTime);

			transform.localScale = Vector3.Lerp(Vector3.zero, endScale, t);

			yield return 1;
		}

		Destroy(this);
	}
}
