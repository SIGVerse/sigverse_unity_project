using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PR2LaserTiltController : MonoBehaviour
{
	private const int MaxTiltPosition = +45;
	private const int MinTiltPosition = -90;
		
	private const float MinTiltSpeed = 1f;
	private const float MaxTiltSpeed = 572.9f; // =10[rad/s]

	// -------------

	public Transform laserTiltLink;

	[TooltipAttribute("[deg]")] public int highestTiltPosition = +40;
	[TooltipAttribute("[deg]")] public int lowestTiltPosition  = -40;

	[TooltipAttribute("[deg/s]")] public float upSpeed   = 40f;
	[TooltipAttribute("[deg/s]")] public float downSpeed = 40f;

	private bool isRunning = true;

	private float speed;

	private void OnValidate()
	{
		this.highestTiltPosition = Mathf.Clamp(this.highestTiltPosition, MinTiltPosition, MaxTiltPosition);
		this.lowestTiltPosition  = Mathf.Clamp(this.lowestTiltPosition,  MinTiltPosition, MaxTiltPosition);

		this.upSpeed   = Mathf.Clamp(this.upSpeed,   MinTiltSpeed, MaxTiltSpeed);
		this.downSpeed = Mathf.Clamp(this.downSpeed, MinTiltSpeed, MaxTiltSpeed);
	}
	
	// Start is called before the first frame update
	void Start()
	{
		if(this.lowestTiltPosition >= this.highestTiltPosition)
		{
			Debug.LogError("Lowest Tilt Position("+this.lowestTiltPosition+") >= Highest Tilt Position("+this.highestTiltPosition+")!");

			this.isRunning = false;
		}

		this.speed = this.upSpeed;
	}

	void FixedUpdate()
	{
		if(!this.isRunning){ return; }

		float nextPos = this.laserTiltLink.localEulerAngles.y + this.speed * Time.fixedDeltaTime;

		if(nextPos > 180f){ nextPos-=360f; }

		if(nextPos >= this.highestTiltPosition) { nextPos = this.highestTiltPosition; this.speed = -this.downSpeed; }
		if(nextPos <= this.lowestTiltPosition)  { nextPos = this.lowestTiltPosition;  this.speed = +this.upSpeed; }

		this.laserTiltLink.localEulerAngles = new Vector3(0f, nextPos, 0f);
	}
}

