
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Game Controller                                                v0.0_2018.06.20
// 
//  AUTHOR:  Angel Rodriguez Jr.
//  CONTACT: angel.rodriguez.gamedev@gmail.com
//
//  Copyright (C) 2018, Angel Rodriguez Jr. All Rights Reserved.
//-------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the initial scene setup and game state management (win, etc.).
/// </summary>
public class GameController : MonoBehaviour 
{
	public static GameController main;

	#region Inspector Fields

	[Header("Dependancies")]

	[Tooltip("A reference to the player controller prefab, to spawn.")]
	[SerializeField]
	private GameObject refPlayerPrefab;
	[SerializeField]
	private GameObject refPlayerAltPrefab;

	#endregion

	#region Run-Time Fields

	[HideInInspector]
	public int[] playerJoysticks;

	[HideInInspector]
	public List<PlayerController> players;
	private List<PlayerController> PC;
	private List<Actor> NPC;

	#endregion

	#region Monobehaviour

	private void Awake()
	{
		if (main != null)
		{
			Destroy(gameObject);
			Debug.Log("!!");
			return;
		}

		main = this;
		DontDestroyOnLoad(this);
	}

	private void Start()
	{
		playerJoysticks = new int[6];

		//PopulateSpawnpoints();
		//SpawnSinglePlayer();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
		}
	}

	#endregion

	#region Public Methods

	/* DEPRECATED
	/// <summary>
	/// Finds all player spawnpoints in the scene and loads them into a list.
	/// </summary>
	private void PopulateSpawnpoints()
	{
		GameObject[] spawnpointObjects = GameObject.FindGameObjectsWithTag("Spawnpoint Player");

		spawnpoints = new List<Spawnpoint>();
		foreach (GameObject spawnpoint in spawnpointObjects)
		{
			spawnpoints.Add(spawnpoint.GetComponent<Spawnpoint>());
		}
	}

	/// <summary>
	/// Spawns a single player at the first spawnpoint found. Likely to be replaced later.
	/// </summary>
	private void SpawnSinglePlayer()
	{
		Instantiate(refPlayerPrefab, spawnpoints[0].GetSpawnPos(), spawnpoints[0].transform.rotation);
	}
	*/

	public void SaveConfig(int[] playerJoysticks)
	{
		this.playerJoysticks = playerJoysticks;
	}

	public void LoadConfig(LevelLoader levelLoader)
	{
		players = new List<PlayerController>();

		// Actually this is the spawner.
		List<Spawnpoint> spawnpoints = new List<Spawnpoint>(GameObject.FindObjectsOfType<Spawnpoint>());

		for (int i=0; i<spawnpoints.Count; i++)
		{
			if (spawnpoints[i].type == Spawnpoint.Type.AMMO)
			{
				spawnpoints.RemoveAt(i);
				i--;
			}

			else
			{
				GameObject playerToSpawn = null;
				if (Random.Range(0,2) == 0)
				{
					playerToSpawn = refPlayerPrefab;
				}
				else
				{
					playerToSpawn = refPlayerAltPrefab;
				}

				players.Add(Instantiate(playerToSpawn).GetComponent<PlayerController>());
				players[players.Count - 1].transform.position = spawnpoints[i].GetSpawnPos();
				players[players.Count - 1].transform.rotation = spawnpoints[i].transform.rotation;
				players[players.Count - 1].Setup
				(
					levelLoader.refReticle[i],
					levelLoader.ammoIcon[i],
					levelLoader.noAmmoIcon[i],
					levelLoader.ammoText[i],
					levelLoader.popUp[i],
					playerJoysticks[i],
					levelLoader.renderTex[i],
					i,
					levelLoader.cullingMasks[i],
					levelLoader.colors[i],
					players.Count-1
				);
			}
		}

		// Populate colors.
		ScoreManager.main.ReportColors(levelLoader.colors);

		StartCoroutine(Countdown());
	}

	private void TurnOnHUDs()
	{
		foreach (PlayerController player in players)
		{
			player.TurnOnHUD();
		}
	}

	private IEnumerator Countdown()
	{
		yield return new WaitForSeconds(2f);

		TurnOnHUDs();
	}

	#endregion
}
