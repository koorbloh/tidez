using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Weatherz : MonoBehaviour {

	//	Seattle, 47.601667, -122.338333
	//	Tacoma, 47.270000, -122.413333
	//	Port Townsend, 48.113333, -122.760000
	//	Friday Harbor, 48.545000, -123.013333
	//	Foul Weather Bluff, 47.926667, -122.616667
	//	Everett, 47.980000, -122.223333

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
		buttonToCoordMap.Add ("Seattle", new Vector2 (47.601667f, -122.338333f));
		buttonToCoordMap.Add ("Tacoma", new Vector2 (47.270000f, -122.413333f));
		buttonToCoordMap.Add ("Port Townsend", new Vector2 (48.113333f, -122.760000f));
		buttonToCoordMap.Add ("Friday Harbor", new Vector2 (48.545000f, -123.013333f));
		buttonToCoordMap.Add ("Foul Weather Bluff", new Vector2 (47.926667f, -122.616667f));
		buttonToCoordMap.Add ("Everett", new Vector2 (47.980000f, -122.223333f));

		//random ass point out in the effin straight of juan de fuca, REALLY helpful for debugging
		//buttonToCoordMap.Add ("Everett", new Vector2 (48.260730f, -122.877708f));
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

		WeatherText.text = "TOTALLY PENDING";

	}

	void GetWeather(Vector2 coord) {
		WeatherText.text = "Weather pending.....";
		string url = new NOAAWeathers.NOAAWeatherRequestBuilder ().getUrlForPoint (coord.x, coord.y);
		Debug.Log (url);
		if (requestToResponseMap.ContainsValue(url)) {
			handleRequestResponse (requestToResponseMap [url]);
		} else {
			StartCoroutine (GetPointData (url));
		}
		WeatherText.text = WeatherText.text + "\nREQ 1 SEND";
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
				weather += (period.name + " " + period.shortForecast + "\n");
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

		Texture2D texture = new Texture2D(imageDimensionsX, imageDimensionsY);
		int graduations = axisHeight;
		for (int y = 0; y < texture.height; y++)
		{
			for (int x = 0; x < texture.width; x++)
			{
				Color color = Color.white;
				if (y == axisHeight) {
					color = Color.black;	
				} else if (y % graduations == 0) {
					color = Color.grey;
				}
				texture.SetPixel(x, y, color);
			}
		}


		DateTime graphStartTime = DateTime.Today.AddDays(startDayOffset);
		int minutesPerPixel = (3 * 24 * 60)/imageDimensionsX;
		DateTime lastLine = graphStartTime;
		DateTime now = DateTime.Now;
		//for every x pixel
		int pixelsPerFoot = axisHeight;
		bool flipper = false;
		for (int x = 0; x < texture.width; ++x) {
			//find a y pixel
			float feet = 10 * pixelsPerFoot * getPredictedWindWaveHeightAtTime(gridForecastData.properties.windWaveHeight, graphStartTime.AddMinutes(minutesPerPixel * x));
			texture.SetPixel (x, (int)feet + axisHeight, Color.blue);
			texture.SetPixel (x, (int)feet + axisHeight+1, Color.blue);
			texture.SetPixel (x, (int)feet + axisHeight+2, Color.blue);
			//if it's been 4 hours since we last drew a line, draw one
			if (graphStartTime.AddMinutes(minutesPerPixel * x) > lastLine.AddHours(4)) {
				for (int y = 0; y < imageDimensionsY; ++y) {
					if (flipper) 
						texture.SetPixel (x, y, Color.black);
					else 
						texture.SetPixel (x, y, Color.grey);
				}
				flipper = !flipper;
				lastLine = graphStartTime.AddMinutes (minutesPerPixel * x);
			}

			//if it's basically now, draw a red line
			if (graphStartTime.AddMinutes(minutesPerPixel * (x-1)) < now && now < graphStartTime.AddMinutes(minutesPerPixel * x)) {
				for (int y = 0; y < imageDimensionsY; ++y) {
					texture.SetPixel (x, y, Color.red);
					texture.SetPixel (x+1, y, Color.red);
					texture.SetPixel (x-1, y, Color.red);
				}
			}
		}
				
		texture.Apply();

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
