
//-------------------------------------------------------------------------------------------------
//  Clothes Combat - Projectile                                                     v0.0_2018.07.06
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
/// Controls the setup, behaviour, and properties of a shirt projectile fired by a player.
/// </summary>
public class Projectile : Actor 
{
	#region Inspector Fields

	[SerializeField]
	private bool autoSetup;

	[SerializeField]
	private Material[] fallingAnim;

	[SerializeField]
	private Material groundMat;

	[Space(10)]

	[SerializeField]
	private float fallingAnimFrameTime;

	[SerializeField]
	private float landingShrinkRatio;

	[SerializeField]
	private float landingOffsetHeight;

	[Space(10)]
	[SerializeField]
	private float selfPickUpTime;
	
	[SerializeField]
	private float moveSpeed;

	[SerializeField]
	private float pickUpRange;

	[Space(10)]
	[SerializeField]
	private LayerMask collideMask;

	#endregion

	#region Run-Time Fields

	private float pickUpRadius;

	private Vector3 savedLastPos;
	private Vector3 launchVelocity;
	private GameObject targetPlayer;
	private Color mainColor;
	private Collider[] colliders;

	private float fallingAnimTimer;
	private int currentFrame = 0;

	private Vector3 lastVel;

	private bool isFalling = false;
	private bool isGrounded = false;
	private bool isCollected = false;
	private bool isHit = false;

	private float selfPickUpTimer = 0f;

	private PlayerController player;

	#endregion

	#region Monobehaviour

	protected void Start()
	{
		if (autoSetup)
		{
			Setup(Vector3.zero, Color.white, 0f, null);
			isGrounded = true;
		}

		refRend.material.SetColor("_ColorTint", mainColor);
		savedLastPos = transform.position;

		colliders = GetComponentsInChildren<Collider>();
		colliders[1].enabled = false;
		colliders[0].gameObject.layer = 16;

		selfPickUpTimer = selfPickUpTime;

		Invoke("EnableHit", 0.1f);
	}

	protected override void Update()
	{
		base.Update();

		if (isCollected)
		{
			colliders[0].gameObject.layer = 16;

			transform.position = Vector3.MoveTowards
			(
				transform.position, 
				targetPlayer.transform.position, 
				moveSpeed * Time.deltaTime
			);

			if (Vector3.Distance(transform.position, targetPlayer.transform.position) < pickUpRange)
			{
				player.CollectShirt();
				Destroy(gameObject);
			}

			return;
		}

		selfPickUpTimer -= Time.deltaTime;

		if (fallingAnimFrameTime <= 0)
		{
			return;
		}

		if (isFalling)
		{
			fallingAnimTimer += Time.deltaTime;

			if (fallingAnimTimer >= 1f / fallingAnimFrameTime)
			{
				fallingAnimTimer = 0f;
				currentFrame++;
				if (currentFrame >= fallingAnim.Length)
				{
					currentFrame = 0;
				}

				if (!autoSetup)
				{ 
					refRend.material = fallingAnim[currentFrame];
					refRend.material.SetColor("_ColorTint", mainColor);
				}
			}
		}

		CheckNoMove();

		// Propel
		transform.Translate(Quaternion.AngleAxis(-90f, Vector3.up) * (propelVector * Time.deltaTime));
		if (propelVector != Vector3.zero && !isGrounded)
		{
			if (!autoSetup)
			{
				refRend.material = groundMat;
				refRend.material.SetColor("_ColorTint", mainColor);
			}
			isGrounded = true;

			colliders[1].enabled = true;

			refVisual.localScale = transform.localScale * landingShrinkRatio;
			refVisual.Translate(Vector3.down * landingOffsetHeight);
		}

		propelVector = Vector3.zero;
	}

	private void EnableHit()
	{
		colliders[0].gameObject.layer = 14;
	}

