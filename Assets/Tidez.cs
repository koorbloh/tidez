using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Tidez : MonoBehaviour {

//	9447659 EVERETT, WA
//	9447854 BUSH POINT WHIDBEY ISLAND, WA
//	9446484 Tacoma, WA
//	9448794 ANACORTES, FIDALGO ISLAND, WA
//	9445016 Foulweather Bluff, WA



//X	v1.0
//X	tide predictions
//X	v1.1
//X	Caching of requests and responses
//X	Date Choosing
//	v1.2
//x	Weather predictions
//x	will need to refactor to make a mapping of location to: station id, coords, etc
//x  https://api.weather.gov/points/47.9846,-122.5621 <-- 1 request here to get the data needed to ask the other questions
//x	https://api.weather.gov/gridpoints/SEW/120,86 <-- grid points gives you waves, wind direction, speed,
//x	https://api.weather.gov/points/47.9846,-122.5621/forecast  <-- points/[coord]/forecast gives you text, temp, 
//	v1.2.1
//x	tides work great, but weather and waves don't, no clue why
//x	make it show up clearly on the phone
//x 		buttons need to be 2x larger
//x		slider 2x larger
//x		only using 1/2 of the veritical space
//x		guess we need to scale everything dynamically...poop
//	v1.3
//x	better points for wind waves
//	current predictions
//x  current graph examples: http://l-36.com/weather.php?lat=47.91&lon=-122.3&point1=Mukilteo,+WA&point2=Marine+Location+Near+Mukilteo,+WA&tide1=Glendale,+Whidbey+Island,+Washington&tide2=Apple+Cove+Point,+0.5+mile+E+of,+Washington+Current&lat_long1=47.91,-122.3&radar=ATX&radar2=RTX&station=sew&ports=9447130&rss=wpow1&rss2=ebsw1&rss3=ptww1&airport=KPAE&geos=west/nw&lat_long2=47.91,-122.3&yd10=on&zone1=PZZ134&zone2=PZZ100&v=0.50&where=Mukilteo,+WA
//x	start day to string for day of week -- duh
//  v1.4
//	improved graphing


	private NOAA.TidePredictions predictions = null;

	public Image tideChart;

	public Text selectedLabel;

	public int axisHeight = 128;
	public int imageDimensionsX = 512;
	public int imageDimensionsY = 256;

	public int startDayOffset = 0;

	private Dictionary<string, string> requestToResponseMap = new Dictionary<string, string> ();
	private string pendingStationText = "";

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		tideChart.sprite = tideChart.sprite;
	}

	public void updateStartDay(int daysInFuture) {
		startDayOffset = daysInFuture;
	}

	public void GetTidesButton(int stationId) {
		string unescapedURL = new NOAA.TidesAndCurrentsRequestBuilder ()
			//.withRange (72)
			.atStation (stationId)
			.getPredictions()
			.withStandardUnitsAndTimeZone()
			.provideApplicationName()
			.specifyFormat("json")
			.forDatum("MLLW")
			.withBeginDate(System.DateTime.Now.AddDays(startDayOffset))
			.withEndDate(System.DateTime.Now.AddDays(2 + startDayOffset))
			.getRequest();

		startPendingRequest (unescapedURL);
	}

	public void startPendingRequest(string url) {
		tideChart.enabled = false;
		GameObject selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
		Button selectedButton = selected.GetComponent<Button> ();
		Text selectedText = selectedButton.GetComponentInChildren<Text> ();
		pendingStationText = selectedText.text;
		selectedLabel.text = "[Pending] " + pendingStationText;

		Debug.Log (url);

		if (requestToResponseMap.ContainsKey (url)) {
			handleRequestResponse (requestToResponseMap [url]);
		} else {
			StartCoroutine (GetTidez(url));
		}
	}

	public void handleRequestResponse(string response) {
		tideChart.enabled = true;

		selectedLabel.text = pendingStationText;

		predictions = JsonUtility.FromJson<NOAA.TidePredictions> (response);

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
		DateTime prev = new DateTime ();
		DateTime lastBar = new DateTime ();
		bool flipper = false;
		for (int i = 0; i < predictions.predictions.Count; ++i) {
			float level = float.Parse (predictions.predictions [i].v);
			int xCoord = (int)(texture.width * i) / predictions.predictions.Count;
			texture.SetPixel(xCoord,(int)(level * 14) + axisHeight, Color.blue);

			DateTime current = DateTime.Parse (predictions.predictions [i].t);

			//ok every 4 hours draw a black line
			if (current > lastBar.AddHours(4)) {
				Color color;
				if (flipper) {
					color = Color.grey;
				} else {
					color = Color.black;
				}
				flipper = !flipper;

				for (int j = 0; j < texture.height; ++j) {
					texture.SetPixel (xCoord, j, color);
				}
				lastBar = current;
			}

			//at current time, draw a red line
			if (prev < now && now < current) {
				for (int j = 0; j < texture.height; ++j) {
					texture.SetPixel (xCoord, j, Color.red);
					texture.SetPixel (xCoord - 1, j, Color.red);
					texture.SetPixel (xCoord + 1, j, Color.red);
				}
			}

			prev = current;
		}

		texture.Apply();

		Sprite oldSprite = tideChart.sprite;
		tideChart.sprite = Sprite.Create(texture, new Rect(0,0, imageDimensionsX, imageDimensionsY), oldSprite.pivot);		
	}
		
	IEnumerator GetTidez(string url) {


		WWW www = new WWW (url);
	
		yield return www;

		requestToResponseMap [url] = www.text;

		handleRequestResponse (www.text);

	}
}
