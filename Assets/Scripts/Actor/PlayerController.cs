
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Player Controller                                              v0.3_2018.07.06
// 
//  AUTHOR:  Angel Rodriguez Jr.
//  CONTACT: angel.rodriguez.gamedev@gmail.com
//
//  Copyright (C) 2018, Angel Rodriguez Jr. All Rights Reserved.
//-------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

[RequireComponent(typeof(Rigidbody))]

/// <summary>
/// Controls a Player actor; singleplayer. Takes player input, moves, and performs actions. 
/// </summary>
public class PlayerController : Actor 
{
	#region Inspector Fields

	[Header("Dependancies")]

	[Tooltip("A reference to the projectile prefab for the shirt projectile that the player " +
		"fires when releasing a charge.")]
	[SerializeField]
	private GameObject projectilePrefab;

	public Material shirtStandingMat;

	public Material shirtRunningMat;

	[SerializeField]
	private MeshRenderer refMesh;

	[SerializeField]
	private Camera refCamera;

	[SerializeField]
	private Reticle refReticle;

	[SerializeField]
	private RawImage ammoIcon;

	[SerializeField]
	private RawImage noAmmoIcon;

	[SerializeField]
	private Text ammoText;

	[SerializeField]
	private UIPopup popUp;

	[Space(10)]

	[Header("Offense")]

	public Color colorMain;

	public int startingAmmo;

	[SerializeField]
	private float screenShakeAmplitude;

	[SerializeField]
	private float screenShakeFrequency;

	[SerializeField]
	private float controllerChargeVibration;

	[SerializeField]
	private float controllerFinalVibrationTime;

	[SerializeField]
	private float launchAngleMin;

	[SerializeField]
	private float launchAngleMax;

	[SerializeField]
	private float launchPowerMin;

	[SerializeField]
	private float launchPowerMax;

	[SerializeField]
	private float cooldownTime;

	[SerializeField]
	private float maxChargeTime;

	[SerializeField]
	private AnimationCurve chargeCurve;

	[Space(10)]

	[Header("Move")]

	[Tooltip("The maximum forward run speed. Other speeds are ratios of this speed.")]
	[SerializeField]
	private float baseMoveSpeed = 13f;

	[Tooltip("The percentage of the Base Move Speed that the player moves when strafing left " +
		"or right. Strafe ratio is further partitioned by angle when using analog.")]
	[SerializeField]
	[Range(0f, 1f)]
	private float strafeSpeedRatio = 0.85f;

	[Tooltip("The percentage of the Base Move Speed that the player moves when moving " +
		"backwards. Strafe ratio is further partitioned by angle when using analog.")]
	[SerializeField]
	[Range(0f, 1f)]
	private float backSpeedRatio = 0.6f;

	[Tooltip("The force the player jumps straight up with when a jump is executed.")]
	[SerializeField]
	private float jumpForce = 11f;

	[Tooltip("The time in seconds that the ground trigger collider is disabled after jumping.")]
	[SerializeField]
	private float jumpGroundedDisableTime = 0.1f;

	[Tooltip("The distance from the player transform center to the ground, which is used to " +
		"check if the player is on the ground.")]
	[SerializeField]
	private float groundDistance = 1.1f;

	[Tooltip("The maximum slope angle before the player slides down.")]
	[SerializeField]
	private float maxSlopeAngle = 70f;

	[Tooltip("The layers that teh ground raycast will collide with, consider ground, and freeze " +
		"the player's gravity when on top of.")]
	[SerializeField]
	private LayerMask groundLayerMask;

	[Space(10)]

	[Tooltip("The velocity magnitude below which the actor is considered still.")]
	[SerializeField]
	private float animationStandingVelocityCutoff = 0.1f;

	[Space(10)]

	[Header("Aim")]
	[Tooltip("The base turn speed when using a controller analog stick to aim. Horizontal turn " +
		"speed is exactly this, and vertical is a ratio of it.")]
	[SerializeField]
	private float baseAimSpeedAnalog = 7f;

