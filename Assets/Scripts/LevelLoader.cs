using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour 
{
	public Reticle[] refReticle;

	public RawImage[] ammoIcon;

	public RawImage[] noAmmoIcon;

	public Text[] ammoText;

	public UIPopup[] popUp;

	public RenderTexture[] renderTex;

	public LayerMask[] cullingMasks;

	public Color[] colors;

	private void Start () 
	{
		GameController.main.LoadConfig(this);
	}
}
