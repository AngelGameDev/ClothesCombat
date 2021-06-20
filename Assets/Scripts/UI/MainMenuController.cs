using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour 
{
	[SerializeField]
	private EventSystem eventSystem;
	[SerializeField]
	private GameObject refMainMenu;
	[SerializeField]
	private GameObject refStartPrompt;
	[SerializeField]
	private GameObject refCreditsShirt;
	[SerializeField]
	private GameObject refHowToShirt;
	[SerializeField]
	private Button buttonPlay;
	[SerializeField]
	private Button buttonHowTo;
	[SerializeField]
	private Button buttonCredits;
	[SerializeField]
	private TransitionManager transManager;

	[Space(10)]

	[SerializeField]
	private string nextScene;

	[Space(10)]

	[SerializeField]
	private float transitionTimeIn;
	[SerializeField]
	private float transitionTimeOut;

	[Space(10)]

	[SerializeField]
	private Vector3 shirtPosOffset;
	[SerializeField]
	private float shirtTransitionTime;
	[SerializeField]
	private AnimationCurve shirtCurve;

	[Space(10)]

	[SerializeField]
	private Vector3 menuPosOffset;
	[SerializeField]
	private float menuTransitionTime;
	[SerializeField]
	private AnimationCurve menuCurve;

	private Vector3 savedMenuStartPos;
	private Vector3 savedCreditsStartPos;
	private Vector3 savedHowtoStartPos;
	private bool hasStarted = false;
	public bool isShowingCredits = false;
	public bool isShowingHowTo = false;
	private float creditsTimer = 0f;
	private float howToTimer = 0f;

	private void Start()
	{
		refMainMenu.SetActive(false);
		savedMenuStartPos = refMainMenu.transform.position;
		savedCreditsStartPos = refCreditsShirt.transform.position;
		savedHowtoStartPos = refHowToShirt.transform.position;
	}

	private void Update()
	{
		UpdateSubmenus();

		if (!hasStarted)
		{ 
			if (Input.GetButtonDown("Submit All"))
			{
				hasStarted = true;
				refStartPrompt.SetActive(false);
				refMainMenu.SetActive(true);
				buttonPlay.Select();
			}
		}
		else
		{
			if (!isShowingCredits && !isShowingHowTo)
			{
				if (Input.GetButtonDown("Cancel All"))
				{
					hasStarted = false;
					refStartPrompt.SetActive(true);
					refMainMenu.SetActive(false);
					eventSystem.SetSelectedGameObject(null);
				}
			}
			else if (isShowingCredits)
			{
				if (Input.GetButtonDown("Cancel All"))
				{
					isShowingCredits = false;
					buttonCredits.Select();
				}
			}
			else if (isShowingHowTo)
			{
				if (Input.GetButtonDown("Cancel All"))
				{
					isShowingHowTo = false;
					buttonHowTo.Select();
				}
			}
		}
	}

	private void UpdateSubmenus()
	{
		float t_mm = 0f;
		float t_c = 0f;
		float t_ht = 0f;

		if (!isShowingCredits && !isShowingHowTo)
		{
			creditsTimer -= Time.deltaTime;
			if (creditsTimer < 0f)
			{
				creditsTimer = 0f;
			}
			howToTimer -= Time.deltaTime;
			if (howToTimer < 0f)
			{
				howToTimer = 0f;
			}

			t_c = shirtCurve.Evaluate(creditsTimer / shirtTransitionTime);
			t_ht = shirtCurve.Evaluate(howToTimer / shirtTransitionTime);

			if (creditsTimer > howToTimer)
				t_mm = t_c;
			else
				t_mm = t_ht;
		}
		else if (isShowingCredits)
		{
			creditsTimer += Time.deltaTime;
			if (creditsTimer > shirtTransitionTime)
			{
				creditsTimer = shirtTransitionTime;
			}
			howToTimer -= Time.deltaTime;
			if (howToTimer < 0f)
			{
				howToTimer = 0f;
			}

			t_c = shirtCurve.Evaluate(creditsTimer / shirtTransitionTime);
			t_ht = shirtCurve.Evaluate(howToTimer / shirtTransitionTime);

			if (creditsTimer > howToTimer)
				t_mm = t_c;
			else
				t_mm = t_ht;
		}
		else if (isShowingHowTo)
		{
			creditsTimer -= Time.deltaTime;
			if (creditsTimer < 0f)
			{
				creditsTimer = 0f;
			}
			howToTimer += Time.deltaTime;
			if (howToTimer > shirtTransitionTime)
			{
				howToTimer = shirtTransitionTime;
			}

			t_c = shirtCurve.Evaluate(creditsTimer / shirtTransitionTime);
			t_ht = shirtCurve.Evaluate(howToTimer / shirtTransitionTime);

			if (creditsTimer > howToTimer)
				t_mm = t_c;
			else
				t_mm = t_ht;
		}

		refCreditsShirt.transform.position = Vector3.Lerp
		(
			savedCreditsStartPos,
			savedCreditsStartPos + shirtPosOffset * Screen.width,
			t_c
		);
		refHowToShirt.transform.position = Vector3.Lerp
		(
			savedHowtoStartPos,
			savedHowtoStartPos + shirtPosOffset * Screen.width,
			t_ht
		);
		refMainMenu.transform.position = Vector3.Lerp
		(
			savedMenuStartPos,
			savedMenuStartPos + menuPosOffset * Screen.width,
			t_mm
		);
	}

	public void SelectCredits()
	{
		isShowingCredits = true;
		eventSystem.SetSelectedGameObject(null);
	}

	public void SelectHowTo()
	{
		isShowingHowTo = true;
		eventSystem.SetSelectedGameObject(null);
	}

	public void SelectQuit()
	{
		Debug.Log("Game Quit.");
		Application.Quit();
	}

	public void SelectConfirm()
	{
		transManager.Reset(true, nextScene);
	}
}
