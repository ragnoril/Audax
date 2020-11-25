using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
	public enum WeaponTypes
	{
		Handgun = 0,
		Rifle,
		Shotgun,
		Sniper,
		Minigun,
		Rocket,
		WeaponCount
	};

	public int MaxAmmoCount;
	public int CurAmmoCount;
	public int AmmoType;
	public int BulletCount;
	public float BulletSpeed;
	public float BulletSpread;
	public float BulletRange;
	public float RateOfFire;
	public float WeaponPushback;

	public GameObject BulletPrefab;

	public AudioClip[] ShootingSounds;
	public AudioClip UnloadSound;
	public AudioClip ReloadSound;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void AddAmmo(int val)
	{
		CurAmmoCount += val;

		if (CurAmmoCount > MaxAmmoCount) CurAmmoCount = MaxAmmoCount;
	}

	public void Shoot()
	{
		CurAmmoCount -= 1;
		//play shoot anim; muzzle flash
		//play shoot sound;

	}

	public void Reload(int val)
	{
		AddAmmo(val);

		// play reload sound
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		if (col.gameObject.tag == "Enemy")
		{
			EnemyAgent enemy = col.gameObject.GetComponent<EnemyAgent>();
			GameObject go = GameObject.Instantiate(enemy.BloodPrefab, col.gameObject.transform.position, Quaternion.Euler(-this.transform.rotation.eulerAngles));
			enemy.GetComponent<Rigidbody2D>().AddForce(transform.up * 5f, ForceMode2D.Impulse); 
			enemy.GotHit(10f);
		}
	}
}
