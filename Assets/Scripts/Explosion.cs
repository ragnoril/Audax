using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	public AudioClip[] ExplosionSounds;

	Animator Anim;
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

		Anim = GetComponent<Animator>();
		endTimer = Anim.GetCurrentAnimatorStateInfo(0).length;
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
}
