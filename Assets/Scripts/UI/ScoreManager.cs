
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Score Manager                                                  v0.1_2018.08.18
// 
//  AUTHOR:  Angel Rodriguez Jr.
//  CONTACT: angel.rodriguez.gamedev@gmail.com
//
//  Copyright (C) 2018, Angel Rodriguez Jr. All Rights Reserved.
//-------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour 
{
	public static ScoreManager main;
	
	public int playerCount = 6;
	public float winScore = 300f;

	[Space(10)]

	public float scoreRouletteTime = 0.3f;

	[SerializeField]
	private GameObject[] scoreIcons;

	[SerializeField]
	private SpriteRenderer[] borders;

	[SerializeField]
	private ScoreIconRoulette[] scoreRoulettes;

	[SerializeField]
	private float startIconX;
	[SerializeField]
	private float endIconX;

	[Space(10)]
	[SerializeField]
	private float borderFadeTime;
	[SerializeField]
	private AnimationCurve borderFadeCurve;

	private float scoreRouletteTimer;

	[HideInInspector]
	public float[] playerScores;

	private Color[] colors;
	private float[] borderTimers;

	private void Awake()
	{
		main = this;

		playerScores = new float[playerCount];
		borderTimers = new float[playerCount];
	}

	private void Update()
	{
		UpdateScores();
		UpdateHUD();
		CheckWin();

		UpdateBorderTimers();
		UpdateBorders();
	}

	public void ReportColors(Color[] colors)
	{
		this.colors = colors;
	}

	private void UpdateScores()
	{
		foreach (PlayerController player in GameController.main.players)
		{
			for (int i=0; i<colors.Length; i++)
			{
				if (player.shirtColor == colors[i])
				{
					playerScores[i] += Time.deltaTime;
				}
			}
		}
	}

	private void UpdateHUD()
	{
		for (int i=0; i<6; i++)
		{
			scoreIcons[i].transform.position = new Vector3
			(
				Mathf.Lerp(startIconX, endIconX, Mathf.Clamp01(playerScores[i] / winScore)),
				scoreIcons[i].transform.position.y,
				scoreIcons[i].transform.position.z
			);
		}

		scoreRouletteTimer += Time.deltaTime;
		if (scoreRouletteTimer >= scoreRouletteTime)
		{
			scoreRouletteTimer -= scoreRouletteTime;
			foreach (ScoreIconRoulette scoreRoulette in scoreRoulettes)
			{
				scoreRoulette.Increment();
			}
		}
	}

	private void CheckWin()
	{
		for (int i=0; i<playerCount; i++)
		{
			if (playerScores[i] >= winScore)
			{
				DeclareWinner(i);
				return;
			}
		}
	}

	private void DeclareWinner(int playerId)
	{
		Debug.Log("Player " + (playerId + 1) + " wins!");
		UnityEngine.SceneManagement.SceneManager.LoadScene("EndScreen" + (playerId + 1));
	}

	private void UpdateBorderTimers()
	{
		for (int i=0; i<6; i++)
		{
			borderTimers[i] -= Time.deltaTime;
			if (borderTimers[i] < 0f)
			{
				borderTimers[i] = 0f;
			}
		}
	}

	private void UpdateBorders()
	{
		for (int i=0; i<6; i++)
		{
			borders[i].color = new Color
			(
				borders[i].color.r, 
				borders[i].color.g, 
				borders[i].color.b, 
				borderFadeCurve.Evaluate(borderTimers[i] / borderFadeTime)
			);
		}
	}

	public void SetBorderColor(int index, Color color)
	{
		borders[index].color = color;
		borderTimers[index] = borderFadeTime;
	}
}
