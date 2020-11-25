using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;

public class MapGenerator : MonoBehaviour
{
	public List<int> TileMap;
	public int MapWidth;
	public int MapHeight;
	public List<MapFragment> Fragments;
	private int[,] fragmentMap;
	public int fragmentSize;

	public GameObject FloorPrefab;
	public GameObject WallPrefab;
	public GameObject UpPrefab;
	public GameObject DownPrefab;
	public GameObject[] TilePrefabs;

	public Vector2 StartPosition;
	public Vector2 ExitPosition;
	GameObject _ExitObject;

	public int mapSize;
	public int mazeLength;

	public Texture2D MinimapTexture;

	// Use this for initialization
	void Start ()
	{
		//fragmentSize = 9;
		mapSize = 7;
		mazeLength = 5;
		CreateTileMap(mapSize, mapSize);
		//LoadFragments(Application.dataPath + "/../Fragments/");
		LoadFragments(Application.dataPath + "/../Map/wildfyre.tmx");
		while (GenerateMap() != 1)
		{

		}
		RenderMap();
		GenerateMinimap();
		GameManager.instance.SetPlayerPosition();
	}
	
	// Update is called once per frame
	void Update ()
	{
	}

	public void Regenerate()
	{
		if (_ExitObject != null)
			GameObject.Destroy(_ExitObject);


		for(int i = 0; i < transform.childCount; i++)
		{
			GameObject obj = transform.GetChild(i).gameObject;
            Destroy(obj);
		}

		mapSize = 7;
		mazeLength = 5 + (GameManager.instance.Level / 2);

		CreateTileMap(mapSize, mapSize);
		
		while (GenerateMap() != 1)
		{

		}
		RenderMap();
		GenerateMinimap();
	}

	public void GenerateMinimap()
	{
		int width = (MapWidth * fragmentSize);
		int height = (MapHeight * fragmentSize);

		MinimapTexture = new Texture2D(width + 2, height + 2, TextureFormat.ARGB32, false);
		MinimapTexture.filterMode = FilterMode.Point;
		for (int i = 0; i < width; i++)
		{
			for (int j = (height - 1); j > -1; j--)
			{
				if (TileMap[i + (j * (width))] == -1)
				{
					MinimapTexture.SetPixel(i, j, Color.clear);
				}
				else if (TileMap[i + (j * (width))] < 16)
				{
					MinimapTexture.SetPixel(i, j, Color.black);
				}
				else
				{
					MinimapTexture.SetPixel(i, j, Color.white);
				}
			}
		}
		MinimapTexture.Apply();
		
	}

	public void RenderMap()
	{
		Vector3 pos = new Vector3(0, 0, 0);

		for (int i = 0; i < MapWidth * fragmentSize; i++)
		{
			for (int j = 0; j < MapHeight * fragmentSize; j++)
			{
				if (TileMap[i + (j * (MapWidth * fragmentSize))] != -1)
				{
					pos.x = (i * 1f);// + 0.5f;
					pos.z = 0.0f;
					pos.y = -(j * 1f);// + 0.5f;

					GameObject obj = Instantiate(TilePrefabs[TileMap[i + (j * (MapWidth * fragmentSize))]], pos, Quaternion.identity);
					obj.transform.SetParent(transform);
				}
			}
		}

		_ExitObject = GameObject.Instantiate(DownPrefab, new Vector3(ExitPosition.x, ExitPosition.y, 0f), Quaternion.identity);
		GameManager.instance.LevelExit = _ExitObject.GetComponent<ExitAgent>();
	}

	public void LoadFragmentsFromTextFiles(string filePath)
	{
		Fragments = new List<MapFragment>();
		string[] filenames = Directory.GetFiles(filePath, "*.txt");
		foreach(var file in filenames)
		{
			MapFragment fragment = new MapFragment();
			fragment.fragmentSize = fragmentSize;
			fragment.LoadTiles(file);
			Fragments.Add(fragment);
		}
		
	}

