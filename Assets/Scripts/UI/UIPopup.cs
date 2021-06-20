using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopup : MonoBehaviour 
{
	[SerializeField]
	private GameObject plusOnePrefab;

	[SerializeField]
	private Vector3 popUpMaxScale;

	[SerializeField]
	private float popUpTime;

	[SerializeField]
	private Vector3 plusOneOffset;

	[SerializeField]
	private AnimationCurve popUpCurve;

	private Vector3 savedStartScale;
	private float popUpTimer;

	private void Start()
	{
		savedStartScale = transform.localScale;
	}

	private void Update()
	{
		popUpTimer -= Time.deltaTime;
		transform.localScale = Vector3.Lerp
		(
			savedStartScale, 
			popUpMaxScale, 
			popUpCurve.Evaluate(popUpTimer / popUpTime)
		);
	}

	public void PopUp()
	{
		GameObject plusOne = Instantiate(plusOnePrefab, transform.parent);
		plusOne.transform.position = transform.position + plusOneOffset;
		popUpTimer = popUpTime;
	}
}
