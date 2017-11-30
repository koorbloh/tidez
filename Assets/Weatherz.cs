using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Weatherz : MonoBehaviour {

	//  https://api.weather.gov/points/47.9846,-122.5621 <-- 1 request here to get the data needed to ask the other questions
	//	https://api.weather.gov/gridpoints/SEW/120,86 <-- grid points gives you waves, wind direction, speed,
	//	https://api.weather.gov/points/47.9846,-122.5621/forecast  <-- points/[coord]/forecast gives you text, temp, 

	public Image waveChart;
	public Text WeatherText;
	public int NumForeCasts = 3;

	public int axisHeight = 50;
	public int imageDimensionsX = 512;
	public int imageDimensionsY = 128;


	private Dictionary<string, string> requestToResponseMap = new Dictionary<string, string> ();

	Dictionary<string, Vector2> buttonToCoordMap = new Dictionary<string, Vector2>();

	public int startDayOffset = 0;

	public void updateStartDay(int daysInFuture) {
		startDayOffset = daysInFuture;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void GetWeatherButton() {
		waveChart.enabled = false;
		GameObject selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
		Button selectedButton = selected.GetComponent<Button> ();
		Text selectedText = selectedButton.GetComponentInChildren<Text> ();

		if (buttonToCoordMap.ContainsKey(selectedText.text)) {
			GetWeather (buttonToCoordMap [selectedText.text]);
		}
	}

	public void GetWeather(Vector2 coord) {
		WeatherText.text = "Weather pending.....";
		string url = new NOAAWeathers.NOAAWeatherRequestBuilder ().getUrlForPoint (coord.x, coord.y);
		Debug.Log (url);
		if (requestToResponseMap.ContainsValue(url)) {
			handleRequestResponse (requestToResponseMap [url]);
		} else {
			StartCoroutine (GetPointData (url));
		}
	}

	IEnumerator GetPointData(string url) {
		Debug.Log ("point data: " + url);
		Dictionary<string,string> headers = new Dictionary<string, string> ();
		headers.Add ("User-Agent", "jeff.p.holbrook@gmail.com");
		headers.Add ("Accept-Encoding", "identity");
		headers.Add ("Accept", "application/geo+json");
		WWW www = new WWW (url, null, headers);
		yield return www;
		Debug.Log ("POINT DATA GOT");
		Debug.Log (url);
		Debug.Log (www.text);
		if (www.error != null) {
			Debug.Log ("POINT DATA ERROR: ");
			Debug.Log (www.error);
		}
		requestToResponseMap [url] = www.text;
		handleRequestResponse (www.text);
	}

	IEnumerator GetPointForecast(string url) {
		Debug.Log ("point forecast: " + url);
		Dictionary<string,string> headers = new Dictionary<string, string> ();
		headers.Add ("User-Agent", "jeff.p.holbrook@gmail.com");
		headers.Add ("Accept-Encoding", "identity");
		headers.Add ("Accept", "application/geo+json");
		WWW www = new WWW (url, null, headers);
		yield return www;
		Debug.Log ("POINT FORECAST GOT");
		Debug.Log (url);
		Debug.Log (www.text);
		requestToResponseMap [url] = www.text;
		handlePointForecast(www.text);
	}

	IEnumerator GetGridForecast(string url) {
		Debug.Log ("grid forecast: " + url);
		Dictionary<string,string> headers = new Dictionary<string, string> ();
		headers.Add ("User-Agent", "jeff.p.holbrook@gmail.com");
		headers.Add ("Accept-Encoding", "identity");
		headers.Add ("Accept", "application/geo+json");
		WWW www = new WWW (url, null, headers);
		yield return www;
		Debug.Log ("GRID DATA GOT");
		Debug.Log (url);
		Debug.Log (www.text);
		requestToResponseMap [url] = www.text;
		handleGridForecast(www.text);
	}

	public void handleRequestResponse(string response) {
		NOAAWeathers.PointData pointData = JsonUtility.FromJson<NOAAWeathers.PointData> (response);
		NOAAWeathers.NOAAWeatherRequestBuilder builder = new NOAAWeathers.NOAAWeatherRequestBuilder ();

		string gridUrl = builder.getUrlForGridPoint (pointData);
		if (requestToResponseMap.ContainsValue(gridUrl)) {
			handleGridForecast (requestToResponseMap [gridUrl]);
		} else {
			StartCoroutine (GetGridForecast (gridUrl));
		}

		string pointDataForecastUrl = builder.getUrlForPointForecast (pointData);
		if (requestToResponseMap.ContainsValue(pointDataForecastUrl)) {
			handlePointForecast (requestToResponseMap [pointDataForecastUrl]);
		} else {
			StartCoroutine (GetPointForecast (pointDataForecastUrl));
		}

	}

	public void handlePointForecast(string response) {
		NOAAWeathers.PointForecastData pointForecastData = JsonUtility.FromJson<NOAAWeathers.PointForecastData> (response);

		if (pointForecastData == null || 
			pointForecastData.properties == null || 
			pointForecastData.properties.periods == null || 
			pointForecastData.properties.periods.Length == 0) {
			WeatherText.text = "Weather wasn't happy.....";
			return;
		}

		//find the first forecast that isn't over yet from when the start date is
		//then show the next 3? forecasts?
		int numToBlindlyInclude = 0;
		DateTime weatherStart = DateTime.Today.AddDays (startDayOffset);
		string weather = "";
		for (int i = 0; i < pointForecastData.properties.periods.Length; ++i) {
			NOAAWeathers.PointForecastData_period period = pointForecastData.properties.periods [i];
			DateTime startTime = DateTime.Parse (period.startTime);
			DateTime endTime = DateTime.Parse (period.endTime);
			//I tried for like 6 minutes to only include 3 forecasts, then I decided to let the text box limit it beacuse I don't care
			//The text box will hold like 3-ish forecasts
			//the start date thing works great though
			if (numToBlindlyInclude > 0 || startTime > weatherStart && weatherStart < endTime) {
				if (numToBlindlyInclude == 0) {
					numToBlindlyInclude = NumForeCasts;
				} else {
					--numToBlindlyInclude;
				}
				weather += ("<b>" + period.name + "</b> " + period.shortForecast + "\n");
				weather += (period.temperature + "° " + period.temperatureTrend + " " +" Wind: " + period.windSpeed + " " + period.windDirection + "\n");
			}

		}
		WeatherText.text = weather;
	}

	public void handleGridForecast(string response) {
		waveChart.enabled = true;

		Debug.Log ("GRID FORECAST: " + response);
		NOAAWeathers.GridPointForecastData gridForecastData = JsonUtility.FromJson<NOAAWeathers.GridPointForecastData> (response);
		Debug.Log (gridForecastData);	

		if (gridForecastData == null ||
			gridForecastData.properties == null ||
			gridForecastData.properties.windWaveHeight == null ||
			gridForecastData.properties.windWaveHeight.values == null ||
			gridForecastData.properties.windWaveHeight.values.Length == 0) {
			return;
		}

		int pixelsPerFoot = axisHeight;
		Texture2D texture = new Texture2D(imageDimensionsX, imageDimensionsY);
		new Graphz (texture, axisHeight, DateTime.Today.AddDays(startDayOffset), imageDimensionsX, imageDimensionsY).graphData ((delegate(DateTime time) {
			return 10 * pixelsPerFoot * getPredictedWindWaveHeightAtTime(gridForecastData.properties.windWaveHeight, time);
		}), (delegate() {
			List<Graphz.DataPoint> points = new List<Graphz.DataPoint>();
			points.Add(new Graphz.DataPoint(axisHeight, Color.black));
			points.Add(new Graphz.DataPoint(axisHeight*2, Color.grey));
			points.Add(new Graphz.DataPoint(axisHeight*3, Color.grey));

			return points;			
		}));

		Sprite oldSprite = waveChart.sprite;
		waveChart.sprite = Sprite.Create(texture, new Rect(0,0, imageDimensionsX, imageDimensionsY), oldSprite.pivot);		

	}

	private bool parseDateAndDuration(string dateAndDuration, out DateTime dateTime, out TimeSpan duration) {
		string[] timeParts = dateAndDuration.Split ('/');
		if (timeParts.Length != 2) {
			dateTime = DateTime.Now;
			duration = TimeSpan.Zero;	
			return false;
		}
		dateTime = DateTime.Parse (timeParts [0]);
		duration = System.Xml.XmlConvert.ToTimeSpan (timeParts [1]);
		return true;
	}

	private float getPredictedWindWaveHeightAtTime(NOAAWeathers.GridPointForecastData_windWaveHeight wavePredictions, DateTime dateTime) {
		for (int i = 0; i < wavePredictions.values.Length; ++i) {
			NOAAWeathers.GridPointForecastData_windWaveHeight_values value = wavePredictions.values [i];
			DateTime periodStart;
			TimeSpan duration;
			if (!parseDateAndDuration(value.validTime, out periodStart, out duration)) {
				continue;
			}
			if ((i == 0 || periodStart <= dateTime) && dateTime <= periodStart.Add(duration)) {
				return value.value/3.28084f; //meters to feet
			}

		}
		return 0.0f;
	}
}
