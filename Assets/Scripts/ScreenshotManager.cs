using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotManager : MonoBehaviour
{

	public bool EnableAutoTake;
	public float ShotTimer;
	float nextShotToTake;
	public KeyCode KeyForAuto;
	public KeyCode KeyForSingle;
	public string FolderName;
	public int SuperSize;

	// Use this for initialization
	void Start ()
	{
		if (string.IsNullOrEmpty(FolderName))
		{
			FolderName = Application.dataPath + "/../Screenshots";
		}

		nextShotToTake = ShotTimer;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown(KeyForSingle))
		{
			TakeScreenShot();
		}

		if (Input.GetKeyDown(KeyForAuto))
		{
			EnableAutoTake = !EnableAutoTake;
		}

		if (EnableAutoTake)
		{
			nextShotToTake -= Time.deltaTime;


			if (nextShotToTake <= 0f)
			{
				TakeScreenShot();
				nextShotToTake = ShotTimer;
			}
		}
	}

	void TakeScreenShot()
	{
		if (SuperSize > 1)
			ScreenCapture.CaptureScreenshot(FolderName + "/Capture_" + DateTime.Now.ToString("yyyyMMdd_HHmmssfff") + ".png", SuperSize);
		else
			ScreenCapture.CaptureScreenshot(FolderName + "/Capture_" + DateTime.Now.ToString("yyyyMMdd_HHmmssfff") + ".png");
	}
}
