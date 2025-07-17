using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.RosBridge.std_msgs;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.Common;
using System.Collections.Generic;

namespace SIGVerse.TIAGo
{
	public class TIAGoPubSonar : RosPubMessage<Range>
	{
		[TooltipAttribute("milliseconds")]
		public float sendingInterval = 100;

		public Camera[] cameras;

		[HeaderAttribute("Sonar Parameters")]
		public byte radiationType = Range.ULTRASOUND;

		//--------------------------------------------------

		private List<Range> ranges = new List<Range>();
		private List<Texture2D> imageTextures = new List<Texture2D>();

		private uint headerSeq = 0; // The sequence number of the message header is shared

		private float elapsedTime= 0.0f;


		protected override void Start()
		{
			base.Start();

			foreach(Camera camera in this.cameras)
			{
				RobotCameraSettings robotCameraSettings = camera.GetComponent<RobotCameraSettings>();

				Range range = new Range();
				range.header = new Header(0, new SIGVerse.RosBridge.msg_helpers.Time(0, 0), camera.transform.parent.gameObject.name);
				range.radiation_type = this.radiationType;
				range.field_of_view  = camera.fieldOfView * Mathf.Deg2Rad ;
				range.min_range      = robotCameraSettings.sonarMinValue;
				range.max_range      = camera.farClipPlane;

				this.ranges.Add(range);
				this.imageTextures.Add(new Texture2D(robotCameraSettings.width, robotCameraSettings.height, TextureFormat.RGB24, false));
			}
		}

		protected override void Update()
		{
			base.Update();

			this.elapsedTime += UnityEngine.Time.deltaTime;

			if (this.elapsedTime < this.sendingInterval * 0.001)
			{
				return;
			}

			this.elapsedTime = 0.0f;

			for(int i=0; i<this.cameras.Length; i++)
			{
				this.headerSeq++;

				RenderTexture.active = cameras[i].targetTexture;

				imageTextures[i].ReadPixels(new Rect(0, 0, this.imageTextures[i].width, this.imageTextures[i].height), 0, 0, false);
				imageTextures[i].Apply();

				byte[] rawTextureData = imageTextures[i].GetRawTextureData();

				float minRange = this.ranges[i].max_range; 

				for (int row = 0; row < this.imageTextures[i].height; row++)
				{
					for (int col = 0; col < this.imageTextures[i].width; col++)
					{
						int index = row * this.imageTextures[i].width + col;

						float val = (rawTextureData[index * 3 + 1] * 256 + rawTextureData[index * 3 + 0]) / 1000.0f;

						if(val < minRange)
						{
							minRange = val;
						}
					}
				}

//				Debug.Log("sonar["+i+"]="+minRange);

				// Measures for rounding errors
				if(minRange > this.ranges[i].max_range * 98 / 100)
				{
					minRange = this.ranges[i].max_range;
				}

				this.ranges[i].header.Update();
				this.ranges[i].header.seq = this.headerSeq;
				this.ranges[i].range = minRange;

				this.publisher.Publish(this.ranges[i]);
			}
		}
	}
}
