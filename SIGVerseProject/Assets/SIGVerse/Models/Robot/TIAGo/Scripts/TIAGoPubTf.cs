using UnityEngine;

using System;
using System.Collections;
using SIGVerse.Common;

namespace SIGVerse.TIAGo
{
	public class TIAGoPubTf : RobotPubTf
	{
		protected override void InitializeVariables()
		{
			this.odomName                   = TIAGoCommon.OdomName;
			this.baseFootprintName          = TIAGoCommon.Link.base_footprint.ToString();
			this.baseFootprintRigidbodyName = TIAGoCommon.BaseFootPrintRigidbodyName;

			this.linkList = TIAGoCommon.GetLinksInChildren(this.transform.root);
		}
	}
}

