using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
	public GameObject ExplosionPrefab;
	public GameObject ExplosionSmallPrefab;
	public int BreakType;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		//Debug.Log(col.gameObject.name);

		if (col.gameObject.tag == "Bullet")
		{
			if (BreakType == 0) // barrel
			{
				Explode();
			}
			else if (BreakType == 1) //crate
			{
				DropLoot();
			}

			GameManager.instance.Breakables.Remove(this);
			Destroy(this.gameObject);
		}

	}

	public void Explode()
	{
		GameObject bam = GameObject.Instantiate(ExplosionPrefab, transform.position, Quaternion.Euler(new Vector3(0f, 0f, 180f) + this.transform.rotation.eulerAngles));
	}
	
	public void DropLoot()
	{
		GameObject bam = GameObject.Instantiate(ExplosionSmallPrefab, transform.position, Quaternion.Euler(new Vector3(0f, 0f, 180f) + this.transform.rotation.eulerAngles));

		//if (isLoodDropped) return;

		int prob = Random.Range(0, 100);

		if (prob > (85 - GameManager.instance.CollectibleDropBonus))
		{
			//drop weapon
			int randId = Random.Range(0, Mathf.Min(6, (GameManager.instance.Level / 2) + 1));
			GameObject go = GameObject.Instantiate(GameManager.instance.LootPrefabs[5 + randId], transform.position, Quaternion.identity);
		}
		else if (prob > (50 - GameManager.instance.CollectibleDropBonus))
		{
			//drop healthpack
			GameObject go = GameObject.Instantiate(GameManager.instance.LootPrefabs[0], transform.position, Quaternion.identity);
		}
		else if (prob > (25 - GameManager.instance.CollectibleDropBonus))
		{
			//drop ammo
			int randId = Random.Range(0, Mathf.Min(4, (GameManager.instance.Level / 2)));
			GameObject go = GameObject.Instantiate(GameManager.instance.LootPrefabs[1 + randId], transform.position, Quaternion.identity);
		}

		//isLoodDropped = true;
	}
}
