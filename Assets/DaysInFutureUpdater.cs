using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaysInFutureUpdater : MonoBehaviour {

	public UnityEngine.UI.Slider slider;
	public Tidez tides;
	public Weatherz weathers;
	public Currentz currents;

	// Use this for initialization
	void Start () {
		DateTime now = DateTime.Now;
		string day = now.DayOfWeek.ToString ();
		GetComponent<UnityEngine.UI.Text> ().text = "Start Day: " + day;

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void onDaysInFutureChanged() {
		tides.updateStartDay ((int)slider.value);
		weathers.updateStartDay ((int)slider.value);
		currents.updateStartDay ((int)slider.value);
		DateTime now = DateTime.Now;
		string day = now.AddDays (slider.value).DayOfWeek.ToString ();
		GetComponent<UnityEngine.UI.Text> ().text = "Start Day: " + day;
	}
}