	[SerializeField]
	[Tooltip("The base turn speed when using keyboard buttons to aim. Horizontal turn " +
		"speed is exactly this, and vertical is a ratio of it.")]
	private float baseAimSpeedKeyboard = 7f;

	[SerializeField]
	[Tooltip("The base turn speed when using the mouse to aim. Horizontal turn " +
		"speed is exactly this, and vertical is a ratio of it.")]
	private float baseAimSpeedMouse = 5f;

	[Tooltip("The percentage of the appropriate aim speed that the player rotates vertically.")]
	[SerializeField]
	[Range(0f,1f)]
	private float aimVerticalRatio = 0.5f;

	[Space(10)]

	[Tooltip("If true the player can use the mouse to aim.")]
	[SerializeField]
	private bool enableMouseAim = true;
	
	[Tooltip("If true the player can use keyboard input to aim.")]
	[SerializeField]
	private bool enableKeyboardAim = true;
	
	[Tooltip("if true the player can use a controller analog to aim.")]
	[SerializeField]
	private bool enableAnalogAim = true;

	[Space(10)]

	[Tooltip("The height below which the player will automatically respawn.")]
	[SerializeField]
	private float respawnHeight = -10f;

	[Space(10)]

	[Header("Gizmo Settings")]

	[SerializeField]
	private bool showGroundDistance;

	[SerializeField]
	private Color gizmoColorGroundDistance;

	[Space(10)]

	[Header("Audio")]

	[SerializeField]
	public AudioSource AudioMain;

	[SerializeField]
	public AudioSource AudioSub;

	[SerializeField]
	public AudioSource AudioExplode;

	[Space(5)]

	[SerializeField]
	private AudioClip sfxChargeUp;

	[SerializeField]
	private AudioClip sfxChargeHold;

	[SerializeField]
	private AudioClip sfxExplodeMain;

	[SerializeField]
	private AudioClip sfxExplodeFull;

	[SerializeField]
	private AudioClip sfxHitPlayer;

	[SerializeField]
	private AudioClip sfxHitWall;

	[SerializeField]
	private AudioClip sfxCollect;

	#endregion

	#region Run-Time Fields

	[HideInInspector]
	public bool isFiring;

	[HideInInspector]
	public int targetJoystick = -1;

	[HideInInspector]
	public Color shirtColor;

	[HideInInspector]
	public int ammo;

	private Rigidbody body;
	private Camera playerCam;
	private Vector3 activeVelocity;
	private Vector3 groundNormal;
	private Vector3 savedStartSpawnPos;
	private Vector3 savedCameraLocalPos;
	private Vector2 moveInput;
	private Vector2 aimInput;      // Modified by base aim speed.
	private float screenShakeTimer;
	private float animSpeed;
	private float groundAngle;
	private float activeSpeed;
	private float jumpGroundedDisableTimer;
	private float chargeTimer;
	private float lastShakeTimer;
	private float cooldownTimer;
	private int playerID;
	private int debugChannel = -1;
	private int debugCount;
	private int chargeShakeLevel = 0;
	private bool isMoving;
	private bool isOnGround;	
	private bool isBottomTriggered;
	private bool isSliding;
	private bool isLastShaking;
	private bool isCoolingDown;
	private bool isChargeHoldingSound;

	#endregion

	#region Monobehaviours

	private void Start()
	{
		body = GetComponent<Rigidbody>();
		playerCam = GetComponentInChildren<Camera>();

		refRend.material = standingMat;
		savedStartSpawnPos = transform.position;
		savedCameraLocalPos = refCamera.transform.localPosition;

		shirtMesh.material.SetColor("_Tint", colorMain);

		if (lastShakeTimer <= 0f)
		{
			/*
			if (targetJoystick == 1)
			{
				GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
			}
			else if (targetJoystick == 2)
			{
				GamePad.SetVibration(PlayerIndex.Two, 0f, 0f);
			}
			else if (targetJoystick == 3)
			{
				GamePad.SetVibration(PlayerIndex.Three, 0f, 0f);
			}
			else if (targetJoystick == 4)
			{
				GamePad.SetVibration(PlayerIndex.Four, 0f, 0f);
			}
			*/
		}
		isLastShaking = false;

		ammoIcon.color = colorMain;
		ammo = startingAmmo;

		refReticle.ChangeColor(colorMain);

		//targetJoystick = 1;
	}

