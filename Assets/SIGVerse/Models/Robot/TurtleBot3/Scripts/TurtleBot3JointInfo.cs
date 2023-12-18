using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LinkType  = SIGVerse.TurtleBot3.TurtleBot3LinkInfo.LinkType;

namespace SIGVerse.TurtleBot3
{
	public class TurtleBot3JointInfo
	{
		public enum JointType
		{
			WheelLeftJoint,
			WheelRightJoint,
			Joint1,
			Joint2,
			Joint3,
			Joint4,
			GripJoint,
			GripJointSub,
		}

		public enum MovementType
		{
			Angular,
			Linear,
		}

		public enum MovementAxis
		{
			PlusX,  PlusY,  PlusZ,
			MinusX, MinusY, MinusZ,
		}

		public JointType    jointType;
//		public string       jointName;
		public LinkType     linkType;
		public MovementType movementType;
		public MovementAxis movementAxis;
		public float        minVal;
		public float        maxVal;


		public TurtleBot3JointInfo(JointType jointType, LinkType linkType, MovementType movementType, MovementAxis movementAxis, float minVal, float maxVal)
		{
			this.jointType    = jointType;
//			this.jointName    = jointName;
			this.linkType     = linkType;
			this.movementType = movementType;
			this.movementAxis = movementAxis;
			this.minVal       = minVal;
			this.maxVal       = maxVal;
		}
	}
}

