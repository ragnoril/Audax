using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicEngine : MonoBehaviour
{

	private static MusicEngine _instance;

	public static MusicEngine instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<MusicEngine>();
				//DontDestroyOnLoad(_instance.gameObject);
			}

			return _instance;
		}
	}

	void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			//DontDestroyOnLoad(this);
		}
		else
		{
			if (this != _instance)
				Destroy(this.gameObject);
		}
	}


	public float WaitTimer;
	float WaitTick;

	public AudioClip MusicFile;


	AudioSource AudioPlayer;
	// Use this for initialization
	void Start ()
    {
		AudioPlayer = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (AudioPlayer.isPlaying)
		{
			WaitTick = 0f;
			if (AudioPlayer.volume < 1f) AudioPlayer.volume += 0.01f;
		}
		else
		{
			WaitTick += Time.deltaTime;
			AudioPlayer.volume = 0f;

			if (WaitTick > WaitTimer)
			{
				AudioPlayer.PlayOneShot(MusicFile);
			}
		}
		
	}

	public void ResetMusic()
	{
		AudioPlayer.Stop();
		WaitTick = 0f;
	}

}