	protected override void Update()
	{
		base.Update();

		GetInput();

		UpdateDebug();

		UpdateTimers();

		UpdateCharge();

		ChargeShake();

		CheckRespawnHeight();

		UpdateUI();

		shirtMesh.material.SetColor("_ColorTint", shirtColor);
	}

	private void FixedUpdate()
	{
		Move();

		UpdateAnimation();
	}

	private void OnTriggerStay(Collider other)
	{
		if (jumpGroundedDisableTimer <= 0)
		{
			isBottomTriggered = true;
		}

		// Resets to false every Update in Move().
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position + (Vector3.down * groundDistance));

		Gizmos.DrawLine(Vector3.right * 50f + Vector3.up * respawnHeight, Vector3.left * 50f + Vector3.up * respawnHeight);
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Gets input from the player.
	/// </summary>
	private void GetInput()
	{
		// Move
		moveInput = new Vector2
		(
			Input.GetAxis("Move Horizontal " + targetJoystick),
			Input.GetAxis("Move Vertical " + targetJoystick)
		);

		// Aim
		Vector2 aimInputMouse = enableMouseAim? new Vector2
		(
			Input.GetAxis("Aim Horizontal Mouse"),
			Input.GetAxis("Aim Vertical Mouse")
		) : Vector2.zero;
		Vector2 aimInputKeyboard = enableKeyboardAim ? new Vector2
		(
			Input.GetAxis("Aim Horizontal Keyboard"),
			Input.GetAxis("Aim Vertical Keyboard")
		) : Vector2.zero;
		Vector2 aimInputAnalog = enableAnalogAim ? new Vector2
		(
			Input.GetAxis("Aim Horizontal Analog " + targetJoystick),
			Input.GetAxis("Aim Vertical Analog " + targetJoystick)
		) : Vector2.zero;

		// Aim priority order: Mouse, Keyboard, Analog
		if (aimInputMouse.magnitude > 0)
		{
			aimInput = aimInputMouse * baseAimSpeedMouse;
		}
		else if (aimInputKeyboard.magnitude > 0)
		{
			aimInput = aimInputKeyboard * baseAimSpeedKeyboard;
		}
		else
		{
			aimInput = aimInputAnalog * baseAimSpeedAnalog;
		}

		aimInput.y *= aimVerticalRatio;

		// Jump
		if (Input.GetKeyDown("joystick " + targetJoystick + " button 0"))
		{
			Jump();
		}

		// Fire
		if (!isFiring && Input.GetKeyDown("joystick " + targetJoystick + " button 5"))
		{
			BeginFireCharge();
		}

		else if (isFiring && Input.GetKeyUp("joystick " + targetJoystick + " button 5"))
		{
			ReleaseFireCharge();
		}

		// Reset
		if (Input.GetKeyDown("joystick " + targetJoystick + " button 6"))
		{
			transform.position = savedStartSpawnPos;
		}
	}

	/// <summary>
	/// Uses input to calculate and apply movement for the player camera and rigidbody.
	/// </summary>
	private void Move()
	{
		// Rotate Player Horizontally
		transform.Rotate
		(
			new Vector3
			(
				0f,
				aimInput.x,
				0f
			)
		);

		// Rotate camera vertically
		playerCam.transform.localEulerAngles = new Vector3
		(
			playerCam.transform.localEulerAngles.x - aimInput.y,
			0f,
			0f
		);

		// Set move velocity based on input
		activeSpeed = (moveInput.y >= 0f ? baseMoveSpeed : baseMoveSpeed * backSpeedRatio);

		WeaponAnimation.main.UpdateWobble(playerID, Mathf.Abs(moveInput.y * (1f - Mathf.Clamp01(chargeTimer/maxChargeTime))));

		if (moveInput.magnitude > 0f)
		{
			activeSpeed = Mathf.Lerp
			(
				activeSpeed, 
				baseMoveSpeed * strafeSpeedRatio, 
				Mathf.Abs(moveInput.x)
			);
		}

		activeVelocity = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * new Vector3
		(
			moveInput.x * activeSpeed,
			body.velocity.y,
			moveInput.y * activeSpeed
		);

		CheckGround();

		CheckSlope();

		// Apply
		body.velocity = activeVelocity + propelVector;

		propelVector = Vector3.zero;

		// Resets the bottom trigger to make sure it's triggered next time, in OnTriggerStay.
		isBottomTriggered = false;
	}

