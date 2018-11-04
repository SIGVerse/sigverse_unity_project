using SIGVerse.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.SampleScenes.Hsr
{
	public class PlacementChecker : MonoBehaviour
	{
		private float maxWaitingTime = 2.0f;
		
		private Dictionary<GameObject, int> placedObjectMap;


		void Start ()
		{
			this.placedObjectMap = new Dictionary<GameObject, int>();

			CheckExistanceOfColliders(this.transform);
		}

		public void Initialize(float maxWaitingTime)
		{
			this.maxWaitingTime = maxWaitingTime;
		}


		public IEnumerator<bool?> IsPlaced(GameObject targetObj)
		{
			Rigidbody targetRigidbody = targetObj.GetComponent<Rigidbody>();

			float timeLimit = Time.time + this.maxWaitingTime;

			while (!this.IsPlacedNow(targetObj, targetRigidbody) && Time.time < timeLimit)
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


		private static void CheckExistanceOfColliders(Transform transform)
		{
			Collider[] colliders = transform.GetComponents<Collider>();
			
			if(colliders.Length==0)
			{
				SIGVerseLogger.Error("No Colliders on " + SIGVerseUtils.GetHierarchyPath(transform));
				throw new Exception ("No Colliders on " + SIGVerseUtils.GetHierarchyPath(transform));
			}
		}


		private bool IsPlacedNow(GameObject targetObj, Rigidbody targetRigidbody)
		{
			return targetRigidbody.IsSleeping() && this.placedObjectMap.ContainsKey(targetObj) && this.placedObjectMap[targetObj] > 0;
		}


		private void OnTriggerEnter(Collider other)
		{
			if(other.attachedRigidbody==null){ return; }

			GameObject contactedObj = other.attachedRigidbody.gameObject;

			if(!this.placedObjectMap.ContainsKey(contactedObj))
			{
				this.placedObjectMap.Add(contactedObj, 0);
			}

			this.placedObjectMap[contactedObj]++;
		}


		private void OnTriggerExit(Collider other)
		{
			if(other.attachedRigidbody==null){ return; }

			GameObject placedObj = other.attachedRigidbody.gameObject;

			this.placedObjectMap[placedObj]--;
		}
	}
}


