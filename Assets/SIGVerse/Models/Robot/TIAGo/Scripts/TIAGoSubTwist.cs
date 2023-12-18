using UnityEngine;
using SIGVerse.RosBridge;
using SIGVerse.Common;


namespace SIGVerse.TIAGo
{
	public class TIAGoSubTwist : RobotSubTwist
	{
		protected override void InitializeVariables()
		{
			this.baseFootprint          = SIGVerseUtils.FindTransformFromChild(this.transform.root, TIAGoCommon.Link.base_footprint.ToString());
			this.baseFootprintPosNoise  = SIGVerseUtils.FindTransformFromChild(this.transform.root, TIAGoCommon.BaseFootPrintPosNoiseName);
			this.baseFootprintRotNoise  = SIGVerseUtils.FindTransformFromChild(this.transform.root, TIAGoCommon.BaseFootPrintRotNoiseName);
			this.baseFootprintRigidbody = SIGVerseUtils.FindTransformFromChild(this.transform.root, TIAGoCommon.BaseFootPrintRigidbodyName);

			this.maxSpeedBase    = TIAGoCommon.MaxSpeedBase;
			this.maxSpeedBaseRad = TIAGoCommon.MaxSpeedBaseRad;
		}
	}
}

