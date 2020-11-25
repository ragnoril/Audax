using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitAgent : MonoBehaviour
{

	AudioSource AudioPlay;

	// Use this for initialization
	void Start ()
	{
		AudioPlay = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (GameManager.instance.IsGamePaused)
		{
			return;
		}

		float distance = Vector3.Distance(transform.position, GameManager.instance.PlayerOne.transform.position);
		float volume = (20f - distance) / 10f;
		AudioPlay.volume = volume;

		//Debug.Log("Distance: " + distance.ToString());
	}

	public void Pause(bool value)
	{
		GetComponent<Animator>().enabled = !value;
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject.tag == "Player")
			GameManager.instance.UpgradeScreen();
	}
}
