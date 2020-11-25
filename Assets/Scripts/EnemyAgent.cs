using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAgent : MonoBehaviour
{
	public float MaxHitPoints;
	public float CurHitPoints;
	public float MoveSpeed;
	public float BulletSpeed;
	public float RateOfFire;
	private float RateOfFireTimer;

	public float BulletSpread;
	public float WeaponPushback;
	public float WeaponDamage;

	public Collider2D AlertCollider;
    public GameObject SpriteObject;
    public GameObject GunHolder;

    public Transform Target;
	public Vector3 TargetPosition;
	public float TargetDistance;
	public float NextMoveTimer;
	Rigidbody2D Rigidbody2d;
	Vector2 _beforePauseVelocity;
    Animator SpriteAnimator;
	AudioSource AudioPlay;

	public AudioClip ShootSfx;
	public AudioClip HurtSfx;

	public AudioClip[] PainSounds;
	public AudioClip[] DeathSounds;


	public int MindState; // 0- ZzZZzzz 1- awake, no target 2- alert, 3- stunned has target, follow&attack
	public int EnemyType; // 0-melee/runner, 2- ranged, 4- suicidal/dash, 5- shotgun/flamer, 6- sniper/laser, 7- spawner/mother
	bool isDead;
	bool isLoodDropped;
	bool isExploded;

	public Transform Canvas;
	public GameObject HealthbarPrefab;
	public Healthbar Healthbar;
	public float HealthbarTimer;
	public GameObject ShoutTextPrefab;
	public Text ShoutText;
	public float ShoutTextTimer;
	public float NextShoutTimer;

	public float MaxStunTimer = 1.25f;
	public float StunTimer;
	bool isStunned;

	public GameObject BulletPrefab;
	public GameObject BloodPrefab;
	public GameObject ExplosionPrefab;

	// Use this for initialization
	void Start()
	{
		Rigidbody2d = GetComponent<Rigidbody2D>();
        SpriteAnimator = SpriteObject.GetComponent<Animator>();
		AudioPlay = GetComponent<AudioSource>();

		Canvas = GameObject.Find("Canvas").transform;
		GameObject goHealthbar = Instantiate(HealthbarPrefab);
		Healthbar = goHealthbar.GetComponent<Healthbar>();
		//Healthbar.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
		Healthbar.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(0, 20f, 0f);
		Healthbar.transform.SetParent(Canvas);


		GameObject goShoutText = Instantiate(ShoutTextPrefab);
		ShoutText = goShoutText.GetComponent<Text>();
		ShoutText.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(0, 25f, 0f);
		ShoutText.transform.SetParent(Canvas);
		ShoutText.gameObject.SetActive(false);
		NextShoutTimer = Random.Range(1f,5f);

		CurHitPoints = MaxHitPoints;
		Healthbar.MaxValue = MaxHitPoints;
		Healthbar.Value = CurHitPoints;
		Healthbar.gameObject.SetActive(false);

		isDead = false;
		isLoodDropped = false;
		isExploded = false;
		isStunned = false;
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if (MindState < 2)
		{
			if (col.gameObject.tag == "Player")
			{
				MindState = 2;
				Target = col.transform;
			}
		}
	}

	void Update()
	{
		if (GameManager.instance.IsGamePaused)
		{
			return;
		}

		if (isDead) return;

		if (CurHitPoints <= 0f)
		{
			Die();
		}

		if (Time.fixedTime < ShoutTextTimer)
		{
			if (ShoutText != null)
			{
				ShoutText.gameObject.SetActive(true);
				ShoutText.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(0, 15f, 0f);
			}
		}
		else
		{
			if (ShoutText != null)
			{
				ShoutText.gameObject.SetActive(false);
			}
		}
		
		if (Time.fixedTime < HealthbarTimer)
		{
			if (Healthbar != null)
			{
				Healthbar.gameObject.SetActive(true);
				Healthbar.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(transform.position) + new Vector3(0, 20f, 0f);
				Healthbar.Value = CurHitPoints;
				Healthbar.ChangeHealth();
			}
		}
		else
		{
			if (Healthbar != null)
			{
				Healthbar.gameObject.SetActive(false);
			}
		}
	}

	void Die()
	{
		int sfxId = Random.Range(0, DeathSounds.Length);
		/*
		if (!AudioPlay.isPlaying)
			AudioPlay.PlayOneShot();
		*/
		GameObject deathSounder = new GameObject();
		var deathSfx = deathSounder.AddComponent<AudioSource>();
		deathSfx.clip = DeathSounds[sfxId];
		deathSfx.Play();
		Destroy(deathSounder, 2.5f);

		if (!isExploded)
		{
			isExploded = true;
			GameObject.Instantiate(GameManager.instance.ExplosionPrefab, this.transform.position, Quaternion.identity);
		}

		GameManager.instance.Enemies.Remove(this);
		if (Healthbar != null)
			Destroy(this.Healthbar.gameObject);
		if (ShoutText != null)
			Destroy(this.ShoutText.gameObject);

		//instantiate a game object with audiosource, attach soundclip to that then destroy it after 2.5f seconds.
		GameManager.instance.AddToKillChain(1);
		GameManager.instance.KillCount += 1;
		isDead = true;
		Destroy(this.gameObject, 0.1f);
		DropLoot();
	}

	public void Shout(string msg)
	{
		ShoutText.text = msg;
		ShoutTextTimer = Time.fixedTime + 1f;
		NextShoutTimer = Random.Range(3f, 5f);
	}

	public void GotHit(float damage)
	{
		if (Target == null)
		{
			Target = GameManager.instance.PlayerOne.transform;
			TargetPosition = Target.position;
			MindState = 2;
		}

		int sfxId = Random.Range(0, PainSounds.Length);
		AudioPlay.PlayOneShot(PainSounds[sfxId]);

		CurHitPoints -= damage;
		HealthbarTimer = Time.fixedTime + 1f;
		//GameManager.instance.PauseAndResume(0.015f);
        GameManager.instance.ShakeIt();
		Stun(damage);
	}

	public void Stun(float damage)
	{
		isStunned = true;
		MindState = 3;
		StunTimer = damage / 200f;
		if (StunTimer > MaxStunTimer) StunTimer = MaxStunTimer;
	}

	public void Pause(bool value)
	{
		SpriteAnimator.enabled = !value;
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

	public void DrawLaserLine()
	{
		LineRenderer lr = GetComponent<LineRenderer>();
		Vector2 pos = Target.transform.position;
		Vector2 dir = Target.transform.position - transform.position;
		RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, Mathf.Infinity);

		foreach (var hit in hits)
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

		lr.sortingOrder = 1;
		lr.startWidth = 0.05f;
		lr.endWidth = 0.05f;
		lr.SetPosition(0, transform.position);
		lr.SetPosition(1, pos);
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (GameManager.instance.IsGamePaused)
		{
			return;
		}

		if (isDead) return;

		if (MindState == 3)
		{
			if (StunTimer > 0f)
			{
				StunTimer -= Time.fixedDeltaTime;
			}
			else
			{
				isStunned = false;
				MindState = 2;
			}
		}
		else if (MindState == 2)
		{
			if (NextShoutTimer > 0f)
			{
				NextShoutTimer -= Time.fixedDeltaTime;
			}
			else
			{
				if (Random.Range(0, 100) > 90)
				{
					Shout("Yous a dead man!");
				}
				else if (Random.Range(0, 100) > 80)
				{
					Shout("Get here you git!");
				}
				else if (Random.Range(0, 100) > 70)
				{
					Shout("Imma gonna kill ya!");
				}
			}
			DoAI();

			if (Vector2.Distance(transform.position, Target.position) > 8f)
			{
				MindState = 1;
				Target = null;
				TargetPosition = Vector3.zero;
				Rigidbody2d.velocity = Vector2.zero;
			}

			if (EnemyType == 4)
			{
				if (Target != null)
				{
					
					DrawLaserLine();
				}
				else
				{
					LineRenderer lr = GetComponent<LineRenderer>();
					lr.SetPosition(0, Vector3.zero);
					lr.SetPosition(1, Vector3.zero);
				}
					
			}

            if (SpriteObject != null)
                SpriteObject.transform.rotation = Quaternion.identity;

            if (GunHolder != null && Target != null)
            {
                Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
                Vector3 playerPos = Camera.main.WorldToScreenPoint(Target.position);
                Vector3 dir = playerPos - objectPos;
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
            }
		}
	}

    private void LateUpdate()
    {
		if (GameManager.instance.IsGamePaused)
		{
			return;
		}

		if (Rigidbody2d.velocity.magnitude != 0f)
        {
            SpriteAnimator.SetBool("isWalking", true);
        }
        else
        {
            SpriteAnimator.SetBool("isWalking", false);
        }
    }

    void DoAI()
	{
		if (EnemyType == 0) // runner
		{
			Rigidbody2d.rotation = 270f + (Mathf.Atan2((Target.position.y - transform.position.y), (Target.position.x - transform.position.x)) * Mathf.Rad2Deg);
			Vector2 diff = (transform.position - Target.position);
			diff = diff.normalized;
			Rigidbody2d.velocity = -diff * MoveSpeed * Time.fixedDeltaTime;
		}
		else if (EnemyType == 1) //suicide bomber
		{
			/*
			if (TargetPosition == Vector3.zero)
			{
				TargetPosition = Target.position;
				Rigidbody2d.rotation = 270f + (Mathf.Atan2((TargetPosition.y - transform.position.y), (TargetPosition.x - transform.position.x)) * Mathf.Rad2Deg);
			}

			Vector2 diff = (transform.position - TargetPosition);
			diff = diff.normalized;
			Rigidbody2d.velocity = -diff * MoveSpeed * 10f * Time.fixedDeltaTime;
			*/

			Rigidbody2d.rotation = 270f + (Mathf.Atan2((Target.position.y - transform.position.y), (Target.position.x - transform.position.x)) * Mathf.Rad2Deg);
			Vector2 diff = (transform.position - Target.position);
			diff = diff.normalized;
			Rigidbody2d.velocity = -diff * MoveSpeed * Time.fixedDeltaTime;

			if (Vector3.Distance(transform.position, Target.position) < 0.65f)
			{
				Explode();
			}
		}
		else if (EnemyType == 2 || EnemyType == 3 || EnemyType == 4) //shooters
		{
			Rigidbody2d.rotation = 270f + (Mathf.Atan2((Target.position.y - transform.position.y), (Target.position.x - transform.position.x)) * Mathf.Rad2Deg);
			if (Vector2.Distance(transform.position, Target.position) > TargetDistance)
			{
				Vector2 diff = (transform.position - Target.position);
				diff = diff.normalized;
				Rigidbody2d.velocity = -diff * MoveSpeed * Time.fixedDeltaTime;
			}
			else
			{
				Rigidbody2d.velocity = Vector2.zero;
				Shoot();
			}
		}

	}

	void Explode()
	{
		GameObject bam = GameObject.Instantiate(ExplosionPrefab, transform.position, Quaternion.Euler(new Vector3(0f, 0f, 180f) + this.transform.rotation.eulerAngles));
		AudioPlay.PlayOneShot(ShootSfx);
		MindState = 0;
		Die();
	}

	void Shoot()
	{
		//Debug.Log(this.name + " spotted player at distance of: " + Vector2.Distance(transform.position, Target.position).ToString());

		if ((Time.fixedTime - RateOfFireTimer) > RateOfFire)
		{

			if (EnemyType == 2)
				FireBullet();
			else if (EnemyType == 3)
				FireShell();
			else if (EnemyType == 4)
				FireShot();

			RateOfFireTimer = Time.fixedTime;
		}

	}

	public void FireShell()
	{
		for (int i = 0; i < 3; i++)
		{
			GameObject bullet = (GameObject)Instantiate(BulletPrefab, transform.position + (transform.up * 1f), transform.rotation);
			bullet.transform.Rotate(0f, 0f, Random.Range(-BulletSpread, BulletSpread) * Mathf.Rad2Deg);
			bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * BulletSpeed;
			bullet.GetComponent<Bullet>().dieTimer = 0.3f;
		}

		AudioPlay.PlayOneShot(ShootSfx);
		Rigidbody2d.AddForce(-transform.up * WeaponPushback, ForceMode2D.Impulse);
	}

	public void FireBullet()
	{
		GameObject bullet = (GameObject)Instantiate(BulletPrefab, transform.position + (transform.up * 1f), transform.rotation);
		bullet.transform.Rotate(0f, 0f, Random.Range(-BulletSpread, BulletSpread) * Mathf.Rad2Deg);

		bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * BulletSpeed;
		bullet.GetComponent<Bullet>().dieTimer = 0.6f;

		/*
		//audio.PlayOneShot(clip);
		GetComponent<AudioSource>().PlayOneShot(shootSound);
		*/
		AudioPlay.PlayOneShot(ShootSfx);
		Rigidbody2d.AddForce(-transform.up * WeaponPushback, ForceMode2D.Impulse);
	}

	public void FireShot()
	{
		GameObject bullet = (GameObject)Instantiate(BulletPrefab, transform.position + (transform.up * 1f), transform.rotation);
		bullet.transform.Rotate(0f, 0f, Random.Range(-BulletSpread, BulletSpread) * Mathf.Rad2Deg);

		bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * BulletSpeed;
		bullet.GetComponent<Bullet>().dieTimer = 2f;
		bullet.GetComponent<Bullet>().WeaponDamage = 20f;

		/*
		//audio.PlayOneShot(clip);
		GetComponent<AudioSource>().PlayOneShot(shootSound);
		*/
		AudioPlay.PlayOneShot(ShootSfx);
		Rigidbody2d.AddForce(-transform.up * WeaponPushback, ForceMode2D.Impulse);
	}

	void OnCollisionStay2D(Collision2D col)
	{
		if (col.gameObject.tag == "Player")
		{
			if (EnemyType != 0) return;

			if ((Time.fixedTime - RateOfFireTimer) > RateOfFire)
			{
				GameObject go = GameObject.Instantiate(BloodPrefab, col.gameObject.transform.position, Quaternion.Euler(-this.transform.rotation.eulerAngles));
				col.gameObject.GetComponent<PlayerController>().GotHurt(WeaponDamage);
				RateOfFireTimer = Time.fixedTime;
			}
		}
	}

	void DropLoot()
	{
		if (isLoodDropped) return;

		int prob = Random.Range(0, 100);

		if (prob > 90)
		{
			//drop weapon
			int randId = Random.Range(0, Mathf.Min(6, (GameManager.instance.Level / 2) + 1));
			GameObject go = GameObject.Instantiate(GameManager.instance.LootPrefabs[5 + randId], transform.position, Quaternion.identity);
		}
		else if (prob > 65)
		{
			//drop healthpack
			GameObject go = GameObject.Instantiate(GameManager.instance.LootPrefabs[0], transform.position, Quaternion.identity);
		}
		else if (prob > 40)
		{
			//drop ammo
			int randId = Random.Range(0, Mathf.Min(4, (GameManager.instance.Level / 2)));
			GameObject go = GameObject.Instantiate(GameManager.instance.LootPrefabs[1 + randId], transform.position, Quaternion.identity);
		}

		isLoodDropped = true;
	}

}
