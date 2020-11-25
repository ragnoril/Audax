using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

	private static GameManager _instance;

	public static GameManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<GameManager>();
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

	public PlayerController PlayerOne;
	public ExitAgent LevelExit;
	//public PlayerController PlayerTwo;
	public int PlayerCount;
	public MapGenerator MapGen;
	public int Level;

	public GameObject[] LootPrefabs;
	public GameObject ExplosionPrefab;
	public GameObject[] EnemyPrefabs;
	public GameObject[] BreakablePrefabs;
	public Texture2D reticle;

	public List<EnemyAgent> Enemies;
	public List<Breakable> Breakables;

	private float SpawnTimer;
	private float LastSpawnTime;
	private bool isGameOver;
	public bool IsGamePaused;
	public bool IsGameSloMo;

	public Image BloodScreen;
	public GameObject UpgradePanel;
	public GameObject DeathPanel;
	public GameObject EndPanel;
	public UpgradeUI UpgradeUI;
	public RawImage Minimap;
	public Text PauseIcon;
	public Text KillCountText;


	public float EnemyHealthPenalty;
	public float EnemyMovePenalty;
	public float EnemyBulletSpeedPenalty;
	public int CollectibleDropBonus;

	public int KillCount;
	public int ShotCount;
	public int HitCount;
	public float TimeCount;


	// Use this for initialization
	void Start ()
	{
		MapGen = FindObjectOfType<MapGenerator>();
		PlayerOne = FindObjectOfType<PlayerController>();
		Enemies = new List<EnemyAgent>();
		Breakables = new List<Breakable>();
		Vector2 cursorHotspot = new Vector2(reticle.width / 2, reticle.height / 2);
		Cursor.SetCursor(reticle, cursorHotspot, CursorMode.Auto);
		SpawnTimer = 30f;
		StartCoroutine(BlackoutScreen(false));
		//MapGen.Generate();
		//SetPlayerPosition();
		Level = 0;

		EnemyHealthPenalty = 0;
		EnemyMovePenalty = 0;
		EnemyBulletSpeedPenalty = 0;
		CollectibleDropBonus = 0;

		IsGamePaused = false;
		IsGameSloMo = false;

		KillCount = 0;
		ShotCount = 0;
		HitCount = 0;
		TimeCount = 0f;
	}

	public void ClearGameData()
	{
		foreach (var enemy in Enemies)
		{
			Debug.Log(enemy.name + " destroyed");

			if (enemy.Healthbar != null)
				Destroy(enemy.Healthbar.gameObject);
			if (enemy.ShoutText != null)
				Destroy(enemy.ShoutText.gameObject);

			GameObject.Destroy(enemy.gameObject);
		}

		foreach (var breakable in Breakables)
		{
			Debug.Log(breakable.name + " destroyed");
			GameObject.Destroy(breakable.gameObject);
		}

		ClearHealthBars();
		ClearCollectibles();

		Enemies.Clear();
		Breakables.Clear();

		IsGameSloMo = false;
	}

	public void ClearGameStats()
	{
		EnemyHealthPenalty = 0;
		EnemyMovePenalty = 0;
		EnemyBulletSpeedPenalty = 0;
		CollectibleDropBonus = 0;

		KillCount = 0;
		ShotCount = 0;
		HitCount = 0;
		TimeCount = 0f;
	}

	public void StartNewLevel()
	{
		MapGen.Regenerate();
		SetPlayerPosition();

		Minimap.texture = MapGen.MinimapTexture;
		MusicEngine.instance.ResetMusic();
	}

	public void SetPlayerPosition()
	{
		PlayerOne.transform.position = MapGen.StartPosition;

		int enemyCount = 20 + (Level * 5);

		for (int i = 0; i < enemyCount; i++)
		{
			SpawnEnemy();
		}

		int breakableCount = 5 + (Level * 5);

		for (int i = 0; i < breakableCount; i++)
		{
			SpawnBarrel();
		}

		SpawnCrates(breakableCount);
	}

	public Texture2D MinimapTex = null;

	public void GenerateMinimap(int _x = 21, int _y = 21)
	{
		int width = (MapGen.MapWidth * MapGen.fragmentSize);
		int height = (MapGen.MapHeight * MapGen.fragmentSize);

		int playerX = (int)PlayerOne.transform.position.x;
		int playerY = -(int)PlayerOne.transform.position.y;

		int startX = playerX - 10;
		int endX = playerX + 11;

		if (startX < 0)
		{
			endX -= startX;
			startX = 0;
		}
		if (endX >= width)
		{
			startX -= (endX - width - 1);
			endX = width;
		}

		int startY = playerY - 10;
		int endY = playerY + 11;
		
		if (startY < 0)
		{
			endY -= startY;
			startY = 0;
		}
		if (endY >= width)
		{
			startY -= (endY - height);
			endY = height;
		}
		
		int playerPosX = playerX - startX;
		int playerPosY = playerY - startY;

		int x = 0;
		int y = 0;

		MinimapTex = new Texture2D(_x, _y, TextureFormat.ARGB32, false);
		for (int i = startX; i < endX; i++)
		{
			for (int j = startY; j < endY; j++)
			{
				if (MapGen.TileMap[i + (j * (width))] == -1)
				{
					MinimapTex.SetPixel(x, y, Color.clear);
				}
				else if (MapGen.TileMap[i + (j * (width))] < 16)
				{
					MinimapTex.SetPixel(x, y, Color.black);
				}
				else
				{
					MinimapTex.SetPixel(x, y, Color.white);
				}

				y += 1;
			}
			x += 1;
		}

		MinimapTex.SetPixel(playerPosX, playerPosY, Color.red);
		MinimapTex.Apply();
		Minimap.texture = MinimapTex;
	}

	public void ShowMinimap()
	{
		GenerateMinimap();
		Minimap.gameObject.SetActive(true);
	}

	public void HideMinimap()
	{
		Minimap.gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update ()
	{
		/*
		if (Minimap.texture == null)
		{
			Minimap.texture = MapGen.MinimapTexture;
		}
		*/

		if (!isGameOver && !IsGamePaused)
		{
			TimeCount += Time.deltaTime;
		}
		
		if (Input.GetKeyDown(KeyCode.P))
		{
			PauseGame(!IsGamePaused);
		}

		if (Input.GetKeyDown(KeyCode.M))
		{
			IsGameSloMo = !IsGameSloMo;
		}

		if (IsGameSloMo)
			Time.timeScale = 0.3f;
		else
			Time.timeScale = 1f;

		if ((Time.fixedTime - SpawnTimer) > LastSpawnTime && IsGamePaused)
		{
			SpawnEnemy();

			SpawnTimer = Time.fixedTime + 30f;
		}

		KillCountText.text = _killChainCount.ToString() + " / " + KillCount.ToString();

		_killChainTimer -= Time.deltaTime;
		if (_killChainTimer <= 0f)
		{
			_killChainCount = 0;
		}

		/*
		if (isGameOver && Input.anyKey)
		{
			isGameOver = false;
			//SceneManager.LoadScene("menuScene");
			ClearGameData();
			PlayerOne.gameObject.SetActive(true);
			PlayerOne.ResetPlayer();
			Level = 0;
			DeathPanel.SetActive(false);
			PauseGame(false);
			StartNewLevel();
		}
		*/

	}

	void ClearHealthBars()
	{
		Transform Canvas = GameObject.Find("Canvas").transform;
		for (int i = 0; i < Canvas.childCount; i++)
		{
			if (Canvas.GetChild(i).name.StartsWith("Health"))
			{
				Destroy(Canvas.GetChild(i).gameObject);
			}
		}
	}

	void ClearCollectibles()
	{
		Collectible[] collectibles = GameObject.FindObjectsOfType<Collectible>();

		foreach(var collectible in collectibles)
		{
			Destroy(collectible.gameObject);
		}
	}

	void SpawnEnemy()
	{
		int _x = Random.Range(0, (MapGen.MapWidth * MapGen.fragmentSize) - 1);
		int _y = Random.Range(0, (MapGen.MapHeight * MapGen.fragmentSize) - 1);

		int counter = 0;
		while ((MapGen.TileMap[_x + (_y * (MapGen.MapWidth * MapGen.fragmentSize))] < 16) || (Vector2.Distance(new Vector2(_x, -_y), MapGen.StartPosition) < 7f))
        {
			counter++;
			if (counter > 10000)
				return;
			
			_x = Random.Range(0, MapGen.MapWidth * MapGen.fragmentSize);
			_y = Random.Range(0, MapGen.MapHeight * MapGen.fragmentSize);
		}

		int upper = System.Math.Min(EnemyPrefabs.Length, 2 + Level / 2);
		GameObject goEnemy = GameObject.Instantiate(EnemyPrefabs[Random.Range(0, upper)], new Vector3(_x, -_y, 0f), Quaternion.identity);
		EnemyAgent agent = goEnemy.GetComponent<EnemyAgent>();
		agent.MaxHitPoints -= EnemyHealthPenalty;
		agent.CurHitPoints = agent.MaxHitPoints;
		agent.MoveSpeed -= EnemyMovePenalty;
		agent.BulletSpeed -= EnemyBulletSpeedPenalty;
		
		Enemies.Add(agent);

	}

	void SpawnBarrel()
	{
		int _x = Random.Range(0, (MapGen.MapWidth * MapGen.fragmentSize) - 1);
		int _y = Random.Range(0, (MapGen.MapHeight * MapGen.fragmentSize) - 1);

		int counter = 0;
		while ((MapGen.TileMap[_x + (_y * (MapGen.MapWidth * MapGen.fragmentSize))] < 16) || (Vector2.Distance(new Vector2(_x, -_y), MapGen.StartPosition) < 3f))
		{
			counter++;
			if (counter > 10000)
				return;

			_x = Random.Range(0, MapGen.MapWidth * MapGen.fragmentSize);
			_y = Random.Range(0, MapGen.MapHeight * MapGen.fragmentSize);
		}
		
		GameObject goBreak = GameObject.Instantiate(BreakablePrefabs[0], new Vector3(_x, -_y, 0f), Quaternion.identity);
		Breakable breakable = goBreak.GetComponent<Breakable>();

		Breakables.Add(breakable);

	}

	void SpawnCrates(int count)
	{
		for (int i = 0; i < count; i++)
		{
			int _x = Random.Range(0, (MapGen.MapWidth * MapGen.fragmentSize) - 1);
			int _y = Random.Range(0, (MapGen.MapHeight * MapGen.fragmentSize) - 1);

			int counter = 0;
			while ((MapGen.TileMap[_x + (_y * (MapGen.MapWidth * MapGen.fragmentSize))] < 16) || (Vector2.Distance(new Vector2(_x, -_y), MapGen.StartPosition) < 3f))
			{
				counter++;
				if (counter > 10000)
					return;

				_x = Random.Range(0, MapGen.MapWidth * MapGen.fragmentSize);
				_y = Random.Range(0, MapGen.MapHeight * MapGen.fragmentSize);
			}

			PutCrate(BreakablePrefabs[1], _x, _y, 8);
		}
	}

	void PutCrate(GameObject prefab, int x, int y, int prob)
	{
		GameObject goBreak = GameObject.Instantiate(prefab, new Vector3(x, -y, 0f), Quaternion.identity);
		Breakable breakable = goBreak.GetComponent<Breakable>();

		Breakables.Add(breakable);

		// north
		if (prob > Random.Range(0, 10))
		{
			if (MapGen.TileMap[x + ((y - 1) * (MapGen.MapWidth * MapGen.fragmentSize))] > 15)
				PutCrate(BreakablePrefabs[1], x, y - 1, prob/2);
		}

		// west
		if (prob > Random.Range(0, 10))
		{
			if (MapGen.TileMap[(x - 1) + (y * (MapGen.MapWidth * MapGen.fragmentSize))] > 15)
				PutCrate(BreakablePrefabs[1], x - 1, y, prob / 2);
		}

		// south
		if (prob > Random.Range(0, 10))
		{
			if (MapGen.TileMap[x + ((y + 1) * (MapGen.MapWidth * MapGen.fragmentSize))] > 15)
				PutCrate(BreakablePrefabs[1], x, y + 1, prob / 2);
		}

		// east
		if (prob > Random.Range(0, 10))
		{
			if (MapGen.TileMap[(x + 1) + (y * (MapGen.MapWidth * MapGen.fragmentSize))] > 15)
				PutCrate(BreakablePrefabs[1], x + 1, y, prob / 2);
		}
	}

	IEnumerator Shake()
	{

		float elapsed = 0.0f;
		float duration = 0.5f;

		Vector3 originalCamPos = Camera.main.transform.position;
		float magnitude = 0.035f;


		while (elapsed < duration)
		{

			elapsed += Time.deltaTime;

			float percentComplete = elapsed / duration;
			float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

			// map value to [-1, 1]
			float x = Random.value * 2.0f - 1.0f;
			float y = Random.value * 2.0f - 1.0f;
			x *= magnitude * damper;
			y *= magnitude * damper;

			Camera.main.transform.position += new Vector3(x, y, 0f);

			yield return null;
		}

		//Camera.main.transform.position = originalCamPos;
	}

	public void ShakeIt()
	{
		StartCoroutine(Shake());
	}


	float _freezeTimer = 0;
	public void PauseAndResume(float timer)
	{
		/*
		Time.timeScale = 0;
		//Display Image here
		StartCoroutine(ResumeAfterNSeconds(timer));
		*/
		StartCoroutine(ResumeAfterNSeconds(timer));
	}

	IEnumerator ResumeAfterNSeconds(float timePeriod)
	{
		_freezeTimer = timePeriod;
		PauseGame(true);
		while (_freezeTimer > 0f)
		{
			_freezeTimer -= Time.unscaledDeltaTime;
			
			yield return null;
		}
		PauseGame(false);
	}

	private IEnumerator Pause(float p)
	{
		Time.timeScale = 0.001f;
		float pauseEndTime = Time.realtimeSinceStartup + p;
		while (Time.realtimeSinceStartup < pauseEndTime)
		{
			yield return 0;
		}
		Time.timeScale = 1;
	}

	public void BloodOnScreen(float bloodTiming = 0.5f)
	{
		Color col = BloodScreen.color;
		col.a = 0.65f;
		BloodScreen.color = col;
		StartCoroutine(BloodSplatting(bloodTiming));
	}

	IEnumerator BloodSplatting(float timePeriod)
	{
		Color col = BloodScreen.color;

		while (col.a > 0f)
		{
			col.a -= 0.05f;
			BloodScreen.color = col;

			yield return null;
		}
	}

	IEnumerator BlackoutScreen(bool mode)
	{
		SimpleBlit CamBlit = Camera.main.GetComponent<SimpleBlit>();
		Material CutOffMat = CamBlit.TransitionMaterial;

		float cutoff = CutOffMat.GetFloat("_Cutoff");

		if (mode)
		{
			while (cutoff < 1f)
			{
				cutoff += 0.05f;
				CutOffMat.SetFloat("_Cutoff", cutoff);

				yield return null;
			}
		}
		else
		{
			while (cutoff > 0f)
			{
				cutoff -= 0.05f;
				CutOffMat.SetFloat("_Cutoff", cutoff);

				yield return null;
			}
		}
	}
	
	public void UpgradeScreen()
	{
		if (Level == 10)
		{
			EndGameScreen();
			return;
		}

		StartCoroutine(BlackoutScreen(true));
		PlayerOne.gameObject.SetActive(false);
		UpgradePanel.SetActive(true);
		UpgradeUI.SetupUpgrades();
		//ClearGameData();
		PauseGame(true);
		//Time.timeScale = 0;

	}

	public void EndGameScreen()
	{
		//StartCoroutine(BlackoutScreen(true));
		//PlayerOne.gameObject.SetActive(false);
		EndPanel.SetActive(true);
		//ClearGameData();
		PauseGame(true);
		//Time.timeScale = 0;
	}

	public void DeathScreen()
	{
		PlayerOne.gameObject.SetActive(false);
		//Time.timeScale = 0;
		isGameOver = true;
		//StartCoroutine(BlackoutScreen(true));
		PauseGame(true);
		DeathPanel.SetActive(true);
		//ClearGameData();
		PauseAndResume(1f);
	}

	public void PauseGame(bool value)
	{
		if (value)
			Cursor.SetCursor(null, Vector2.one, CursorMode.Auto);
		else
			Cursor.SetCursor(reticle, new Vector2(reticle.width / 2, reticle.height / 2), CursorMode.Auto);


		if (value)
			Time.timeScale = 0f;
		else
			Time.timeScale = 1f;

		IsGamePaused = value;

		PlayerOne.Pause(value);
		LevelExit.Pause(value);

		foreach(var enemy in Enemies)
		{
			enemy.Pause(value);
		}

		var bullets = GameObject.FindGameObjectsWithTag("Bullet");

		foreach(var bullet in bullets)
		{
			bullet.GetComponent<Bullet>().Pause(value);
		}

		var explosions = GameObject.FindGameObjectsWithTag("Explosion");

		foreach (var exp in explosions)
		{
			exp.GetComponent<Animator>().enabled = !value;
		}

		PauseIcon.gameObject.SetActive(value);
		//ToDo: Pause the music and sound effects if needed.
	}

	public void ContinueGame()
	{
		PlayerOne.gameObject.SetActive(true);
		StartCoroutine(BlackoutScreen(false));
		//Time.timeScale = 1;
		Level += 1;
		UpgradePanel.SetActive(false);
		PauseGame(false);
		ClearGameData();
		StartNewLevel();
	}

	public void StartAgain()
	{
		isGameOver = false;
		//SceneManager.LoadScene("menuScene");
		ClearGameData();
		ClearGameStats();
		PlayerOne.gameObject.SetActive(true);
		PlayerOne.ResetPlayer();
		Level = 0;
		DeathPanel.SetActive(false);
		EndPanel.SetActive(false);
		PauseGame(false);
		StartNewLevel();
	}

	float _killChainTimer = 0f;
	int _killChainCount = 0;
	public void AddToKillChain(int val)
	{
		_killChainCount += val;
		_killChainTimer = (2f * val);
	}

}
