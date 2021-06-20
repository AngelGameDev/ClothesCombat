using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreIconRoulette : MonoBehaviour 
{
	public int currentIndex;
	
	public void Increment()
	{
		currentIndex++;
		if (currentIndex >= 6)
		{
			currentIndex = 0;
		}

		transform.localPosition = new Vector3
		(
			transform.localPosition.x,
			transform.localPosition.y,
			-1 * (currentIndex + 1)
		);
	}
}
