using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour 
{
	[SerializeField]
	private bool goingIn = true;

	[SerializeField]
	private bool wait = false;

	[SerializeField]
	private string nextScene;

	[SerializeField]
	private RawImage[] bars;

	[SerializeField]
	private float transitionBarTime;
	
	[SerializeField]
	private float barOffsetTime;

	[SerializeField]
	private float barPositionOffset;

	[SerializeField]
	private AnimationCurve barMoveCurve;

	private Vector3[] barP;
	private float[] barT;
	private bool[] barB;

	private void Start()
	{
		barT = new float[bars.Length];
		barB = new bool[bars.Length];
		barP = new Vector3[bars.Length];

		for (int i=0; i<bars.Length; i++)
		{
			barT[i] = 0f;
			barB[i] = false;
			barP[i] = bars[i].transform.position;

			if (goingIn)
			{
				bars[i].enabled = false;
			}
			else
			{
				bars[i].enabled = true;
			}
		}
	}

	private void Update()
	{
		if (wait)
		{
			return;
		}

		for (int i = 0; i < bars.Length; i++)
		{
			if (barB[i])
			{
				bars[i].enabled = true;
				barT[i] += Time.deltaTime;

				// Position
				if (goingIn)
				{ 
					bars[i].transform.position = Vector3.Lerp
					(
						barP[i] + (Vector3.right * (Screen.width * barPositionOffset)),
						barP[i],
						barMoveCurve.Evaluate(barT[i])
					);
				}
				else
				{
					bars[i].transform.position = Vector3.Lerp
					(
						barP[i],
						barP[i] + (Vector3.right * (Screen.width * -barPositionOffset)),
						barMoveCurve.Evaluate(barT[i])
					);
				}
			}
			else
			{
				barT[i] += Time.deltaTime;
				if (barT[i] >= barOffsetTime * i)
				{
					barB[i] = true;
					barT[i] = 0f;
				}

				if (goingIn)
				{
					bars[i].transform.position = barP[i] + (Vector3.right * (Screen.width * barPositionOffset));
				}
				else
				{
					bars[i].transform.position = barP[i];
				}
			}
		}

		if (barB[bars.Length-1] && barT[bars.Length-1] >= 1f && nextScene != "")
		{
			SceneManager.LoadScene(nextScene);
		}
	}

	public void Reset(bool goingIn, string nextScene="")
	{
		this.goingIn = goingIn;
		this.nextScene = nextScene;

		barT = new float[bars.Length];
		barB = new bool[bars.Length];

		for (int i = 0; i < bars.Length; i++)
		{
			barT[i] = 0f;
			barB[i] = false;

			if (goingIn)
			{
				bars[i].enabled = false;
			}
		}
	}

	public void Activate()
	{
		wait = false;
	}
}