	/// <summary>
	/// Jumps by giving a single force impulse upwards, at the magnitude specified by jumpForce.
	/// </summary>
	private void Jump()
	{
		if (!isOnGround)
		{
			return;
		}

		body.useGravity = true;

		body.velocity = new Vector3
		(
			body.velocity.x,
			jumpForce,
			body.velocity.z
		);

		isOnGround = false;
		isBottomTriggered = false;
		jumpGroundedDisableTimer = jumpGroundedDisableTime;
	}

	/// <summary>
	/// Begins a charge of the player's weapon.
	/// </summary>
	private void BeginFireCharge()
	{
		if (isFiring || isCoolingDown || ammo <= 0)
		{
			return;
		}

		//Debug.Log("Beginning charge...");
		isFiring = true;
		chargeTimer = 0f;

		AudioMain.Stop();
		AudioMain.loop = false;
		AudioMain.pitch = 1.1f;
		AudioMain.volume = 1f;
		AudioMain.clip = sfxChargeUp;
		AudioMain.Play();

		isChargeHoldingSound = false;
	}

	/// <summary>
	/// Releases a charge of the player's weapon and instanties a projectile with appropriate 
	/// fields based on the charge duration.
	/// </summary>
	private void ReleaseFireCharge()
	{
		if (!isFiring)
		{
			return;
		}

		AudioMain.Stop();
		AudioSub.Stop();

		Projectile projectile = Instantiate(projectilePrefab).GetComponent<Projectile>();
		projectile.transform.position = transform.position;

		float t = chargeCurve.Evaluate(Mathf.Clamp01(chargeTimer / maxChargeTime));

		// Launch angle
		Vector3 launchAngle = new Vector3
		(
			refCamera.transform.forward.x,
			refCamera.transform.forward.y + Mathf.Lerp
			(
				launchAngleMin, 
				launchAngleMax, 
				t
			),
			refCamera.transform.forward.z
		);

		if (t >= 1f)
		{
			AudioExplode.Stop();
			AudioExplode.loop = false;
			AudioExplode.pitch = 1f;
			AudioExplode.clip = sfxExplodeFull;
			AudioExplode.volume = 1f;
			AudioExplode.Play();
		}
		else
		{
			AudioExplode.Stop();
			AudioExplode.loop = false;
			AudioExplode.pitch = 1f;
			AudioExplode.clip = sfxExplodeMain;
			AudioExplode.volume = Mathf.Lerp(0.1f, 0.5f, t);
			AudioExplode.Play();
		}

		projectile.Setup
		(
			launchAngle.normalized * Mathf.Lerp(launchPowerMin, launchPowerMax, t), 
			colorMain,
			transform.eulerAngles.y,
			this
		);

		isLastShaking = true;
		lastShakeTimer = controllerFinalVibrationTime;
	
		/*
		if (targetJoystick == 1)
		{ 
			GamePad.SetVibration(PlayerIndex.Two, 1.0f, 1.0f);
		}
		else if (targetJoystick == 3)
		{
			GamePad.SetVibration(PlayerIndex.Four, 1.0f, 1.0f);
		}
		else if (targetJoystick == 4)
		{
			GamePad.SetVibration(PlayerIndex.Three, 1.0f, 1.0f);
		}
		else if (targetJoystick == 6)
		{
			GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
		}
		*/

		isCoolingDown = true;
		cooldownTimer = cooldownTime;
		
		isFiring = false;

		ammo--;

		isChargeHoldingSound = false;

		WeaponAnimation.main.Reload(playerID);
		chargeTimer = 0f;
	}

