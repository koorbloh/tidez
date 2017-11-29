using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Graphz {

	private Texture2D texture;
	private int axisHeight;
	private DateTime graphStartTime;
	private int imageDimensionsX;
	private int imageDimensionsY;

	public class DataPoint {
		public DataPoint(int _y, Color _color) {
			this.y = _y;
			this.color = _color;
		}
		public int y;
		public Color color;
	}

	public delegate float GraphzData (DateTime time);
	public delegate List<DataPoint> ExtraGraphz ();

	public Graphz(Texture2D _texture, int _axisHeight, DateTime _graphStartTime, int _imageDimensionsX, int _imageDimensionsY) {
		this.texture = _texture;
		this.axisHeight = _axisHeight;
		this.graphStartTime = _graphStartTime;
		this.imageDimensionsX = _imageDimensionsX;
		this.imageDimensionsY = _imageDimensionsY;
	}

	public void graphData(GraphzData dataDelegate, ExtraGraphz extraPointsDelegate) {
		for (int y = 0; y < texture.height; y++)
		{
			for (int x = 0; x < texture.width; x++)
			{
				Color color = Color.white;
				texture.SetPixel(x, y, color);
			}
		}


		int minutesPerPixel = (2 * 24 * 60)/imageDimensionsX;
		DateTime lastLine = graphStartTime;
		DateTime now = DateTime.Now;
		//for every x pixel
		bool flipper = false;
		for (int x = 0; x < texture.width; ++x) {

			List<DataPoint> dataPoints = extraPointsDelegate ();
			for (int i = 0; i < dataPoints.Count; ++i) {
				texture.SetPixel (x, dataPoints [i].y, dataPoints [i].color);
			}

			//find a y pixel
			float feet = dataDelegate(graphStartTime.AddMinutes(minutesPerPixel * x));
			texture.SetPixel (x, (int)feet + axisHeight-1, Color.blue);
			texture.SetPixel (x, (int)feet + axisHeight, Color.blue);
			texture.SetPixel (x, (int)feet + axisHeight+1, Color.blue);
			texture.SetPixel (x, (int)feet + axisHeight+2, Color.blue);
			//if it's been 4 hours since we last drew a line, draw one
			if (graphStartTime.AddMinutes(minutesPerPixel * x) > lastLine.AddHours(4)) {
				for (int y = 0; y < imageDimensionsY; ++y) {
					if (flipper) 
						texture.SetPixel (x, y, Color.black);
					else 
						texture.SetPixel (x, y, Color.grey);
				}
				flipper = !flipper;
				lastLine = graphStartTime.AddMinutes (minutesPerPixel * x);
			}

			//if it's basically now, draw a red line
			if (graphStartTime.AddMinutes(minutesPerPixel * (x-1)) < now && now < graphStartTime.AddMinutes(minutesPerPixel * x)) {
				for (int y = 0; y < imageDimensionsY; ++y) {
					texture.SetPixel (x, y, Color.red);
					texture.SetPixel (x+1, y, Color.red);
					texture.SetPixel (x-1, y, Color.red);
				}
			}
		}

		texture.Apply();		
	}

}
