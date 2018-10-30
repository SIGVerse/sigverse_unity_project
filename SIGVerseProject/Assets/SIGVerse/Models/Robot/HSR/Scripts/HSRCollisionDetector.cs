using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;
using SIGVerse.Competition;
using UnityEngine.EventSystems;

namespace SIGVerse.ToyotaHSR
{
	public interface IHSRCollisionHandler : IEventSystemHandler
	{
		void OnHsrCollisionEnter(Collision collision, float collisionVelocity, float effectScale);
	}


	public class HSRCollisionDetector : MonoBehaviour
	{
		private const float CollisionInterval = 1.0f; //[s]

		public List<GameObject> collisionNotificationDestinations;

		public List<string> exclusionColliderTags;

		//------------------------

		private GameObject collisionEffect;

		private List<Collider> exclusionColliderList;

		private float collidedTime;

		private Collider[] colliders;
		private float[]    colliderVelocities;
		private Vector3[]  prePoss;

		private AudioSource collisionAudioSource;

		private AudioClip   collisionClip;


		protected void Awake()
		{
			this.collisionEffect = (GameObject)Resources.Load(SIGVerseUtils.CollisionEffectPath);

			this.exclusionColliderList = new List<Collider>();

			foreach(string exclusionColliderTag in exclusionColliderTags)
			{
				List<GameObject> exclusionColliderObjects = GameObject.FindGameObjectsWithTag(exclusionColliderTag).ToList<GameObject>();

				foreach(GameObject exclusionColliderObject in exclusionColliderObjects)
				{
					List<Collider> colliders = exclusionColliderObject.GetComponentsInChildren<Collider>().ToList<Collider>();

					this.exclusionColliderList.AddRange(colliders);
				}
			}

			this.colliders = this.GetComponentsInChildren<Collider>();
			this.colliderVelocities  = new float[this.colliders.Length];
			this.prePoss = new Vector3[this.colliders.Length];

			this.collisionAudioSource = this.transform.root.gameObject.GetComponent<AudioSource>();

			this.collisionClip = (AudioClip)Resources.Load(SIGVerseUtils.CollisionAudioClip2Path);

			SIGVerseLogger.Info("HSR collider count=" + this.colliders.Length);
		}

		// Use this for initialization
		void Start()
		{
			this.collidedTime = 0.0f;

			for(int i=0; i<this.colliders.Length; i++)
			{
				this.colliderVelocities[i] = 0.0f;

				this.prePoss[i] = this.colliders[i].transform.position;
			}
		}

		// Update is called once per frame
		void FixedUpdate()
		{
			for (int i=0; i<this.colliders.Length; i++)
			{
				this.colliderVelocities[i] = (this.colliders[i].transform.position - this.prePoss[i]).magnitude / Time.fixedDeltaTime;

				this.prePoss[i] = this.colliders[i].transform.position;
			}
		}


		void OnCollisionEnter(Collision collision)
		{
			if (Time.time - this.collidedTime < CollisionInterval) { return; }

			if(collision.collider.transform.root==this.transform.root) { return; }

			if(collision.collider.isTrigger) { return; }

			foreach(Collider collider in exclusionColliderList)
			{
				if(collision.collider==collider) { return; }
			}

			this.ExecCollisionProcess(collision);
		}


		private void ExecCollisionProcess(Collision collision)
		{
			float collisionVelocity = this.colliderVelocities[Array.IndexOf(this.colliders, collision.contacts[0].thisCollider)];

			SIGVerseLogger.Info("HSR Collision Detection! Time=" + Time.time + ", Collision Velocity=" + collisionVelocity + 
				", Part=" +collision.contacts[0].thisCollider.name + ", Collided object=" + SIGVerseUtils.GetHierarchyPath(collision.collider.transform));

			// Effect
			GameObject effect = MonoBehaviour.Instantiate(this.collisionEffect);
			
			Vector3 contactPoint = SIGVerseUtils.CalcContactAveragePoint(collision);

			effect.transform.position = contactPoint;
			effect.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

			Destroy(effect, 1.0f);


			// Sound
			this.collisionAudioSource.PlayOneShot(this.collisionClip);


			// Send the collision notification
			foreach(GameObject destination in this.collisionNotificationDestinations)
			{
				ExecuteEvents.Execute<IHSRCollisionHandler>
				(
					target: destination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnHsrCollisionEnter(collision, collisionVelocity,  0.5f)
				);
			}

			this.collidedTime = Time.time;
		}
	}
}

