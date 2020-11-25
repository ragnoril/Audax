using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
	public PlayerController Player;
	public Healthbar HealthBar;
	public Healthbar ShootBar;
	public Text AmmoText;
	public Text[] GunTexts;

	// Use this for initialization
	void Start ()
	{
		Player.PlayerUI = this;
		gameObject.name += "_" + Player.name; 

		HealthBar.MaxValue = Player.MaxHitPoints;
		HealthBar.Value = Player.CurHitPoints;
		HealthBar.ChangeHealth();

		ShootBar.MaxValue = Player.RateOfFire + Player.Weapons[Player.WeaponType].RateOfFire;
		ShootBar.Value = 0f;
		ShootBar.ChangeHealth();
		ShootBar.gameObject.SetActive(false);

		//ScoreText.text = Player.name;
	}
	
	// Update is called once per frame
	void Update ()
	{

	}

	public void ChangeHealth()
	{
		HealthBar.MaxValue = Player.MaxHitPoints;
		HealthBar.Value = Player.CurHitPoints;
		HealthBar.ChangeHealth();
	}

	public void ChangeScore(float score)
	{
		//ScoreText.text = ""; 
	}

	public void ChangeAmmoCount(int maxAmmo, int curAmmo)
	{
		AmmoText.text = curAmmo.ToString() + " / " + maxAmmo.ToString() + "  " + Player.AmmoList[Player.Weapons[Player.WeaponType].AmmoType];
	}

	public void SetGunPanel()
	{
		if (Player.WeaponList.Length != GunTexts.Length) return;
		for (int i = 0; i < GunTexts.Length; i++)
		{
			if (Player.WeaponList[i])
			{
				GunTexts[i].color = Color.yellow;
			}
			else
			{
				GunTexts[i].color = Color.grey;
			}
		}

		AmmoText.text = Player.Weapons[Player.WeaponType].CurAmmoCount.ToString() + " / " + Player.Weapons[Player.WeaponType].MaxAmmoCount.ToString() + "  " + Player.AmmoList[Player.Weapons[Player.WeaponType].AmmoType];
	}
}
