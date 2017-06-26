using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaysInFutureUpdater : MonoBehaviour {

	public UnityEngine.UI.Slider slider;
	public Tidez tides;
	public Weatherz weathers;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void onDaysInFutureChanged() {
		tides.updateStartDay ((int)slider.value);
		weathers.updateStartDay ((int)slider.value);
		GetComponent<UnityEngine.UI.Text> ().text = "Start Day Offset: " + slider.value.ToString ();
	}
}
