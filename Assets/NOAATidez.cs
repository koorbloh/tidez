using System;
using System.Collections.Generic;
using UnityEngine;


namespace NOAA {

	[System.Serializable]
	public class TidePrediction
	{
		public string t; //time
		public string v; //level is v?  sure, that makes sense
	}

	[System.Serializable]
	public class TidePredictions 
	{
		public List<TidePrediction> predictions;
	}

	public class TidesAndCurrentsRequestBuilder {

		public static string BASE_URL = "https://tidesandcurrents.noaa.gov/api/datagetter?";

		public TidesAndCurrentsRequestBuilder() {
			this.request = BASE_URL;
		}

		public TidesAndCurrentsRequestBuilder(string request) {
			this.request = request + "&";
		}

		public TidesAndCurrentsRequestBuilder withRange(int hours) {
			return new TidesAndCurrentsRequestBuilder(request + "range=" + hours.ToString ());
		}

		public TidesAndCurrentsRequestBuilder atStation(int station) {
			return new TidesAndCurrentsRequestBuilder(request + "station=" + station.ToString ());
		}

		public TidesAndCurrentsRequestBuilder getPredictions() {
			return new TidesAndCurrentsRequestBuilder(request + "product=predictions");
		}

		public TidesAndCurrentsRequestBuilder getCurrents() {
			return new TidesAndCurrentsRequestBuilder(request + "product=currents");
		}


		public TidesAndCurrentsRequestBuilder withStandardUnitsAndTimeZone() {
			return new TidesAndCurrentsRequestBuilder(request + "units=english&time_zone=lst_ldt");
		}

		public TidesAndCurrentsRequestBuilder provideApplicationName() {
			return new TidesAndCurrentsRequestBuilder(request + "application=koorbloh");
		}

		public TidesAndCurrentsRequestBuilder forDatum(string datum) {
			return new TidesAndCurrentsRequestBuilder(request + "datum=" + datum);
		}

		public TidesAndCurrentsRequestBuilder specifyFormat(string format) {
			return new TidesAndCurrentsRequestBuilder (request + "format=" + format);
		}

		public TidesAndCurrentsRequestBuilder withBeginDate(DateTime beginDate) {
			return new TidesAndCurrentsRequestBuilder (request + "begin_date=" + beginDate.ToShortDateString());
		}

		public TidesAndCurrentsRequestBuilder withEndDate(DateTime endDate) {
			return new TidesAndCurrentsRequestBuilder (request + "end_date=" + endDate.ToShortDateString());
		}

		public string getRequest() {
			return request.Substring(0, request.Length - 1);
		}

		string request;
	}
}