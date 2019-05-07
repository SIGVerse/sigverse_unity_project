using SIGVerse.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.SampleScenes.Hsr
{
	public class PlacementChecker : MonoBehaviour
	{
		public enum JudgeType
		{
			On, In, 
		}

		// The velocity to judge if it has entered a box(e.g. trash can).
		private const float ThresholdVelocity = 0.5f;

		private bool isInitialized = false;

		private JudgeType judgeType;

		private float maxWaitingTime = 2.0f;
		
		private Dictionary<Rigidbody, int> placedRigidbodyMap;


		void Start ()
		{
			this.placedRigidbodyMap = new Dictionary<Rigidbody, int>();

			if(this.transform.GetComponents<Collider>().Length==0)
			{
				SIGVerseLogger.Error("No Colliders on " + SIGVerseUtils.GetHierarchyPath(this.transform));
				throw new Exception ("No Colliders on " + SIGVerseUtils.GetHierarchyPath(this.transform));
			}
		}

		public void Initialize(JudgeType judgeType, float maxWaitingTime = 2.0f)
		{
			this.judgeType      = judgeType;
			this.maxWaitingTime = maxWaitingTime;

			this.isInitialized = true;
		}


		public IEnumerator<bool?> IsPlaced(GameObject targetObj)
		{
			if(!this.isInitialized){ throw new Exception("Please use PlacementChecker.Initialize()."); }

			Rigidbody targetRigidbody = targetObj.GetComponent<Rigidbody>();

			float timeLimit = Time.time + this.maxWaitingTime;

			while (!this.IsPlacedNow(targetRigidbody) && Time.time < timeLimit)
			{
				yield return null;
			}
		
			if(Time.time < timeLimit)
			{
				yield return true;
			}
			else
			{
				SIGVerseLogger.Info("Target placement failed: Time out.");

				yield return false;
			}
		}


		public bool IsPlacedNow(Rigidbody targetRigidbody)
		{
			if(!this.isInitialized){ throw new Exception("Please use PlacementChecker.Initialize()."); }

			switch(this.judgeType)
			{
				case JudgeType.On :
				{
					return this.placedRigidbodyMap.ContainsKey(targetRigidbody) && this.placedRigidbodyMap[targetRigidbody] > 0 && targetRigidbody.IsSleeping();
				}
				case JudgeType.In :
				{
					return this.placedRigidbodyMap.ContainsKey(targetRigidbody) && this.placedRigidbodyMap[targetRigidbody] > 0 && targetRigidbody.velocity.magnitude < ThresholdVelocity;
				}
				default:
				{
					throw new Exception("Illegal JudgeType (IsPlacedNow) class=" + this.GetType().Name);
				}
			}
		}


		private void OnTriggerEnter(Collider other)
		{
			if(other.attachedRigidbody==null){ return; }

			if(!this.placedRigidbodyMap.ContainsKey(other.attachedRigidbody))
			{
				this.placedRigidbodyMap.Add(other.attachedRigidbody, 0);
			}

			this.placedRigidbodyMap[other.attachedRigidbody]++;
		}


		private void OnTriggerExit(Collider other)
		{
			if(other.attachedRigidbody==null){ return; }

			this.placedRigidbodyMap[other.attachedRigidbody]--;
		}
	}
}


