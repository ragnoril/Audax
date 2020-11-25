using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public GameObject BloodPrefab;
	public GameObject SmokePrefab;
	public GameObject ExplosionPrefab;
	public int BulletType;
	public float WeaponDamage;
	public Transform Owner;

	public AudioClip[] RicochetSounds;
	AudioSource AudioPlay;

	Vector2 _beforePauseVelocity;
	Rigidbody2D Rigidbody2d;

	public float dieTimer;
	// Use this for initialization
	void Start ()
	{
		AudioPlay = GetComponent<AudioSource>();
		Rigidbody2d = GetComponent<Rigidbody2D>();
	}
	
	void Update()
	{
		if (GameManager.instance.IsGamePaused)
		{
			return;
		}

		dieTimer -= Time.deltaTime;

		if (dieTimer < 0f)
		{
			Destroy(this.gameObject);
		}
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		Rigidbody2d.angularVelocity = 0f;
    }


	public void Pause(bool value)
	{
		if (value)
		{
			if (Rigidbody2d != null)
			{
				_beforePauseVelocity = Rigidbody2d.velocity;
				Rigidbody2d.Sleep();
			}
		}
		else
		{
			if (Rigidbody2d != null)
			{
				Rigidbody2d.WakeUp();
				Rigidbody2d.velocity = _beforePauseVelocity;
			}
		}
	}


	void OnCollisionEnter2D(Collision2D col)
	{
		Debug.Log(col.gameObject.name);

		if (col.transform == Owner) return;

		if (col.gameObject.tag == "Enemy")
		{
			//GameManager.instance.PauseAndResume(0.05f);
			Vector3 pos = col.contacts[0].point;
			GameObject go = GameObject.Instantiate(BloodPrefab, pos, Quaternion.Euler(new Vector3(0f, 0f, 180f) + this.transform.rotation.eulerAngles));
			col.gameObject.GetComponent<Rigidbody2D>().AddForce(transform.up * 5f, ForceMode2D.Impulse);
			col.gameObject.GetComponent<EnemyAgent>().GotHit(WeaponDamage);

			if (GameManager.instance.PlayerOne.Vampirism)
			{
				if (Owner == GameManager.instance.PlayerOne.transform)
				{
					if (col.gameObject.GetComponent<EnemyAgent>().CurHitPoints <= 0)
					{
						GameManager.instance.PlayerOne.CurHitPoints += WeaponDamage;
					}
				}
			}

			if (BulletType == 1)
			{
				GameObject bam = GameObject.Instantiate(ExplosionPrefab, transform.position, Quaternion.Euler(new Vector3(0f, 0f, 180f) + this.transform.rotation.eulerAngles));
			}

			Destroy(this.gameObject);
		}

		if (col.gameObject.tag == "Player")
		{
			Vector3 pos = col.contacts[0].point;
			GameObject go = GameObject.Instantiate(BloodPrefab, pos, Quaternion.Euler(new Vector3(0f, 0f, 180f) + this.transform.rotation.eulerAngles));
			col.gameObject.GetComponent<PlayerController>().GotHurt(WeaponDamage);

			if (BulletType == 1)
			{
				GameObject bam = GameObject.Instantiate(ExplosionPrefab, transform.position, Quaternion.Euler(new Vector3(0f, 0f, 180f) + this.transform.rotation.eulerAngles));
			}

			Destroy(this.gameObject);
		}
		
		if (col.gameObject.tag == "Walls")
		{
			Vector3 pos = col.contacts[0].point;
			GameObject go = GameObject.Instantiate(SmokePrefab, pos, Quaternion.Euler(new Vector3(0f, 0f, 180f) + this.transform.rotation.eulerAngles));

			int sfxId = Random.Range(0, RicochetSounds.Length);
			/*
			if (!AudioPlay.isPlaying)
				AudioPlay.PlayOneShot();
			*/
			GameObject deathSounder = new GameObject();
			var deathSfx = deathSounder.AddComponent<AudioSource>();
			deathSfx.clip = RicochetSounds[sfxId];
			deathSfx.Play();
			Destroy(deathSounder, 2.5f);

			if (BulletType == 1)
			{
				GameObject bam = GameObject.Instantiate(ExplosionPrefab, transform.position, Quaternion.Euler(new Vector3(0f, 0f, 180f) + this.transform.rotation.eulerAngles));
			}
			/*
			this.GetComponent<Renderer>().enabled = false;
			this.GetComponent<Collider2D>().enabled = false;
			*/
			Destroy(this.gameObject, 0.1f);
		}

		if (col.gameObject.tag == "Bullet")
		{
			int sfxId = Random.Range(0, RicochetSounds.Length);
			GameObject deathSounder = new GameObject();
			var deathSfx = deathSounder.AddComponent<AudioSource>();
			deathSfx.clip = RicochetSounds[sfxId];
			deathSfx.Play();
			Destroy(deathSounder, 2.5f);
		}

		if (col.gameObject.tag == "Breakable")
		{
			if (BulletType == 1)
			{
				GameObject bam = GameObject.Instantiate(ExplosionPrefab, transform.position, Quaternion.Euler(new Vector3(0f, 0f, 180f) + this.transform.rotation.eulerAngles));
			}

			Destroy(this.gameObject);
		}

	}
}
