using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreenManager : MonoBehaviour 
{
	public TransitionManager transitionManager;

	private bool isEnding = false;

	private void Update()
	{
		if (isEnding)
		{
			return;
		}

		if (Input.GetKeyDown("joystick button 7"))
		{
			isEnding = true;
			StartCoroutine(MainMenuRoutine());
		}
	}

	private IEnumerator MainMenuRoutine()
	{
		transitionManager.Activate();
		yield return new WaitForSeconds(3f);
		UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
	}
}
