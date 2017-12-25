using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropDownHandler : MonoBehaviour {

	public UnityEngine.UI.Dropdown locationDropDown;

	public Tidez tidez;
	public Weatherz weatherz;
	public Currentz currentz;
	public Locationz locationz;

	//THESE ORDERS MATTER, AND THAT'S GROSS
	//THEY NEED TO MATCH UP WITH THE DROP DOWN
	//I'LL REFACTOR IT LATER WHEN IT BUGS MORE MORE
	// <3, JEFF

	class datadata {
		public string location;
		public int stationId;
		public Vector2 coord;
		public datadata(string _location, int _stationId, Vector2 _coord) {
			location = _location;
			stationId = _stationId;
			coord = _coord;
		}
	}

	//This is pretty gross to just hardcode this list, again....
	datadata[] data = {
		new datadata (null, -1, new Vector2 (47.888900f, -121.930150f)),
		new datadata ("Everett", 9447659, new Vector2 (47.981434f, -122.292339f)),
		new datadata ("Port Townsend", 9444900, new Vector2 (48.121882f, -122.683192f)),
		new datadata ("Tacoma", 9446484, new Vector2 (47.326957f, -122.432850f)),
		new datadata ("Friday Harbor", 9449880, new Vector2 (48.577655f, -122.855933f)),
		new datadata ("Foul Weather Bluff", 9445016, new Vector2 (47.948556f, -122.584284f)),
		new datadata ("Seattle", 9447130, new Vector2 (47.601667f, -122.338333f)),
		new datadata ("Home", -1, new Vector2 (47.888900f, -121.930150f)),
//		new datadata ("Here", -1, new Vector2 (-1000, -1000)),
		new datadata ("Here", -1, new Vector2 (42.48f, -114.45f)),
	};
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}




	public void handleLocationDropdownChanged(int selected) {
		tidez.GetTidesButton (data [selected].stationId, data[selected].location); 
		currentz.GetCurrentsByLocationName (data [selected].location);
		Vector2 coord = data [selected].coord;
		if (coord.x == 1000 && coord.y == 1000) {
			if (!locationz.getReady()) {
				return;
			}
			coord.x = locationz.getLat ();
			coord.y = locationz.getLon ();
		}
		weatherz.GetWeather (data [selected].coord);
	}
}
