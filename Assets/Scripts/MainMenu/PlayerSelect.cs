using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelect : MonoBehaviour
{

	public int PlayerID;
	public bool IsSelected;


	public Text PressToSelectText;
	public GameObject CharPanel;


	float isBlinkOn;

	// Use this for initialization
	void Start ()
	{
		isBlinkOn = 1f;
		 
		if (IsSelected)
		{
			PressToSelectText.gameObject.SetActive(false);
			CharPanel.SetActive(true);
		}
		else
		{
			PressToSelectText.gameObject.SetActive(true);
			CharPanel.SetActive(false);
		}
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if (!IsSelected)
		{
			StartCoroutine(Blink());
		}
	}

	IEnumerator Blink()
	{

		float alpha = PressToSelectText.color.a;

		alpha -= (3f * Time.fixedDeltaTime * isBlinkOn);
		if (alpha < 0f)
		{
			isBlinkOn = -1f;
		}
		else if (alpha > 1f)
		{
			isBlinkOn = 1f;
		}
		PressToSelectText.color = new Color(PressToSelectText.color.r, PressToSelectText.color.g, PressToSelectText.color.b, alpha);
		//Play sound
		yield return null;

	}
}
