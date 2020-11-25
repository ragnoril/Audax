using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public Text PressToSelectText;
	float isBlinkOn = 1f;

	// Use this for initialization
	void Start ()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		StartCoroutine(Blink());

		if (Input.anyKey)
		{
			SceneManager.LoadScene("fooScene");
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
