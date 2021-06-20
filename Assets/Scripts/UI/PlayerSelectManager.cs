using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectManager : MonoBehaviour 
{
	[SerializeField]
	private TransitionManager transManager;

	[SerializeField]
	private Image[] images;

	private UIWiggle[] wiggles;

	[SerializeField]
	private RawImage[] fills;

	[SerializeField]
	private Sprite spriteNotReady;

	[SerializeField]
	private Sprite spriteReady;

	[Space(10)]

	[Header("Debug")]
	
	public bool[] lockedStatus;
	public int[] playerIndex;
	public int currentIndex = 0;

	private bool isReady = false;

	private void Start()
	{
		lockedStatus = new bool[images.Length];
		playerIndex = new int[images.Length];
		wiggles = new UIWiggle[images.Length];

		for (int i=0; i<images.Length; i++)
		{
			lockedStatus[i] = false;
			playerIndex[i] = -1;
			wiggles[i] = images[i].GetComponentInParent<UIWiggle>();
		}
	}

	private void Update()
	{
		if (isReady)
		{
			return;
		}
		GetInput();
		UpdateVisuals();
		CheckContinue();

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	private void GetInput()
	{
		currentIndex = 0;
		for (int i = 0; i < images.Length; i++)
		{
			if (!lockedStatus[i])
			{
				currentIndex = i;
				i = images.Length;
				break; // In case?
			}
		}

		for (int i = 0; i < images.Length; i++)
		{
			bool skipJoystick = false;

			for (int j = 0; j < images.Length; j++)
			{
				if (playerIndex[j] == i)
				{
					skipJoystick = true;
					break;
				}
			}

			if (!skipJoystick)
			{
				if (Input.GetKeyDown("joystick " + (i + 1) + " button 0"))
				{
					playerIndex[currentIndex] = i;
					lockedStatus[currentIndex] = true;
				}
			}
		}

		// Back button
		for (int i=0; i<images.Length; i++)
		{
			if (Input.GetKeyDown("joystick " + (i + 1) + " button 1"))
			{
				// Search if indexed
				for (int j=0; j<images.Length; j++)
				{
					if (playerIndex[j] == (i))
					{
						// Reset i
						playerIndex[j] = -1;
						lockedStatus[j] = false;
					}
				}
			}
		}
	}

	private void UpdateVisuals()
	{
		for (int i=0; i<images.Length; i++)
		{
			if (lockedStatus[i])
			{
				images[i].sprite = spriteReady;
				wiggles[i].wiggleAmplitude = 0f;
				fills[i].enabled = true;
			}
			else
			{
				images[i].sprite = spriteNotReady;
				wiggles[i].wiggleAmplitude = 4f;
				fills[i].enabled = false;
			}
		}
	}

	private void CheckContinue()
	{
		int readyCount = 0;

		foreach (bool status in lockedStatus)
		{
			if (status)
			{
				readyCount++;
			}
		}

		if (readyCount >= 6)
		{
			GameController.main.SaveConfig(playerIndex);
			isReady = true;
			transManager.Reset(true, "Level");
		}
	}
}
