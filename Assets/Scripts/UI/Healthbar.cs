using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
	public float Value;
	public float MaxValue;
	public Image BarImage;
	public Image BgImage;

	private float length;

	// Use this for initialization
	void Start ()
	{
		length = GetComponent<RectTransform>().rect.width;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void ChangeHealth()
	{
		BarImage.rectTransform.sizeDelta = new Vector2((Value * length) / MaxValue, BarImage.rectTransform.sizeDelta.y);
	}
}
