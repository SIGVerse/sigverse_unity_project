using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SIGVerse.SampleScenes.Hsr
{
	[RequireComponent(typeof (AudioSource))]
	public class CollisionTransfererInitializer : MonoBehaviour
	{
		public List<string> exclusionColliderTags = new List<string>{ "NonDeductionCollider" };

		public List<string> targetTags;

		public List<GameObject> collisionNotificationDestinations;

		public float velocityThreshold = 1.0f;
		public float minimumSendingInterval = 0.1f;

		//------------------------

		private List<GameObject> targets;

		private AudioSource objectCollisionAudioSource;

		void Awake()
		{
			this.targets = new List<GameObject>();

			foreach(string targetTag in this.targetTags)
			{
				this.targets.AddRange(GameObject.FindGameObjectsWithTag(targetTag).ToList<GameObject>());
			}

			this.objectCollisionAudioSource = this.GetComponent<AudioSource>();
		}

		// Use this for initialization
		void Start()
		{
			foreach(GameObject target in this.targets)
			{
				CollisionTransferer collisionTransferer = target.AddComponent<CollisionTransferer>();

				collisionTransferer.SetExclusionColliderTags(exclusionColliderTags);

				collisionTransferer.Initialize(this.collisionNotificationDestinations, this.velocityThreshold, this.minimumSendingInterval, this.objectCollisionAudioSource);
			}
		}
	}
}