	private void FixedUpdate()
	{
		lastVel = transform.position - savedLastPos;

		if (isHit)
		{
			return;
		}

		RaycastHit hit;
		if (Physics.Raycast(transform.position, refBody.velocity, out hit, refBody.velocity.magnitude * Time.fixedDeltaTime, collideMask))
		{
			if (hit.collider.isTrigger)
			{
				return;
			}
			transform.position = hit.point;
			refBody.velocity = Vector3.zero;

			isHit = true;

			if (isGrounded)
			{
				colliders[1].enabled = true;
				return;
			}

			if (hit.collider.gameObject.layer == 9 || hit.collider.gameObject.layer == 14)
			{
				if (!autoSetup)
				{
					refRend.material = groundMat;
					refRend.material.SetColor("_ColorTint", mainColor);
				}
				isGrounded = true;

				colliders[1].enabled = true;

				refVisual.localScale = transform.localScale * landingShrinkRatio;
				refVisual.Translate(Vector3.down * landingOffsetHeight);
			}
			else if (!isFalling)
			{
				if (!autoSetup)
				{
					refRend.material = fallingAnim[0];
					refRend.material.SetColor("_ColorTint", mainColor);
				}
				isFalling = true;
				colliders[1].enabled = true;
				fallingAnimTimer = 0f;
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!isGrounded || !isFalling)
		{
			colliders[1].enabled = false;
		}

		if (isGrounded)
		{
			colliders[1].enabled = true;
			return;
		}

		if (collision.collider.gameObject.layer == 11 && !autoSetup)
		{
			PlayerController playerVictim = collision.collider.gameObject.GetComponentInParent<PlayerController>();
			if (playerVictim != null && playerVictim != player)
			{
				playerVictim.Hit(lastVel, mainColor);
			}
			Destroy(gameObject);
		}

		if (collision.collider.gameObject.layer == 9 || collision.collider.gameObject.layer == 14) 
		{
			if (!autoSetup)
			{
				refRend.material = groundMat;
				refRend.material.SetColor("_ColorTint", mainColor);
			}
			isGrounded = true;

			colliders[1].enabled = true;

			refVisual.localScale = transform.localScale * landingShrinkRatio;
			refVisual.Translate(Vector3.down * landingOffsetHeight);

			if (player != null)
			{ 
				player.HitWall();
			}
		}
		else if (!isFalling)
		{
			if (!autoSetup)
			{
				refRend.material = fallingAnim[0];
				refRend.material.SetColor("_ColorTint", mainColor);
			}
			isFalling = true;

			colliders[1].enabled = true;

			fallingAnimTimer = 0f;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isGrounded)
		{
			return;
		}

		if (other.gameObject.layer != 11 || (selfPickUpTimer > 0f))// && other.transform.parent.name == player.name))
		{
			return;
		}

		targetPlayer = other.gameObject;
		player = targetPlayer.GetComponentInParent<PlayerController>();

		Rigidbody refBody = GetComponent<Rigidbody>();
		refBody.isKinematic = true;

		isCollected = true;
	}

	#endregion

	#region Private Methods

	private void Launch()
	{
		//Debug.Log("Launch " + launchVelocity);
		refBody.AddForce(launchVelocity, ForceMode.Impulse);
	}

	private void CheckNoMove()
	{
		if (isGrounded)
		{
			colliders[1].enabled = true;
			return;
		}

		/*
		if (Vector3.Distance(transform.position, savedLastPos) <= 0.2f)
		{
			refRend.material = groundMat;
			refRend.material.SetColor("_ColorTint", mainColor);
			isGrounded = true;

			colliders[1].enabled = true;

			refVisual.localScale = transform.localScale * landingShrinkRatio;
			refVisual.Translate(Vector3.down * landingOffsetHeight);
		}
		*/

		if (transform.position.y <= -50f)
		{
			Destroy(gameObject);
		}
	}

	#endregion

	#region Public Methods

	public void Setup(Vector3 launchVelocity, Color mainColor, float angle, PlayerController self)
	{
		this.launchVelocity = launchVelocity;
		this.mainColor = mainColor;
		player = self;

		transform.eulerAngles = new Vector3
		(
			0f,
			angle + 95f,
			0f
		);

		refRend.material.SetColor("_Tint", mainColor);

		Launch();
	}

	#endregion
}
