using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
	public int CollectibleType;
	public int Amount;
	public int SubType;

	public AudioClip[] CollectSounds;
	
	// Use this for initialization
	void Start ()
	{
		
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject.tag == "Player")
		{
			//GetComponent<AudioSource>().PlayOneShot(CollectSfx);
			if (CollectibleType == 0) // healthPack
			{
				GameManager.instance.PlayerOne.CurHitPoints += Amount;
				if (GameManager.instance.PlayerOne.CurHitPoints > GameManager.instance.PlayerOne.MaxHitPoints)
					GameManager.instance.PlayerOne.CurHitPoints = GameManager.instance.PlayerOne.MaxHitPoints;

				GameManager.instance.PlayerOne.PlayerUI.ChangeHealth();

			}
			else if (CollectibleType == 1) // ammo
			{
				GameManager.instance.PlayerOne.AmmoList[SubType] += Amount;
				GameManager.instance.PlayerOne.PlayerUI.SetGunPanel();
			}
			else if (CollectibleType == 2) // weapon
			{
				GameManager.instance.PlayerOne.WeaponList[SubType] = true;
				GameManager.instance.PlayerOne.Weapons[SubType].AddAmmo(Amount);
				GameManager.instance.PlayerOne.PlayerUI.SetGunPanel();
			}

			int sfxId = Random.Range(0, CollectSounds.Length);
			GameObject deathSounder = new GameObject();
			var deathSfx = deathSounder.AddComponent<AudioSource>();
			deathSfx.clip = CollectSounds[sfxId];
			deathSfx.Play();
			Destroy(deathSounder, 2.5f);

			Destroy(this.gameObject);
		}
			
	}
}
