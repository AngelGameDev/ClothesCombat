using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimation : MonoBehaviour 
{
	public static WeaponAnimation main;

	public SpriteRenderer[] sprites;

	public Sprite defaultSprite;
	public Sprite reloadSprite;

	private WeaponWobble[] wobble;

	private void Awake()
	{
		main = this;	
	}

	private void Start()
	{
		wobble = new WeaponWobble[sprites.Length];
		for (int i=0; i<sprites.Length; i++)
		{
			wobble[i] = sprites[i].GetComponent<WeaponWobble>();
			wobble[i].SetWobbleFactor(0f);
		}
	}

	public void Reload(int index)
	{
		sprites[index].sprite = reloadSprite;
	}

	public void UnReload(int index)
	{
		sprites[index].sprite = defaultSprite;
	}

	public void UpdateWobble(int index, float wobbleFactor)
	{
		wobble[index].SetWobbleFactor(wobbleFactor);
	}
}