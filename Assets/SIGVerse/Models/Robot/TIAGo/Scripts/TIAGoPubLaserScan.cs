using UnityEngine;

using System;
using SIGVerse.RosBridge.sensor_msgs;
using SIGVerse.Common;

namespace SIGVerse.Common
{
	public class TIAGoPubLaserScan : RobotPubLaserScan
	{
		/// <summary>
		/// Initialize laserScan
		/// </summary>
		protected override void InitializeVariables()
		{
			// TiM 571

			float halfOfLaserAngle = 110.0f; // [deg]
			int numLines = 666;

			this.laserScan = new LaserScanForSIGVerseBridge();

			this.laserScan.header = this.header;

			this.laserScan.angle_min = -halfOfLaserAngle * Mathf.Deg2Rad;
			this.laserScan.angle_max = +halfOfLaserAngle * Mathf.Deg2Rad;
			this.laserScan.angle_increment = halfOfLaserAngle * 2 / (numLines-1) * Mathf.Deg2Rad;
			this.laserScan.time_increment = 0.0;
			this.laserScan.scan_time = 0.0;
			this.laserScan.range_min = 0.05;
			this.laserScan.range_max = 25.0;
			this.laserScan.ranges      = new double[numLines];
			this.laserScan.intensities = new double[numLines];
		}
	}
}
