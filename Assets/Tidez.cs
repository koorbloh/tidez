using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Tidez : MonoBehaviour {

	public Image tideChart;
	public Text selectedLabel;
	public int axisHeight = 128;
	public int imageDimensionsX = 512;
	public int imageDimensionsY = 256;
	public int startDayOffset = 0;

	private Dictionary<string, string> requestToResponseMap = new Dictionary<string, string> ();
	private string pendingStationText = "";
	private NOAA.TidePredictions predictions = null;

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

	public void GetTidesButton(int stationId, string stationText) {

		if (stationId < 0) {
			return;
		}

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

		startPendingRequest (unescapedURL, stationText);
	}

	public void startPendingRequest(string url, string stationText) {
		tideChart.enabled = false;
		pendingStationText = stationText;
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
		int iHint = 0;
		DateTime lastTime = DateTime.MinValue;
		new Graphz (texture, axisHeight, DateTime.Today.AddDays(startDayOffset), imageDimensionsX, imageDimensionsY).graphData ((delegate(DateTime time) {

			if (time < lastTime) {
				iHint = 0;
			}

			lastTime = time;
			for (int i = iHint; i < predictions.predictions.Count - 1; ++i) {
				DateTime current = DateTime.Parse (predictions.predictions [i].t);
				DateTime next = DateTime.Parse (predictions.predictions [i + 1].t);
				if (current <= time && time < next) {
					return 14*float.Parse(predictions.predictions[i].v);
				}
				iHint = i;
			}

			return 0;
		}), (delegate() {
			List<Graphz.DataPoint> points = new List<Graphz.DataPoint>();
			points.Add(new Graphz.DataPoint(axisHeight, Color.black));
			return points;			
		}));

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
