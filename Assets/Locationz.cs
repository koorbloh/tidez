using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Locationz : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine("StartGPS");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public int maxWaitSeconds = 20;
	public Text locationLabel;

	private float lat = -1000;
	private float lon = -1000;

	public float getLat() {
		return lat;
	}

	public float getLon() {
		return lon;
	}

	public bool getReady() {
		return lat != -1000 && lon != -1000;
	}

	IEnumerator StartGPS()
	{
		Debug.Log ("StartGPS");
		// First, check if user has location service enabled
		if (!Input.location.isEnabledByUser) {
			locationLabel.text = "Location Disabled.";
			yield break;
		}

		// Start service before querying location
		Input.location.Start();

		// Wait until service initializes
		while (Input.location.status == LocationServiceStatus.Initializing && maxWaitSeconds > 0)
		{
			yield return new WaitForSeconds(1);
			locationLabel.text = "Location Pending...";
			maxWaitSeconds--;
		}

		// Service didn't initialize in 20 seconds
		if (maxWaitSeconds < 1)
		{
			Debug.Log("Timed out");
			locationLabel.text = "Location Timed out.";
			yield break;
		}

		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed)
		{
			Debug.Log("Unable to determine device location");
			locationLabel.text = "Location failed.";
			yield break;
		}
		else
		{
			// Access granted and location value could be retrieved
			locationLabel.text = "Lat: " + Input.location.lastData.latitude + 
				" Long: " + Input.location.lastData.longitude +
				" Alt: " + Input.location.lastData.altitude + 
				" Acc: " + Input.location.lastData.horizontalAccuracy;
			lat = Input.location.lastData.latitude;
			lon = Input.location.lastData.longitude;
			Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
		}

		// Stop service if there is no need to query location updates continuously
		Input.location.Stop();
	}
}
