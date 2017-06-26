using System;

namespace NOAAWeathers
{
	[System.Serializable]
	class PointData_properties {
		public string forecast;
		public string forecastGridData;
	}

	[System.Serializable]
	class PointData {
		public PointData_properties properties;
	}

	[System.Serializable]
	class PointForecastData_period {
		public int number;
		public string name;
		public string startTime;
		public string endTime;
		public bool isDaytime;
		public float temperature;
		public string temperatureUnit;
		public string temperatureTrend;
		public string windSpeed;
		public string windDirection;
		public string icon;
		public string shortForecast;
		public string detailedForecast;
	}

	[System.Serializable]
	class PointForecastData_properties {
		public PointForecastData_period[] periods;
	}

	[System.Serializable]
	class PointForecastData {
		public PointForecastData_properties properties;	
	}

	[System.Serializable]
	class GridPointForecastData_windWaveHeight_values {
		public string validTime;
		public float value;
	}

	[System.Serializable]
	class GridPointForecastData_windWaveHeight {
		public string sourceUnit;
		public string uom;
		public GridPointForecastData_windWaveHeight_values[] values;
	}

	[System.Serializable]
	class GridPointForecastData_properties {
		public GridPointForecastData_windWaveHeight waveHeight;
		public GridPointForecastData_windWaveHeight windWaveHeight;
	}

	[System.Serializable]
	class GridPointForecastData {
		public GridPointForecastData_properties properties;
	}

	class NOAAWeatherRequestBuilder {

		static string BASE_URL = "https://api.weather.gov/";

		public string getUrlForPoint(float latitude, float longitude) {
			return BASE_URL + "points/" + latitude.ToString() + "," + longitude.ToString();
		}

		public string getUrlForPointForecast(PointData pointData) {
			if (pointData == null || pointData.properties == null) {
				return "";
			}
			return pointData.properties.forecast;
		}

		public string getUrlForGridPoint(PointData pointData) {
			if (pointData == null || pointData.properties == null) {
				return "";
			}
			return pointData.properties.forecastGridData;
		}
	}
}