	public void LoadFragments(string filePath)
	{
		Fragments = new List<MapFragment>();

		XmlDocument doc = new XmlDocument();

		doc.Load(filePath);
		XmlNode mapNode = doc.SelectSingleNode("map");

		XmlNodeList nodes = mapNode.SelectNodes("layer");

		foreach (XmlNode node in nodes)
		{
			string tiles = node.SelectSingleNode("data").InnerText;
			XmlNodeList exits = node.SelectSingleNode("properties").SelectNodes("property");

			int exitBits = 0;
			foreach (XmlNode exit in exits)
			{
				if (exit.Attributes["value"].Value == "true")
				{
					if (exit.Attributes["name"].Value == "north")
					{
						exitBits |= 1;
					}
					else if (exit.Attributes["name"].Value == "east")
					{
						exitBits |= 2;
					}
					else if (exit.Attributes["name"].Value == "south")
					{
						exitBits |= 4;
					}
					else if (exit.Attributes["name"].Value == "west")
					{
						exitBits |= 8;
					}
				}
			}

			MapFragment fragment = new MapFragment();
			fragment.ExitBits = exitBits;
			fragment.fragmentSize = fragmentSize;
			fragment.LoadTiles(tiles);
			Fragments.Add(fragment);
		}


	}

	private void CreateTileMap(int colCount, int rowCount)
	{
		MapWidth = colCount;
		MapHeight = rowCount;
		for (int i = 0; i < colCount * fragmentSize; i++)
		{
			for (int j = 0; j < rowCount * fragmentSize; j++)
			{
				TileMap.Add(-1);
			}
		}
	}

