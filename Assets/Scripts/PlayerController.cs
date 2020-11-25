using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public GameObject BulletPrefab;
	public GameObject ExplosionPrefab;

	public PlayerUI PlayerUI;

	public float MaxHitPoints;
	public float CurHitPoints;
	public float Healing;
	public float MoveSpeed;
	public int BulletCount;
	public float BulletSpeed;
	public float BulletSpread;
	public float BulletRange;
	public float BulletDamage;
	public int ExtraMagazine;
	//public float MovingBulletSpread;
	public float RateOfFire;
	private float RateOfFireTimer;
	public float WeaponPushback;

	public bool Regeneration;
	public bool Vampirism;
	public bool SecondChance;
	public bool ExplodingBullets;
	public bool HomingBullets;
	public bool FireResistance;


	public int WeaponType;
	public Sprite[] WeaponSprites;
	public Weapon[] Weapons;
	public bool[] WeaponList = new bool[(int)Weapon.WeaponTypes.WeaponCount];

	// 0 bullet, 1 shell, 2 minigun, 3 rocket
	public int[] AmmoList;

	public AudioClip ShootSfx;
	public AudioClip HurtSfx;
	public AudioClip[] PainSounds;
	public AudioClip[] DeathSounds;

	AudioSource AudioPlay;
	Animator Anim;
	Rigidbody2D Rigidbody2d;
	Vector2 _beforePauseVelocity;
	public GameObject GunHolder;
	public Transform MuzzlePosition;
	public Transform CameraPosition;
	//private Vector3 oldMousePosition;

	// Use this for initialization
	void Start ()
    {
		AudioPlay = GetComponent<AudioSource>();
		Anim = GetComponent<Animator>();
		Rigidbody2d = GetComponent<Rigidbody2D>();
		//GunRigidbody2d = GunHolder.GetComponent<Rigidbody2D>();
		RateOfFireTimer = Time.fixedTime;
		CurHitPoints = MaxHitPoints;

		Regeneration = false;
		Vampirism = false;
		SecondChance = false;
		FireResistance = false;
		HomingBullets = false;
		ExplodingBullets = false;

		PlayerUI.ChangeHealth();
		/*
		if (WeaponType < WeaponSprites.Length)
			GunHolder.GetComponent<SpriteRenderer>().sprite = WeaponSprites[WeaponType];
		*/

		AmmoList = new int[4];
		AmmoList[0] = 60;
		AmmoList[1] = 0;
		AmmoList[2] = 0;
		AmmoList[3] = 0;

		WeaponList[0] = true;
		Weapons[0].gameObject.SetActive(true);
		GunHolder = Weapons[0].gameObject;
		MuzzlePosition = GunHolder.transform.Find("Muzzle");
		for (int i = 1; i < (int)Weapon.WeaponTypes.WeaponCount; i++)
		{
			WeaponList[i] = false;
			Weapons[i].gameObject.SetActive(false);
		}

		PlayerUI.ShootBar.MaxValue = RateOfFire + Weapons[WeaponType].RateOfFire;
		PlayerUI.ChangeAmmoCount(Weapons[WeaponType].MaxAmmoCount, Weapons[WeaponType].CurAmmoCount);
		PlayerUI.SetGunPanel();
	}

	
	// Update is called once per frame
	void Update ()
    {
		if (GameManager.instance.IsGamePaused)
		{
			return;
		}

		// set camera position
		
		//CameraPosition.position = transform.position + (transform.up * 2f);
		//CameraPosition.position = (Camera.main.WorldToScreenPoint(transform.position) - transform.position) / 2f;

		if (CurHitPoints <= 0)
		{
			if (SecondChance)
			{
				// create explosion
				GameObject bam = GameObject.Instantiate(ExplosionPrefab, transform.position, Quaternion.Euler(new Vector3(0f, 0f, 180f) + this.transform.rotation.eulerAngles));
				CurHitPoints = MaxHitPoints;
				SecondChance = false;
			}
			else
			{
				GameManager.instance.DeathScreen();
			}
		}
		else if (CurHitPoints < 15)
		{
			GameManager.instance.IsGameSloMo = true;
		}
		else if (GameManager.instance.IsGameSloMo)
		{
			GameManager.instance.IsGameSloMo = false;
		}


		if (Input.GetKeyDown(KeyCode.R))
		{
			ReloadWeapon();
		}

		if (Input.GetKey(KeyCode.Tab))
		{
			GameManager.instance.ShowMinimap();
		}

		if (Input.GetKeyUp(KeyCode.Tab))
		{
			GameManager.instance.HideMinimap();
		}

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			if (WeaponList[(int)Weapon.WeaponTypes.Handgun])
			{
				ChangeWeapon((int)Weapon.WeaponTypes.Handgun);
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			if (WeaponList[(int)Weapon.WeaponTypes.Rifle])
			{
				ChangeWeapon((int)Weapon.WeaponTypes.Rifle);
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			if (WeaponList[(int)Weapon.WeaponTypes.Shotgun])
			{
				ChangeWeapon((int)Weapon.WeaponTypes.Shotgun);
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			if (WeaponList[(int)Weapon.WeaponTypes.Sniper])
			{
				ChangeWeapon((int)Weapon.WeaponTypes.Sniper);
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			if (WeaponList[(int)Weapon.WeaponTypes.Minigun])
			{
				ChangeWeapon((int)Weapon.WeaponTypes.Minigun);
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			if (WeaponList[(int)Weapon.WeaponTypes.Rocket])
			{
				ChangeWeapon((int)Weapon.WeaponTypes.Rocket);
			}
		}

		if (Input.GetKeyDown(KeyCode.Q) || (Input.GetAxis("Mouse ScrollWheel") > 0f))
		{
			int nextWeapon = WeaponType + 1;
			if (nextWeapon >= (int)Weapon.WeaponTypes.WeaponCount)
			{
				nextWeapon = 0;
			}

			while (WeaponList[nextWeapon] == false)
			{
				nextWeapon = nextWeapon + 1;
				if (nextWeapon >= (int)Weapon.WeaponTypes.WeaponCount)
				{
					nextWeapon = 0;
				}
			}

			ChangeWeapon(nextWeapon);
		}

		if (Input.GetKeyDown(KeyCode.E) || (Input.GetAxis("Mouse ScrollWheel") < 0f))
		{
			int nextWeapon = WeaponType - 1;
			if (nextWeapon < 0)
			{
				nextWeapon = (int)Weapon.WeaponTypes.WeaponCount - 1;
			}

			while (WeaponList[nextWeapon] == false)
			{
				nextWeapon = nextWeapon - 1;
				if (nextWeapon < 0)
				{
					nextWeapon = (int)Weapon.WeaponTypes.WeaponCount - 1;
				}
			}

			ChangeWeapon(nextWeapon);
		}
	}

	void FixedUpdate()
	{
		if (GameManager.instance.IsGamePaused)
		{
			return;
		}

		Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
		Vector3 dir = Input.mousePosition - objectPos;
		GunHolder.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 270f + (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg)));
		//GunRigidbody2d.rotation = 270f + (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
		
		if (GunHolder.transform.rotation.eulerAngles.z < 180)
		{
			GunHolder.GetComponent<SpriteRenderer>().flipX = true;
		}
		else if (GunHolder.transform.rotation.eulerAngles.z > 180)
		{
			GunHolder.GetComponent<SpriteRenderer>().flipX = false;
		}

		// Move senteces
		float moveX = Input.GetAxis("Horizontal") * MoveSpeed;
		float moveY = Input.GetAxis("Vertical") * MoveSpeed;
		Rigidbody2d.velocity = new Vector2(Mathf.Lerp(0, moveX, 0.8f), Mathf.Lerp(0, moveY, 0.8f));
		Rigidbody2d.angularVelocity = 0f;

		if (moveX < 0f)
		{
			GetComponent<SpriteRenderer>().flipX = true;	
		}
		else if (moveX > 0f)
		{
			GetComponent<SpriteRenderer>().flipX = false;
		}


		if (moveX != 0f || moveY != 0f)
		{
			/*
			MovingBulletSpread += 0.05f * Time.fixedDeltaTime;
			MovingBulletSpread = Mathf.Min(MovingBulletSpread, 1.4f);
			*/
			Anim.SetBool("isRunning", true);
        }
		else
		{
			/*
			MovingBulletSpread -= 0.1f * Time.fixedDeltaTime;
			MovingBulletSpread = Mathf.Max(MovingBulletSpread, 1f);
			*/


			if (Regeneration)
			{
				CurHitPoints += Healing * Time.fixedDeltaTime;
				CurHitPoints = Mathf.Min(MaxHitPoints, CurHitPoints);
				if (CurHitPoints != PlayerUI.HealthBar.Value)
					PlayerUI.ChangeHealth();
			}
			
			
			Anim.SetBool("isRunning", false);
		}

		if (WeaponType == 3)
		{
			DrawLaserLine();
		}
		else
		{
			LineRenderer lr = GetComponent<LineRenderer>();
			lr.SetPosition(0, Vector3.zero);
			lr.SetPosition(1, Vector3.zero);
		}

		if ((Time.fixedTime - RateOfFireTimer) > (Weapons[WeaponType].RateOfFire + RateOfFire))
		{
			PlayerUI.ShootBar.gameObject.SetActive(false);
		}
		else if (PlayerUI.ShootBar.gameObject.activeSelf)
		{
			PlayerUI.ShootBar.Value = (Time.fixedTime - RateOfFireTimer);
			PlayerUI.ShootBar.ChangeHealth();
		}

		// Fire a bullet.
		if (Input.GetMouseButton(0))
		{
			float rateOfFire = Weapons[WeaponType].RateOfFire + RateOfFire;
			if ((Time.fixedTime - RateOfFireTimer) > rateOfFire)
			{
				PlayerUI.ShootBar.gameObject.SetActive(true);

				if (Weapons[WeaponType].CurAmmoCount > 0)
				{
					FireBullet();
					RateOfFireTimer = Time.fixedTime;
				}
				else
				{
					/*
					if (Weapons[WeaponType].UnloadSound != null)
						AudioPlay.PlayOneShot(Weapons[WeaponType].UnloadSound);
					*/
					ReloadWeapon();
				}
			}
		}
	}


	public void Pause(bool value)
	{
		Anim.enabled = !value;

		if (value)
		{
			_beforePauseVelocity = Rigidbody2d.velocity;
			Rigidbody2d.Sleep();
		}
		else
		{
			Rigidbody2d.WakeUp();
			Rigidbody2d.velocity = _beforePauseVelocity;
		}
	}


	public void ReloadWeapon()
	{
		if ((WeaponType == 0) || (WeaponType == 1) || (WeaponType == 3))
		{
			if (AmmoList[0] > 0)
			{
				int ammo = Weapons[WeaponType].MaxAmmoCount - Weapons[WeaponType].CurAmmoCount;
				if (AmmoList[0] < ammo) ammo = AmmoList[0];
				AmmoList[0] -= ammo;
				Weapons[WeaponType].Reload(ammo);
				RateOfFireTimer = Time.fixedTime;
			}
		}
		else if (WeaponType == 2)
		{
			if (AmmoList[1] > 0)
			{
				int ammo = Weapons[WeaponType].MaxAmmoCount - Weapons[WeaponType].CurAmmoCount;
				if (AmmoList[1] > ammo) ammo = AmmoList[1];
				Weapons[WeaponType].Reload(ammo);
				RateOfFireTimer = Time.fixedTime;
			}
		}
		else if (WeaponType == 4)
		{
			if (AmmoList[2] > 0)
			{
				int ammo = Weapons[WeaponType].MaxAmmoCount - Weapons[WeaponType].CurAmmoCount;
				if (AmmoList[2] > ammo) ammo = AmmoList[2];
				Weapons[WeaponType].Reload(ammo);
				RateOfFireTimer = Time.fixedTime;
			}
		}
		else if (WeaponType == 5)
		{
			if (AmmoList[3] > 0)
			{
				int ammo = Weapons[WeaponType].MaxAmmoCount - Weapons[WeaponType].CurAmmoCount;
				if (AmmoList[3] > ammo) ammo = AmmoList[3];
				Weapons[WeaponType].Reload(ammo);
				RateOfFireTimer = Time.fixedTime;
			}
		}

		AudioPlay.PlayOneShot(Weapons[WeaponType].ReloadSound);
		PlayerUI.ChangeAmmoCount(Weapons[WeaponType].MaxAmmoCount, Weapons[WeaponType].CurAmmoCount);
	}

	public void ChangeWeapon(int weaponId)
	{
		WeaponType = weaponId;
		for (int i = 0; i < Weapons.Length; i++)
		{
			Weapons[i].gameObject.SetActive(false);
		}

		Weapons[weaponId].gameObject.SetActive(true);
		GunHolder = Weapons[weaponId].gameObject;
		MuzzlePosition = GunHolder.transform.Find("Muzzle");

		PlayerUI.ShootBar.MaxValue = RateOfFire + Weapons[WeaponType].RateOfFire;
		PlayerUI.SetGunPanel();
	}

	public void ResetPlayer()
	{
		MaxHitPoints = 100;
		CurHitPoints = MaxHitPoints;
		PlayerUI.HealthBar.MaxValue = MaxHitPoints;
		PlayerUI.HealthBar.Value = CurHitPoints;
		Healing = 3;
		MoveSpeed = 5;
		BulletCount = 0;
		BulletSpeed = 0;
		BulletSpread = 0;
		BulletRange = 0;
		BulletDamage = 0;
		ExtraMagazine = 0;
		RateOfFire = 0;
		PlayerUI.ShootBar.MaxValue = RateOfFire + Weapons[0].RateOfFire;
		PlayerUI.ShootBar.Value = 0f;
		WeaponPushback = 0;

		Regeneration = false;
		Vampirism = false;
		SecondChance = false;
		FireResistance = false;
		HomingBullets = false;
		ExplodingBullets = false;

		AmmoList[0] = 60;
		AmmoList[1] = 0;
		AmmoList[2] = 0;
		AmmoList[3] = 0;

		WeaponType = 0;
		WeaponList[0] = true;
		Weapons[0].CurAmmoCount = 20;
		Weapons[0].gameObject.SetActive(true);
		GunHolder = Weapons[0].gameObject;
		MuzzlePosition = GunHolder.transform.Find("Muzzle");
		for (int i = 1; i < (int)Weapon.WeaponTypes.WeaponCount; i++)
		{
			WeaponList[i] = false;
			Weapons[i].CurAmmoCount = 0;
			Weapons[i].gameObject.SetActive(false);
		}

		PlayerUI.ChangeHealth();
		PlayerUI.SetGunPanel();
	}

	public void DrawLaserLine()
	{
		LineRenderer lr = GetComponent<LineRenderer>();
		Vector2 pos = Vector2.zero;// Camera.main.ScreenToWorldPoint(Input.mousePosition);
								   
		LayerMask mask = LayerMask.GetMask("Enemy", "Default");
		ContactFilter2D filter = new ContactFilter2D();
		filter.SetLayerMask(mask);
		
		Vector2 dir = (MuzzlePosition.position - GunHolder.transform.position);
		RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, Mathf.Infinity);

		foreach(var hit in hits)
		{
			if (hit.collider.gameObject != this.gameObject)
			{
				/*
				if (hit.collider.isTrigger)
					continue;
				*/
				if (this.transform.position.z >= 0f)
					this.transform.position += new Vector3(0f, 0f, -1f);
				pos = hit.point;
				break;
			}
		}

		lr.startWidth = 0.05f;
		lr.endWidth = 0.05f;
		lr.SetPosition(0, GunHolder.transform.position);
		lr.SetPosition(1, pos);
	}

	public void FireBullet()
	{
		int bulletCount = BulletCount + Weapons[WeaponType].BulletCount;
		if (Weapons[WeaponType].CurAmmoCount < bulletCount)
			bulletCount = Weapons[WeaponType].CurAmmoCount;
		for (int i = 0; i < bulletCount; i++)
		{
			GameObject bullet = (GameObject)Instantiate(Weapons[WeaponType].BulletPrefab, MuzzlePosition.position, GunHolder.transform.rotation);
			bullet.transform.Rotate(0f, 0f, Random.Range(-(BulletSpread + Weapons[WeaponType].BulletSpread), BulletSpread + Weapons[WeaponType].BulletSpread) * Mathf.Rad2Deg);
			bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * (BulletSpeed + Weapons[WeaponType].BulletSpeed);
			bullet.GetComponent<Bullet>().dieTimer = BulletRange + Weapons[WeaponType].BulletRange;
			bullet.GetComponent<Bullet>().WeaponDamage += BulletDamage;
			bullet.GetComponent<Bullet>().Owner = this.transform;
			if (ExplodingBullets)
			{
				bullet.GetComponent<Bullet>().BulletType = 1;
			}

			GameManager.instance.ShotCount += 1;
			Weapons[WeaponType].CurAmmoCount -= 1;
		}
		/*
		//audio.PlayOneShot(clip);
		GetComponent<AudioSource>().PlayOneShot(shootSound);
		*/
		//AudioPlay.PlayOneShot(ShootSfx);
		int sfxId = Random.Range(0, Weapons[WeaponType].ShootingSounds.Length);
		AudioPlay.PlayOneShot(Weapons[WeaponType].ShootingSounds[sfxId]);
		Rigidbody2d.AddForce(-GunHolder.transform.up * Weapons[WeaponType].WeaponPushback, ForceMode2D.Impulse);
		PlayerUI.ChangeAmmoCount(Weapons[WeaponType].MaxAmmoCount, Weapons[WeaponType].CurAmmoCount);
	}

	public void GotHurt(float value)
	{
		int sfxId = Random.Range(0, PainSounds.Length);
		AudioPlay.PlayOneShot(PainSounds[sfxId]);


		GameManager.instance.HitCount += (int)value;

		CurHitPoints -= value;
		PlayerUI.ChangeHealth();
		//GameManager.instance.PauseAndResume(0.01f);
		GameManager.instance.ShakeIt();
		GameManager.instance.BloodOnScreen();
	}
	
}
