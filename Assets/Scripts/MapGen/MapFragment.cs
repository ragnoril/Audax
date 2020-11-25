using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class MapFragment
{
	public List<int> Exits;
	public int ExitBits;
	public int[,] Tiles;
	public int fragmentSize;

	public void LoadTiles(string tiles)
	{
		Tiles = new int[fragmentSize, fragmentSize];
		Exits = new List<int>();

		string[] tileList = tiles.Split(',');

		for (int i = 0; i < fragmentSize; i++)
		{
			for (int j = 0; j < fragmentSize; j++)
			{
				Tiles[i, j] = int.Parse(tileList[j + (i * fragmentSize)]) - 1;
			}
		}

	}

	public void LoadTilesFromTextFiles(string filePath)
	{
		Tiles = new int[fragmentSize, fragmentSize];
		Exits = new List<int>();

		
		string[] lines = File.ReadAllLines(filePath);
		foreach (var exit in lines[0])
		{
			Exits.Add(int.Parse(exit.ToString()));
		}

		ExitBits = Convert.ToInt32(lines[0], 2);
		//Debug.Log(filePath + " : " + ExitBits.ToString());

		for (int i = 1; i < lines.Length; i++)
		{
			for (int j = 0; j < lines[i].Length; j++)
			{
				if (lines[i][j] == '#')
					Tiles[(i - 1), j] = 1;
				else if (lines[i][j] == '.')
					Tiles[(i - 1), j] = 0;
				else if (lines[i][j] == '>')
					Tiles[(i - 1), j] = 2;
				else if (lines[i][j] == '<')
					Tiles[(i - 1), j] = 3;
				else
					Tiles[(i - 1), j] = -1;
			}
		}

	}

}