	public int GenerateMap()
	{
		//Vector2 startPos = new Vector2(Random.Range(0, MapWidth), Random.Range(0, MapHeight));
		List<Vector2> path = new List<Vector2>();
		//int[,] smallMap = new int[MapWidth, MapHeight];
		fragmentMap = new int[MapWidth, MapHeight];

		for (int i = 0; i < MapWidth; i++)
		{
			for (int j = 0; j < MapHeight; j++)
			{
				//smallMap[i, j] = -1;
				fragmentMap[i, j] = -1;
			}
		}

		//smallMap[(int)startPos.x, (int)startPos.y] = -3;

		//int posX = (int)startPos.x;
		//int posY = (int)startPos.y;
		int startX = Random.Range(0, MapWidth);
		int startY = Random.Range(0, MapHeight); 
		int posX = startX;
		int posY = startY;

		StartPosition = new Vector2((posX * fragmentSize), -((posY * fragmentSize)));


		path.Add(new Vector2(posX, posY));

		int counterExit = 0;
		// 0 north, 1 east 2 south 3 west
		int newDir = -1; //= Random.Range(0, 4);
		int prevDir = -1;
		Vector2 lastRoom = Vector2.zero;
		// from start to finish.
		while (mazeLength > 0)
		{
			counterExit++;
			if (counterExit > 100000)
			{
				Debug.Log("counter exit");
				return -1;
			}

			
			bool isPlaced = false;

			int counterExit1 = 0;
			while (!isPlaced)
			{
				counterExit1++;
				if (counterExit1 > 100000)
				{
					Debug.Log("counter1 exit");
					return -1;
				}

				newDir = Random.Range(0, 4);
				if (newDir == 0)  // north
				{
					if ((posY - 1) >= 0)
					{
						if (fragmentMap[posX, posY - 1] == -1)
						{
							int exitBit = 1;
							exitBit = exitBit << newDir;

							if (prevDir != -1)
							{
								int prevBit = 1;
								prevBit = prevBit << prevDir;
								exitBit = exitBit | prevBit;
							}
							
							fragmentMap[posX, posY] = GetFragmentWithExits(exitBit);
							lastRoom = new Vector2(posX, posY);
							isPlaced = true;
							posY -= 1;
							prevDir = 2;
						}
					}
				}
				else if (newDir == 1) // east
				{
					if ((posX + 1) < MapWidth)
					{
						if (fragmentMap[posX + 1, posY] == -1)
						{
							int exitBit = 1;
							exitBit = exitBit << newDir;

							if (prevDir != -1)
							{
								int prevBit = 1;
								prevBit = prevBit << prevDir;
								exitBit = exitBit | prevBit;
							}

							fragmentMap[posX, posY] = GetFragmentWithExits(exitBit);
							lastRoom = new Vector2(posX, posY);
							isPlaced = true;
							posX += 1;
							prevDir = 3;
						}
					}
				}
				else if (newDir == 2) // south
				{
					if ((posY + 1) < MapHeight)
					{
						if (fragmentMap[posX, posY + 1] == -1)
						{
							int exitBit = 1;
							exitBit = exitBit << newDir;

							if (prevDir != -1)
							{
								int prevBit = 1;
								prevBit = prevBit << prevDir;
								exitBit = exitBit | prevBit;
							}

							fragmentMap[posX, posY] = GetFragmentWithExits(exitBit);
							lastRoom = new Vector2(posX, posY);
							isPlaced = true;
							posY += 1;
							prevDir = 0;
						}
					}
				}
				else if (newDir == 3) // west
				{
					if ((posX - 1) >= 0)
					{
						if (fragmentMap[posX - 1, posY] == -1)
						{
							int exitBit = 1;
							exitBit = exitBit << newDir;

							if (prevDir != -1)
							{
								int prevBit = 1;
								prevBit = prevBit << prevDir;
								exitBit = exitBit | prevBit;
							}

							fragmentMap[posX, posY] = GetFragmentWithExits(exitBit);
							lastRoom = new Vector2(posX, posY);
							isPlaced = true;
							posX -= 1;
							prevDir = 1;
						}
					}
				}
			}
			mazeLength -= 1;
		}

		/*
		// fill the empty spaces in the fragment map
		for (int i = 0; i < MapWidth; i++)
		{
			for (int j = 0; j < MapHeight; j++)
			{
				int exits = 0; 
				if (fragmentMap[i, j] == -1)
				{
					// check north 
					if (j > 0)
					{
						int fId = fragmentMap[i, j - 1];
						if (fId > 0)
						{
							int nExits = Fragments[fId].ExitBits;
							if ((nExits & 4) == 4)
							{
								exits = exits | 4;
							}
						}
					}

					// check east 
					if (i < (MapWidth - 1))
					{
						int fId = fragmentMap[i + 1, j];
                        if (fId > 0)
						{
							int nExits = Fragments[fId].ExitBits;
							if ((nExits & 8) == 8)
							{
								exits = exits | 8;
							}
						}
					}

					// check south 
					if (j < (MapHeight - 1))
					{
						int fId = fragmentMap[i, j + 1];
                        if (fId > 0)
						{
							int nExits = Fragments[fId].ExitBits;
							if ((nExits & 1) == 1)
							{
								exits = exits | 1;
							}
						}
					}

					// check west 
					if (i > 0)
					{
						int fId = fragmentMap[i - 1, j];
                        if (fId > 0)
						{
							int nExits = Fragments[fId].ExitBits;
							if ((nExits & 2) == 2)
							{
								exits = exits | 2;
							}
						}
					}

					fragmentMap[i, j] = GetFragmentWithExits(exits);
				}
			}
		}
		*/

		// fragment map ready, put fragment tiles into tilemap
		for (int i = 0; i < MapWidth; i++)
		{
			for (int j = 0; j < MapHeight; j++)
			{
				if (fragmentMap[i, j] == -1)
				{
					FillEmpty(i, j);
				}
				else
				{
					FillFragment(i, j, fragmentMap[i, j]);
				}
			}
		}

		/*
		// fill the outer tiles with walls
		for (int i = 0; i < MapWidth * fragmentSize; i++)
		{
			TileMap[i] = -1;
			TileMap[i + (((MapHeight * fragmentSize) - 1) * (MapWidth * fragmentSize))] = -1;
		}		
		for (int j = 0; j < MapHeight * fragmentSize; j++)
		{
			TileMap[(j * (MapWidth * fragmentSize))] = -1;
			TileMap[((MapWidth * fragmentSize) - 1) + (j * (MapWidth * fragmentSize))] = -1;
		}
		*/

		//fixes tiles with empty neighbours
		FixTileMap();

		//readjust player starting position
		List<Vector2> startPosList = new List<Vector2>();
		for (int i_ = 0; i_ < fragmentSize; i_++)
		{
			for (int j_ = 0; j_ < fragmentSize; j_++)
			{
				int playerPosX = (startX * fragmentSize) + i_;
				int playerPosY = (startY * fragmentSize) + j_;
				if (TileMap[playerPosX + (playerPosY * (MapWidth * fragmentSize))] > 15)
				{
					startPosList.Add(new Vector2(playerPosX, -playerPosY));
				}
            }
		}
		
		if (startPosList.Count > 0)
			StartPosition = startPosList[Random.Range(0, startPosList.Count - 1)];

		//readjust level exit position

		List<Vector2> endPosList = new List<Vector2>();
		for (int i_ = 0; i_ < fragmentSize; i_++)
		{
			for (int j_ = 0; j_ < fragmentSize; j_++)
			{
				int exitPosX = ((int)lastRoom.x * fragmentSize) + i_;
				int exitPosY = ((int)lastRoom.y * fragmentSize) + j_;
				if (TileMap[exitPosX + (exitPosY * (MapWidth * fragmentSize))] > 15)
				{
					endPosList.Add(new Vector2(exitPosX, -exitPosY));
				}
			}
		}

		if (endPosList.Count > 0)
			ExitPosition = endPosList[Random.Range(0, endPosList.Count - 1)];

		
		return 1;
	}

