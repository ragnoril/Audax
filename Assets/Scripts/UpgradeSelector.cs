using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSelector : MonoBehaviour
{

	public enum Upgrades
	{
		ExtraBullet = 0,
		FasterBullets,
		ExtraBulletRange,
		MorePowerfulBullets,
		ExtraHealth,
		MoreSpeed,
		MoreAccurateBullets,
		MoreRateOfFire,
		Regeneration,
		Vampirism,
		SlowEnemies,
		ExplodingBullets,
		//BiggerMagazineClip,
		FireResistance,
		//HomingBullets,
		MoreCollectibleDrop,
		WeakEnemies,
		SlowerEnemyBullets,
		SecondChance,
		UpgradeCount
	};


	public int[] UpgradeRatios = new int[(int)Upgrades.UpgradeCount];


	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void DistributeStartingRatios()
	{
		for (int i = 0; i < UpgradeRatios.Length; i++)
		{
			UpgradeRatios[i] = 100 / (int)Upgrades.UpgradeCount;
		}
	}

	public int GetOneUpgrade()
	{
		/*
		int diceResult = Random.Range(0, 100);
		
		for(int i = 0; i < UpgradeRatios.Length; i++)
		{
			if (UpgradeRatios[i] > diceResult)
			{
				return i; 
			}
		}
		*/

		return Random.Range(0, (int)Upgrades.UpgradeCount);
	}

	public void CalculateSelectedUpgradeRatio(int upgradeId)
	{
		UpgradeRatios[upgradeId] -= (((int)Upgrades.UpgradeCount) - 1);

		for (int i = 0; i < UpgradeRatios.Length; i++)
		{
			if (i != upgradeId)
			{
				UpgradeRatios[i] += 1;
			}
		}
	}

	public string GetSelectedUpgradeText(int selectedUpgrade)
	{
		string upgradeText = "";
		if (selectedUpgrade == (int)Upgrades.ExtraBullet)
		{
			upgradeText = "Extra Bullet";
		}
		else if (selectedUpgrade == (int)Upgrades.ExtraBulletRange)
		{
			upgradeText = "Longer Bullet Range";
		}
		else if (selectedUpgrade == (int)Upgrades.ExtraHealth)
		{
			upgradeText = "Extra Health";
		}
		else if (selectedUpgrade == (int)Upgrades.FasterBullets)
		{
			upgradeText = "Faster Bullets";
		}
		else if (selectedUpgrade == (int)Upgrades.MoreAccurateBullets)
		{
			upgradeText = "Better Shooting Accuracy";
		}
		else if (selectedUpgrade == (int)Upgrades.MorePowerfulBullets)
		{
			upgradeText = "Extra Bullet Damage";
		}
		else if (selectedUpgrade == (int)Upgrades.MoreRateOfFire)
		{
			upgradeText = "Increased Rate of Fire";
		}
		else if (selectedUpgrade == (int)Upgrades.MoreSpeed)
		{
			upgradeText = "Faster Moving";
		}
		else if (selectedUpgrade == (int)Upgrades.Regeneration)
		{
			upgradeText = "Regeneration";
		}
		else if (selectedUpgrade == (int)Upgrades.Vampirism)
		{
			upgradeText = "Life Steal";
		}
		else if (selectedUpgrade == (int)Upgrades.SlowEnemies)
		{
			upgradeText = "Slow Moving Enemies";
		}
		else if (selectedUpgrade == (int)Upgrades.ExplodingBullets)
		{
			upgradeText = "Exploding Bullets";
		}
		/*
		else if (selectedUpgrade == (int)Upgrades.BiggerMagazineClip)
		{
			upgradeText = "Bigger Magazine Clip";
		}
		*/
		else if (selectedUpgrade == (int)Upgrades.FireResistance)
		{
			upgradeText = "Fire Resistance";
		}
		/*
		else if (selectedUpgrade == (int)Upgrades.HomingBullets)
		{
			upgradeText = "Homing Bullets";
		}
		*/
		else if (selectedUpgrade == (int)Upgrades.MoreCollectibleDrop)
		{
			upgradeText = "More Collectible Drop";
		}
		else if (selectedUpgrade == (int)Upgrades.WeakEnemies)
		{
			upgradeText = "Weaker Enemies";
		}
		else if (selectedUpgrade == (int)Upgrades.SecondChance)
		{
			upgradeText = "Life After Death";
		}
		else if (selectedUpgrade == (int)Upgrades.SlowerEnemyBullets)
		{
			upgradeText = "Slower Enemy Bullets";
		}

		return upgradeText;
	}

	public void ImplementUpgrade(int selectedUpgrade)
	{
		if (selectedUpgrade == (int)Upgrades.ExtraBullet)
		{
			GameManager.instance.PlayerOne.BulletCount += 1;
			GameManager.instance.PlayerOne.BulletSpread += 0.05f;
		}
		else if (selectedUpgrade == (int)Upgrades.ExtraBulletRange)
		{
			GameManager.instance.PlayerOne.BulletRange += 0.2f;
		}
		else if (selectedUpgrade == (int)Upgrades.ExtraHealth)
		{
			GameManager.instance.PlayerOne.MaxHitPoints += 20f;
			GameManager.instance.PlayerOne.PlayerUI.HealthBar.MaxValue = GameManager.instance.PlayerOne.MaxHitPoints;
		}
		/*
		else if (selectedUpgrade == (int)Upgrades.BetterRegeneration)
		{
			GameManager.instance.PlayerOne.Healing += 2f;
		}
		*/
		else if (selectedUpgrade == (int)Upgrades.FasterBullets)
		{
			GameManager.instance.PlayerOne.BulletSpeed += 1f;
		}
		else if (selectedUpgrade == (int)Upgrades.MoreAccurateBullets)
		{
			GameManager.instance.PlayerOne.BulletSpread -= 0.05f;
		}
		else if (selectedUpgrade == (int)Upgrades.MorePowerfulBullets)
		{
			GameManager.instance.PlayerOne.BulletDamage += 1.5f;
		}
		else if (selectedUpgrade == (int)Upgrades.MoreRateOfFire)
		{
			GameManager.instance.PlayerOne.RateOfFire -= 0.05f;
		}
		else if (selectedUpgrade == (int)Upgrades.MoreSpeed)
		{
			GameManager.instance.PlayerOne.MoveSpeed += 1f;
		}
		else if (selectedUpgrade == (int)Upgrades.Regeneration)
		{
			GameManager.instance.PlayerOne.Regeneration = true;
		}
		else if (selectedUpgrade == (int)Upgrades.Vampirism)
		{
			GameManager.instance.PlayerOne.Vampirism = true;
		}
		else if (selectedUpgrade == (int)Upgrades.SlowEnemies)
		{
			GameManager.instance.EnemyMovePenalty += 20f;
		}
		else if (selectedUpgrade == (int)Upgrades.ExplodingBullets)
		{
			GameManager.instance.PlayerOne.ExplodingBullets = true;
		}
		/*
		else if (selectedUpgrade == (int)Upgrades.BiggerMagazineClip)
		{
			// Todo: Player'a extramagazine variable'ı verdim ama
			// etkisini nasıl işleyecek?

			GameManager.instance.PlayerOne.ExtraMagazine += 5;
		}
		*/
		else if (selectedUpgrade == (int)Upgrades.FireResistance)
		{
			GameManager.instance.PlayerOne.FireResistance = true;
		}
		/*
		else if (selectedUpgrade == (int)Upgrades.HomingBullets)
		{
			
		}
		*/
		else if (selectedUpgrade == (int)Upgrades.MoreCollectibleDrop)
		{
			GameManager.instance.CollectibleDropBonus += 5;
		}
		else if (selectedUpgrade == (int)Upgrades.WeakEnemies)
		{
			GameManager.instance.EnemyHealthPenalty += 10f;
		}
		else if (selectedUpgrade == (int)Upgrades.SecondChance)
		{ 
			GameManager.instance.PlayerOne.SecondChance = true;
		}
		else if (selectedUpgrade == (int)Upgrades.SlowerEnemyBullets)
		{
			GameManager.instance.EnemyBulletSpeedPenalty += 1.5f;
		}

		//CalculateSelectedUpgradeRatio(selectedUpgrade);
	}

}
