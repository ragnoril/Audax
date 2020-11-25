using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionAgent : MonoBehaviour
{

	//List<GameObject> Damaged = new List<GameObject>();
	public AudioClip[] ExplosionSounds;
	public float endTimer;

	// Use this for initialization
	void Start ()
	{
		int sfxId = Random.Range(0, ExplosionSounds.Length);
		GameObject deathSounder = new GameObject();
		var deathSfx = deathSounder.AddComponent<AudioSource>();
		deathSfx.clip = ExplosionSounds[sfxId];
		deathSfx.Play();
		Destroy(deathSounder, 2.5f);

		endTimer = 0.5f;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (GameManager.instance.IsGamePaused)
		{
			return;
		}

		endTimer -= Time.deltaTime;

		if (endTimer <= 0f)
			GameObject.Destroy(this.gameObject);
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		/*
		foreach (var damaged in Damaged)
		{
			if (damaged == col.gameObject)
				return;
		}

		Damaged.Add(col.gameObject);
		*/
		if (col.gameObject.tag == "Enemy")
		{
			EnemyAgent enemy = col.gameObject.GetComponent<EnemyAgent>();
			//GameObject go = GameObject.Instantiate(enemy.BloodPrefab, col.gameObject.transform.position, Quaternion.Euler(-this.transform.rotation.eulerAngles));
			enemy.GetComponent<Rigidbody2D>().AddForce(transform.up * 5f, ForceMode2D.Impulse);
			enemy.GotHit(20f);
		}
		else if (col.gameObject.tag == "Player")
		{
			PlayerController player = col.gameObject.GetComponent<PlayerController>();
			//GameObject go = GameObject.Instantiate(player., col.gameObject.transform.position, Quaternion.Euler(-this.transform.rotation.eulerAngles));
			player.GetComponent<Rigidbody2D>().AddForce(-transform.rotation.eulerAngles * 150f, ForceMode2D.Impulse);
			if (player.FireResistance == false) 
				player.GotHurt(20f);
		}
	}
}