	/// <summary>
	/// Uses a raycast to check if on the ground then the hit normal angle to check if the player 
	/// is grounded.
	/// </summary>
	private void CheckGround()
	{
		if (!isBottomTriggered)
		{
			isOnGround = false;
			return;
		}

		RaycastHit hit;
		isOnGround = Physics.Raycast(transform.position, Vector3.down, out hit, groundDistance, groundLayerMask);
		groundNormal = hit.normal;
		groundAngle = Vector3.Angle(Vector3.up, hit.normal);
	}

	private void CheckSlope()
	{
		body.useGravity = !isOnGround || groundAngle > maxSlopeAngle;

		if (isOnGround)
		{
			float speed = activeVelocity.magnitude;
			Vector3 temp = Vector3.Cross(groundNormal, activeVelocity.normalized);
			activeVelocity = Vector3.Cross(temp, groundNormal) * speed;
		}
	}

	/// <summary>
	/// (Debug) Updates the debug readout for an actor. Likely to change based on dev needs.
	/// </summary>
	private void UpdateDebug()
	{
		if (debugChannel == -1)
		{
			if (DebugCore.main != null)
			{
				debugChannel = DebugCore.main.CreateNewChannel();
			}
			else
			{
				return;
			}
		}

		string moveText = moveInput.x.ToString("0.0").PadLeft(4, ' ') + ", " + 
			moveInput.y.ToString("0.0").PadLeft(4, ' ');
		string aimText = aimInput.x.ToString("0.0").PadLeft(4, ' ') + ", " + 
			aimInput.y.ToString("0.0").PadLeft(4, ' ');
		string positionText = transform.position.x.ToString("0.0").PadLeft(4, ' ') + ", " +
			transform.position.z.ToString("0.0").PadLeft(4, ' ');
		string propelText = propelVector.x.ToString("0.0").PadLeft(4, ' ') + ", " +
			propelVector.y.ToString("0.0").PadLeft(4, ' ') + ", " +
			propelVector.z.ToString("0.0").PadLeft(4, ' ');

		string debugText = 
			"Position:      " + positionText + "\n\n" +
			"Move Input:    " + moveText + "     Aim Input:     " + aimText + "\n\n" +
			"IsOnGround:    " + (isOnGround? "Yes" : "No") + "\n" +
			"Normal Angle:  " + groundAngle.ToString("F1") + "\n" +
			"Propel Vector: " + propelText;

		if (DebugCore.main != null)
		{
			DebugCore.main.SetConsoleChannelText
			(
				debugChannel,
				debugText
			);
		}
	}

	/// <summary>
	/// Sets the appropriate animation, and if appilcaple updates the speed field in the 
	/// material properites of the animated material shader.
	/// </summary>
	private void UpdateAnimation()
	{
		// Set animation state
		if (moveInput.magnitude >= animationStandingVelocityCutoff && !isMoving)
		{
			isMoving = true;
			refRend.material = runningMat;
			shirtMesh.material = shirtRunningMat;

			shirtMesh.material.SetColor("_Tint", colorMain);
		}
		else if (moveInput.magnitude < animationStandingVelocityCutoff && isMoving)
		{
			isMoving = false;
			refRend.material = standingMat;
			shirtMesh.material = shirtStandingMat;

			shirtMesh.material.SetColor("_Tint", colorMain);
		}
	}

