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

		private void appendToRequest(string toAppend)
		{
			this.request += toAppend + "&";
		}
		
		public TidesAndCurrentsRequestBuilder withRange(int hours)
		{
			appendToRequest("range=" + hours.ToString());
			return this;
		}

		public TidesAndCurrentsRequestBuilder atStation(int station) {
			appendToRequest("station=" + station.ToString ());
			return this;
		}

		public TidesAndCurrentsRequestBuilder getPredictions() {
			appendToRequest("product=predictions");
			return this;
		}

		public TidesAndCurrentsRequestBuilder getCurrents() {
			appendToRequest("product=currents");
			return this;
		}


		public TidesAndCurrentsRequestBuilder withStandardUnitsAndTimeZone() {
			appendToRequest("units=english&time_zone=lst_ldt");
			return this;
		}

		public TidesAndCurrentsRequestBuilder provideApplicationName() {
			appendToRequest("application=koorbloh");
			return this;
		}

		public TidesAndCurrentsRequestBuilder forDatum(string datum) {
			appendToRequest("datum=" + datum);
			return this;
		}

		public TidesAndCurrentsRequestBuilder specifyFormat(string format) {
			appendToRequest("format=" + format);
			return this;
		}

		public TidesAndCurrentsRequestBuilder withBeginDate(DateTime beginDate) {
			appendToRequest("begin_date=" + beginDate.ToShortDateString());
			return this;
		}

		public TidesAndCurrentsRequestBuilder withEndDate(DateTime endDate) {
			appendToRequest("end_date=" + endDate.ToShortDateString());
			return this;
		}

		public string getRequest() {
			return request.Substring(0, request.Length - 1);
		}

		private string request;
	}
}