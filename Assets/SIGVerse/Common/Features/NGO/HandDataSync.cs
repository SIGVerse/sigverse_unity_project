using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;
using Unity.Collections;

namespace SIGVerse.Common
{
	public class HandDataSync : NetworkBehaviour
	{
		private NetworkVariable<float> leftHandPostureRatio  = new(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
		private NetworkVariable<float> rightHandPostureRatio = new(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

		public void SetLeftHandPostureRatio(float val)
		{
			this.leftHandPostureRatio.Value = val;
		}

		public void SetRightHandPostureRatio(float val)
		{
			this.rightHandPostureRatio.Value = val;
		}

		public float GetLeftHandPostureRatio()
		{
			return this.leftHandPostureRatio.Value;
		}

		public float GetRightHandPostureRatio()
		{
			return this.rightHandPostureRatio.Value;
		}
	}
}

