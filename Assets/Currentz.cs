using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using NOAACurrent;

public class Currentz : MonoBehaviour {

	Dictionary<string, Dictionary<int, string>> buttonToYearToFileMap = new Dictionary<string, Dictionary<int, string>>();

	public Image currentChart;

	public int axisHeight = 128;
	public int imageDimensionsX = 512;
	public int imageDimensionsY = 256;

	public int startDayOffset = 0;

	public void updateStartDay(int daysInFuture) {
		startDayOffset = daysInFuture;
	}

	public void GetTidesButton() {
		Debug.Log ("GET CURRENTS");

		GameObject selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
		Button selectedButton = selected.GetComponent<Button> ();
		Text selectedText = selectedButton.GetComponentInChildren<Text> ();

		int currentYear = DateTime.Today.Year;

		if (buttonToYearToFileMap[selectedText.text] == null ||
			buttonToYearToFileMap[selectedText.text][currentYear] == null) {
			return;
		}

		string fileToLoad = buttonToYearToFileMap [selectedText.text] [currentYear];

		GraphCurrents (fileToLoad);


	}

	// Use this for initialization
	void Start () {

		Dictionary<int, string> locationData = new Dictionary<int, string> ();

			locationData = new Dictionary<int, string> ();
			locationData [2017] = "currents/friday_harbor_current_2017";
			locationData [2018] = "currents/friday_harbor_current_2018";
			locationData [2019] = "currents/friday_harbor_current_2019";
		buttonToYearToFileMap.Add ("Friday Harbor",      locationData);

			locationData = new Dictionary<int, string> ();
			locationData [2017] = "currents/foul_weather_bluff_current_2017";
			locationData [2018] = "currents/foul_weather_bluff_current_2018";
			locationData [2019] = "currents/foul_weather_bluff_current_2019";
		buttonToYearToFileMap.Add ("Foul Weather Bluff", locationData);
			
			locationData = new Dictionary<int, string> ();
			locationData [2017] = "currents/tacoma_current_2017";
			locationData [2018] = "currents/tacoma_current_2018";
			locationData [2019] = "currents/tacoma_current_2019";
		buttonToYearToFileMap.Add ("Tacoma",             locationData);
			
			locationData = new Dictionary<int, string> ();
			locationData [2017] = "currents/seattle_current_2017";
			locationData [2018] = "currents/seattle_current_2018";
			locationData [2019] = "currents/seattle_current_2019";
		buttonToYearToFileMap.Add ("Seattle",            locationData);
			
			locationData = new Dictionary<int, string> ();
			locationData [2017] = "currents/foul_weather_bluff_current_2017";
			locationData [2018] = "currents/foul_weather_bluff_current_2018";
			locationData [2019] = "currents/foul_weather_bluff_current_2019";
		buttonToYearToFileMap.Add ("Everett",            locationData);
			
			locationData = new Dictionary<int, string> ();
			locationData [2017] = "currents/port_townsend_current_2017";
			locationData [2018] = "currents/port_townsend_current_2018";
			locationData [2019] = "currents/port_townsend_current_2019";
		buttonToYearToFileMap.Add ("Port Townsend",      locationData);


	}


	void GraphCurrents(string fileToLoad) {

		Texture2D texture = new Texture2D(imageDimensionsX, imageDimensionsY);
		for (int y = 0; y < texture.height; y++)
		{
			for (int x = 0; x < texture.width; x++)
			{
				Color color = Color.white;
				if (y == axisHeight) {
					color = Color.black;	
				}
				texture.SetPixel(x, y, color);
			}
		}

		DateTime now = DateTime.Now;
		DateTime startTime = DateTime.Today.AddDays (startDayOffset);
		CurrentData currentData = new CurrentData (fileToLoad);
		DateTime lastLine = startTime;
		int minutesPerPixel = (3 * 24 * 60)/imageDimensionsX;
		bool flipper = false;
		for (int x = 0; x < texture.width; x++) {
			texture.SetPixel (x, axisHeight + 30, Color.grey);
			texture.SetPixel (x, axisHeight + 60, Color.black);
			texture.SetPixel (x, axisHeight + 90, Color.grey);
			texture.SetPixel (x, axisHeight - 30, Color.grey);
			texture.SetPixel (x, axisHeight - 60, Color.black);
			texture.SetPixel (x, axisHeight - 90, Color.grey);
			int height = axisHeight + (int)(30.0f * getCurrentSpeedAtTime (currentData, startTime.AddMinutes (minutesPerPixel * x)));
			texture.SetPixel (x, height, Color.blue);
			texture.SetPixel (x, height+1, Color.blue);
			texture.SetPixel (x, height-1, Color.blue);
			if (startTime.AddMinutes(minutesPerPixel * x) > lastLine.AddHours(4)) {
				for (int y = 0; y < imageDimensionsY; ++y) {
					if (flipper) 
						texture.SetPixel (x, y, Color.black);
					else 
						texture.SetPixel (x, y, Color.grey);
				}
				flipper = !flipper;
				lastLine = startTime.AddMinutes (minutesPerPixel * x);
			}

			//if it's basically now, draw a red line
			if (startTime.AddMinutes(minutesPerPixel * (x-1)) < now && now < startTime.AddMinutes(minutesPerPixel * x)) {
				for (int y = 0; y < imageDimensionsY; ++y) {
					texture.SetPixel (x, y, Color.red);
					texture.SetPixel (x+1, y, Color.red);
					texture.SetPixel (x-1, y, Color.red);
				}
			}
		}
			
		texture.Apply();

		Sprite oldSprite = currentChart.sprite;
		currentChart.sprite = Sprite.Create(texture, new Rect(0,0, imageDimensionsX, imageDimensionsY), oldSprite.pivot);		
	}


	float getCurrentSpeedAtTime(CurrentData currentData, DateTime time) {
		for (int i = 0; i < currentData.dataPoints.Count - 1; ++i) {
			if (currentData.dataPoints[i].timestamp <= time && currentData.dataPoints[i+1].timestamp >= time) {
				//found our 2 points! yay!
				long totalTime = currentData.dataPoints[i+1].timestamp.Ticks - currentData.dataPoints[i].timestamp.Ticks;
				long elapsedTime = time.Ticks - currentData.dataPoints[i].timestamp.Ticks;
				float percent = ((float)elapsedTime) / ((float)totalTime);
				return Mathf.Lerp (currentData.dataPoints [i].knots, currentData.dataPoints [i + 1].knots, percent);
			}
		}
		return 0;
	}


}
