using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{

	public Text LevelText;
	public Text TimeSpentText;
	public Text DamageTakenText;
	public Text EnemyKilledText;
	public Text ShotTakenText;

	// Use this for initialization
	void Start ()
	{

		
	}
	
	// Update is called once per frame
	void Update ()
	{
		LevelText.text = (GameManager.instance.Level + 1).ToString();
		TimeSpentText.text = GameManager.instance.TimeCount.ToString();
		DamageTakenText.text = GameManager.instance.HitCount.ToString();
		ShotTakenText.text = GameManager.instance.ShotCount.ToString();
		EnemyKilledText.text = GameManager.instance.KillCount.ToString();
	}
}