	private void FixTileMap()
	{
		for (int i = 0; i < (MapWidth * fragmentSize); i++)
		{
			for (int j = 0; j < (MapHeight * fragmentSize); j++)
			{
				if (TileMap[i + (j * (MapWidth * fragmentSize))] > 15)
				{
					//check north tile
					#region north
					if (j > 0)  // if there is a tile at north to check
					{
						if (TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == -1) // if its north neighbour is empty
						{
							TileMap[i + (j * (MapWidth * fragmentSize))] = 11;  // place a wall tile facing north instead

							//change its neighbours
							if (i > 0) //left 
							{
								if ((TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 4) || (TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 5))
								{
									TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] = 12;
								}
								else if (TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 0)
								{
									TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] = 10;
								}
							}
							if (i < ((MapWidth * fragmentSize) - 1)) //right
							{
								if ((TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 6) || (TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 7))
								{
									TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] = 15;
								}
								else if (TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 3)
								{
									TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] = 10;
								}
							}

							continue;
						}
						else if ((TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 8) || (TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 9)) // if its north neighbour is wall then connect
						{
							TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] = 16;

							TileMap[(i - 1) + ((j - 1) * (MapWidth * fragmentSize))] = 1;
							TileMap[(i + 1) + ((j - 1) * (MapWidth * fragmentSize))] = 2;

							continue;
                        }
					}
					else
					{
						TileMap[i + (j * (MapWidth * fragmentSize))] = 11;  // place a wall tile facing north instead

						//change its neighbours
						if (i > 0) //left 
						{
							if ((TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 4) || (TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 5))
							{
								TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] = 12;
							}
							else if (TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 0)
							{
								TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] = 10;
							}
						}
						if (i < ((MapWidth * fragmentSize) - 1)) //right
						{
							if ((TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 6) || (TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 7))
							{
								TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] = 15;
							}
							else if (TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 3)
							{
								TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] = 10;
							}
						}

						continue;
					}
					#endregion

					//check south tile
					#region south
					if (j < ((MapHeight * fragmentSize) - 1))  // if there is a tile at south to check
					{
						if (TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == -1) // if its south neighbour is empty
						{
							TileMap[i + (j * (MapWidth * fragmentSize))] = 8;  // place a wall tile facing south instead

							//change its neighbours
							if (i > 0) //left 
							{
								if ((TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 4) || (TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 5))
								{
									TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] = 13;
								}
								else if (TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 1)
								{
									TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] = 9;
								}
							}
							if (i < ((MapWidth * fragmentSize) - 1)) //right
							{
								if ((TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 6) || (TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 7))
								{
									TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] = 14;
								}
								else if (TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 2)
								{
									TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] = 9;
								}
							}

							continue;
						}
						else if ((TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 10) || (TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 11)) // if its south neighbour is wall then connect
						{
							TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] = 17;

							TileMap[(i - 1) + ((j + 1) * (MapWidth * fragmentSize))] = 0;
							TileMap[(i + 1) + ((j + 1) * (MapWidth * fragmentSize))] = 3;

							continue;
						}
					}
					else
					{
						TileMap[i + (j * (MapWidth * fragmentSize))] = 8;  // place a wall tile facing south instead

						//change its neighbours
						if (i > 0) //left 
						{
							if ((TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 4) || (TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 5))
							{
								TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] = 13;
							}
							else if (TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 1)
							{
								TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] = 9;
							}
						}
						if (i < ((MapWidth * fragmentSize) - 1)) //right
						{
							if ((TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 6) || (TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 7))
							{
								TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] = 14;
							}
							else if (TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 2)
							{
								TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] = 9;
							}
						}

						continue;
					}
					#endregion

					//check west tile
					#region west
					if (i > 0)  // if there is a tile at west to check
					{
						if (TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == -1) // if its west neighbour is empty
						{
							TileMap[i + (j * (MapWidth * fragmentSize))] = 5;  // place a wall tile facing west instead

							//change its neighbours
							if (j > 0) //up 
							{
								if ((TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 10) || (TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 11))
								{
									TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] = 12;
								}
								else if (TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 0)
								{
									TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] = 4;
								}
							}
							if (j < ((MapHeight * fragmentSize) - 1)) //down
							{
								if ((TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 8) || (TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 9))
								{
									TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] = 13;
								}
								else if (TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 1)
								{
									TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] = 4;
								}
							}

							continue;
						}
						else if ((TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 6) || (TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] == 7)) // if its west neighbour is wall then connect
						{
							TileMap[(i - 1) + (j * (MapWidth * fragmentSize))] = 18;

							TileMap[(i - 1) + ((j - 1) * (MapWidth * fragmentSize))] = 3;
							TileMap[(i - 1) + ((j + 1) * (MapWidth * fragmentSize))] = 2;

							continue;
						}
					}
					else
					{
						TileMap[i + (j * (MapWidth * fragmentSize))] = 5;  // place a wall tile facing west instead

						//change its neighbours
						if (j > 0) //up 
						{
							if ((TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 10) || (TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 11))
							{
								TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] = 12;
							}
							else if (TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 0)
							{
								TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] = 4;
							}
						}
						if (j < ((MapHeight * fragmentSize) - 1)) //down
						{
							if ((TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 8) || (TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 9))
							{
								TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] = 13;
							}
							else if (TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 1)
							{
								TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] = 4;
							}
						}

						continue;
					}
					#endregion

					//check east tile
					#region east
					if (i < ((MapHeight * fragmentSize) - 1))  // if there is a tile at east to check
					{
						if (TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == -1) // if its east neighbour is empty
						{
							TileMap[i + (j * (MapWidth * fragmentSize))] = 6;  // place a wall tile facing east instead

							//change its neighbours
							if (j > 0) //up 
							{
								if ((TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 10) || (TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 11))
								{
									TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] = 15;
								}
								else if (TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 3)
								{
									TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] = 7;
								}
							}
							if (j < ((MapHeight * fragmentSize) - 1)) //down
							{
								if ((TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 8) || (TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 9))
								{
									TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] = 14;
								}
								else if (TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 2)
								{
									TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] = 7;
								}
							}

							continue;
						}
						else if ((TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 4) || (TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] == 5)) // if its west neighbour is wall then connect
						{
							TileMap[(i + 1) + (j * (MapWidth * fragmentSize))] = 19;

							TileMap[(i + 1) + ((j - 1) * (MapWidth * fragmentSize))] = 0;
							TileMap[(i + 1) + ((j + 1) * (MapWidth * fragmentSize))] = 1;

							continue;
						}
					}
					else
					{
						TileMap[i + (j * (MapWidth * fragmentSize))] = 6;  // place a wall tile facing east instead

						//change its neighbours
						if (j > 0) //up 
						{
							if ((TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 10) || (TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 11))
							{
								TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] = 15;
							}
							else if (TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] == 3)
							{
								TileMap[i + ((j - 1) * (MapWidth * fragmentSize))] = 7;
							}
						}
						if (j < ((MapHeight * fragmentSize) - 1)) //down
						{
							if ((TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 8) || (TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 9))
							{
								TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] = 14;
							}
							else if (TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] == 2)
							{
								TileMap[i + ((j + 1) * (MapWidth * fragmentSize))] = 7;
							}
						}

						continue;
					}
					#endregion
				}
			}
		}


	}

	private void FillEmpty(int x, int y)
	{
		int _x = (x * fragmentSize);
		int _y = (y * fragmentSize);
		for (int i = _x; i < (_x + fragmentSize); i++)
		{
			for (int j = _y; j < (_y + fragmentSize); j++)
			{
				TileMap[i + (j * (MapWidth * fragmentSize))] = -1;
			}
		}
	}

	private void FillFragment(int x, int y, int fragmentId)
	{
		int _x = (x * fragmentSize);
		int _y = (y * fragmentSize);
		for (int i = _x; i < (_x + fragmentSize); i++)
		{
			for (int j = _y; j < (_y + fragmentSize); j++)
			{
				TileMap[i + (j * (MapWidth * fragmentSize))] = Fragments[fragmentId].Tiles[(j - _y), (i - _x)];
			}
		}
	}

	private int GetExitsOfFragment(int fragmentId)
	{
		MapFragment fragment = Fragments[fragmentId];

		return fragment.ExitBits;
	}

	private int GetFragmentWithExits(int exits)
	{
		int randFragment = Random.Range(0, Fragments.Count);
		
		int counterExit = 0;
		while ((Fragments[randFragment].ExitBits & exits) != exits)
		{
			counterExit++;
			if (counterExit > 100)
			{
				Debug.Log("counterFragment exit");
				return -1;
			}
			randFragment = Random.Range(0, Fragments.Count);
		}
		
		return randFragment;
	}


}
