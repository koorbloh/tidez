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

		GetCurrentsByLocationName (selectedText.text);

	}

	public void GetCurrentsByLocationName(string locationName) {
		if (locationName == null || !buttonToYearToFileMap.ContainsKey(locationName)) {
			return;
		}

		int currentYear = DateTime.Today.Year;

		if (buttonToYearToFileMap[locationName] == null ||
			buttonToYearToFileMap[locationName][currentYear] == null) {
			return;
		}

		string fileToLoad = buttonToYearToFileMap [locationName] [currentYear];

		GraphCurrents (fileToLoad);

	}

	// Use this for initialization
	void Start () {

		//I'd love to move this data table into the poorly-named DropDownHandler that's really the big boss

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

		CurrentData currentData = new CurrentData (fileToLoad);
		new Graphz (texture, axisHeight, DateTime.Today.AddDays(startDayOffset), imageDimensionsX, imageDimensionsY).graphData ((delegate(DateTime time) {
			return 30.0f * getCurrentSpeedAtTime(currentData, time);
		}), (delegate() {
			List<Graphz.DataPoint> points = new List<Graphz.DataPoint>();
			points.Add(new Graphz.DataPoint(axisHeight, Color.black));
			points.Add(new Graphz.DataPoint(axisHeight + 30, Color.grey));
			points.Add(new Graphz.DataPoint(axisHeight + 60, Color.black));
			points.Add(new Graphz.DataPoint(axisHeight + 90, Color.grey));
			points.Add(new Graphz.DataPoint(axisHeight - 30, Color.grey));
			points.Add(new Graphz.DataPoint(axisHeight - 60, Color.black));
			points.Add(new Graphz.DataPoint(axisHeight - 90, Color.grey));
			return points;
		}));

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
