using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
	public Sprite[] UpgradeIcons;

	public Image[] Upgrades;
	public Text[] UpgradeTexts;
	public int[] UpgradeIds;

	UpgradeSelector _upgrader;

	// Use this for initialization
	void Start ()
	{
		_upgrader = GetComponent<UpgradeSelector>();	
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void SetupUpgrades()
	{
		/*
		int i =_upgrader.GetOneUpgrade();
		int j = _upgrader.GetOneUpgrade();
		int k = _upgrader.GetOneUpgrade();
		*/
		if (_upgrader == null) _upgrader = GetComponent<UpgradeSelector>();

		UpgradeIds = new int[Upgrades.Length];

		for(int i = 0; i < Upgrades.Length; i++)
		{
			int id = _upgrader.GetOneUpgrade();

			UpgradeIds[i] = id;

			Upgrades[i].sprite = UpgradeIcons[id];
			UpgradeTexts[i].text = _upgrader.GetSelectedUpgradeText(id);
			
		}
	}

	public void SelectUpgrade(int id)
	{
		_upgrader.ImplementUpgrade(UpgradeIds[id]);
		GameManager.instance.ContinueGame();
	}
}