	/// <summary>
	/// Updates all timers.
	/// </summary>
	private void UpdateTimers()
	{
		jumpGroundedDisableTimer -= Time.deltaTime;
		if (jumpGroundedDisableTimer < 0)
		{
			jumpGroundedDisableTimer = 0;
		}

		if (isFiring)
		{
			chargeTimer += Time.deltaTime;
		}

		if (isCoolingDown)
		{
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer <= 0f)
			{
				isCoolingDown = false;
				WeaponAnimation.main.UnReload(playerID);
			}
		}
	}

	/// <summary>
	/// Updates the charge visuals based on charge progress.
	/// </summary>
	private void UpdateCharge()
	{
		if (isFiring)
		{
			float t = chargeCurve.Evaluate(Mathf.Clamp01(chargeTimer / maxChargeTime));

			refReticle.ChangeChargeLevel
			(
				(int)Mathf.Lerp
				(
					0, 
					refReticle.chargeSprites.Length-2, 
					t
				) + 1
			);

			/*
			if (targetJoystick == 1)
			{
				GamePad.SetVibration(PlayerIndex.One, controllerChargeVibration * t, controllerChargeVibration * t);
			}
			else if (targetJoystick == 2)
			{
				GamePad.SetVibration(PlayerIndex.Two, controllerChargeVibration * t, controllerChargeVibration * t);
			}
			else if (targetJoystick == 3)
			{
				GamePad.SetVibration(PlayerIndex.Three, controllerChargeVibration * t, controllerChargeVibration * t);
			}
			else if (targetJoystick == 4)
			{
				GamePad.SetVibration(PlayerIndex.Four, controllerChargeVibration * t, controllerChargeVibration * t);
			}
			*/

			if (refReticle.chargeLevel == refReticle.chargeSprites.Length - 2)
			{
				chargeShakeLevel = 1;
			}
			else if (refReticle.chargeLevel == refReticle.chargeSprites.Length - 1)
			{
				chargeShakeLevel = 2;

				if (!isChargeHoldingSound)
				{ 
					isChargeHoldingSound = true;

					AudioSub.Stop();
					AudioSub.volume = 0.2f;
					AudioSub.pitch = 1.2f;
					AudioSub.loop = true;
					AudioSub.clip = sfxChargeHold;
					AudioSub.Play();
				}
			}
		}
		else if (refReticle.chargeLevel != 0)
		{
			chargeShakeLevel = 0;
			refReticle.ChangeChargeLevel(0);

			/*
			if (lastShakeTimer <= 0f)
			{
				if (targetJoystick == 1)
				{
					GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
				}
				else if (targetJoystick == 2)
				{
					GamePad.SetVibration(PlayerIndex.Two, 0f, 0f);
				}
				else if (targetJoystick == 3)
				{
					GamePad.SetVibration(PlayerIndex.Three, 0f, 0f);
				}
				else if (targetJoystick == 4)
				{
					GamePad.SetVibration(PlayerIndex.Four, 0f, 0f);
				}
			}
			*/
		}
	}

	private void ChargeShake()
	{
		if (isLastShaking)
		{
			lastShakeTimer -= Time.deltaTime;
			if (lastShakeTimer <= 0f)
			{
				lastShakeTimer = 0;
				isLastShaking = false;
				if (lastShakeTimer <= 0f)
				{
					/*
					if (targetJoystick == 1)
					{
						GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
					}
					else if (targetJoystick == 2)
					{
						GamePad.SetVibration(PlayerIndex.Two, 0f, 0f);
					}
					else if (targetJoystick == 3)
					{
						GamePad.SetVibration(PlayerIndex.Three, 0f, 0f);
					}
					else if (targetJoystick == 4)
					{
						GamePad.SetVibration(PlayerIndex.Four, 0f, 0f);
					}
					*/
				}
			}
			else
			{
				if (lastShakeTimer <= 0f)
				{
					/*
					if (targetJoystick == 1)
					{
						GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
					}
					else if (targetJoystick == 2)
					{
						GamePad.SetVibration(PlayerIndex.Two, 1f, 1f);
					}
					else if (targetJoystick == 3)
					{
						GamePad.SetVibration(PlayerIndex.Three, 1f, 1f);
					}
					else if (targetJoystick == 4)
					{
						GamePad.SetVibration(PlayerIndex.Four, 1f, 1f);
					}
					*/
				}
			}

			return;
		}

		if (chargeShakeLevel == 1)
		{
			screenShakeTimer += Time.deltaTime;
			refCamera.transform.localPosition = Vector3.LerpUnclamped
			(
				savedCameraLocalPos + refCamera.transform.up * -screenShakeAmplitude / 3f,
				savedCameraLocalPos + refCamera.transform.up * screenShakeAmplitude / 3f,
				Mathf.Sin(screenShakeTimer / screenShakeFrequency)
			);
		}
		else if (chargeShakeLevel == 2)
		{
			screenShakeTimer += Time.deltaTime;
			refCamera.transform.localPosition = Vector3.LerpUnclamped
			(
				savedCameraLocalPos + refCamera.transform.up * -screenShakeAmplitude,
				savedCameraLocalPos + refCamera.transform.up * screenShakeAmplitude,
				Mathf.Sin(screenShakeTimer / screenShakeFrequency)
			);
		}
		else
		{
			refCamera.transform.localPosition = savedCameraLocalPos;
			screenShakeTimer = 0f;
		}
	}

	/// <summary>
	/// Check the height of the player. If they are below the determined respawn height
	/// then respawn them to their starting position at scene begin.
	/// </summary>
	private void CheckRespawnHeight()
	{
		if (transform.position.y <= respawnHeight)
		{
			transform.position = savedStartSpawnPos;
		}
	}

	/// <summary>
	/// Update the UI HUD elements.
	/// </summary>
	private void UpdateUI()
	{
		ammoText.text = "x" + ammo.ToString();
		if (ammo <= 0)
		{
			ammo = 0;
			noAmmoIcon.enabled = true;
		}
		else
		{
			if (ammo > 99)
			{
				ammo = 99;
			}
			noAmmoIcon.enabled = false;
		}
	}

	#endregion

	#region Public Methods

	public void CollectShirt()
	{
		popUp.PopUp();
		ammo++;

		Collect();
	}

	public void Setup(Reticle refReticle, RawImage ammoIcon, RawImage noAmmoIcon, Text ammoText, UIPopup popUp, int targetJoystick, RenderTexture renderTex, int playerID, LayerMask cullingMask, Color colorMain, int actualID)
	{
		this.refReticle = refReticle;
		this.ammoIcon = ammoIcon;
		this.noAmmoIcon = noAmmoIcon;
		this.ammoText = ammoText;
		this.popUp = popUp;
		this.targetJoystick = targetJoystick + 1;
		this.colorMain = colorMain;
		this.playerID = actualID;

		if (playerID != 0)
		{
			refMesh.gameObject.layer = playerID + 16;
			shirtMesh.gameObject.layer = playerID + 16;
		}

		shirtColor = colorMain;

		refCamera.targetTexture = renderTex;
		refCamera.cullingMask = cullingMask;

		refReticle.enabled = false;
		ammoIcon.enabled = false;
		noAmmoIcon.enabled = false;
		ammoText.enabled = false;

		//ScoreManager.main.SetBorderColor(playerID, colorMain);
	}

	public void TurnOnHUD()
	{
		refReticle.enabled = true;
		ammoIcon.enabled = true;
		noAmmoIcon.enabled = true;
		ammoText.enabled = true;
	}

	public void Hit(Vector3 force, Color color)
	{
		shirtColor = color;

		//Debug.Log("HIT " + force);
		isOnGround = false;
		//Jump();
		force.y = 0;
		refBody.AddForce(force * 30f + (Vector3.up * 20f), ForceMode.Impulse);

		AudioMain.Stop();
		AudioMain.volume = 1.0f;
		AudioMain.pitch = 1.0f;
		AudioMain.loop = false;
		AudioMain.clip = sfxHitPlayer;
		AudioMain.Play();

		ScoreManager.main.SetBorderColor(playerID, color);
	}

	public void HitWall()
	{
		AudioMain.Stop();
		AudioMain.volume = 0.6f;
		AudioMain.pitch = 1.0f;
		AudioMain.loop = false;
		AudioMain.clip = sfxHitWall;
		AudioMain.Play();
	}

	public void Collect()
	{
		AudioSub.Stop();
		AudioSub.volume = 0.9f;
		AudioSub.pitch = 1.0f;
		AudioSub.loop = false;
		AudioSub.clip = sfxCollect;
		AudioSub.Play();
	}

	#endregion
}
