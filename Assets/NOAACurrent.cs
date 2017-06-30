using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NOAACurrent {

	public class CurrentDataPoint {
		public DateTime timestamp;
		public string stage;
		public float knots;

		public void fillInFromString(string source) {
			string[] parts = source.Split (',');
			timestamp = DateTime.Parse (parts [0]);
			stage = parts [1];
			if (!float.TryParse(parts[2], out knots)) {
				knots = 0.0f;
			}
		}
	}

	public class CurrentData {

		public List<CurrentDataPoint> dataPoints = new List<CurrentDataPoint> ();
		public CurrentData(string filePath) {
			TextAsset asset = Resources.Load (filePath) as TextAsset;	

			string[] lines = asset.text.Split ("\n"[0]);
			//i starts at 1 because the first line is headings, and suck it
			for (int i = 1; i < lines.Length; ++i) {
				CurrentDataPoint dataPoint = new CurrentDataPoint ();
				dataPoint.fillInFromString (lines [i]);
				dataPoints.Add (dataPoint);
			}
		}

	}

}

