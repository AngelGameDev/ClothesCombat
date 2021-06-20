using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFloatUp : MonoBehaviour 
{
	[SerializeField]
	private float floatUpHeight;

	[SerializeField]
	private float floatUpTime;

	[SerializeField]
	private AnimationCurve floatUpCurve;

	[SerializeField]
	private AnimationCurve fadeOutCurve;

	private Text refText;
	private Vector3 savedStartPos;
	private float floatUpTimer;

	private void Start()
	{
		savedStartPos = transform.position;
		refText = GetComponent<Text>();
	}

	private void Update()
	{
		floatUpTimer += Time.deltaTime;

		float t = floatUpCurve.Evaluate(floatUpTimer / floatUpTime);

		transform.position = Vector3.Lerp
		(
			savedStartPos,
			savedStartPos + Vector3.up * floatUpHeight,
			t
		);

		refText.color = new Color
		(
			refText.color.r,
			refText.color.g,
			refText.color.b, 
			Mathf.Lerp(1f, 0f, t)
		);

		if (t >= 1f)
		{
			Destroy(gameObject);
		}
	}
}
