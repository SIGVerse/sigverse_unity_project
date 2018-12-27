using SIGVerse.Common;
using SIGVerse.ToyotaHSR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static SIGVerse.ToyotaHSR.HSRCommon;

namespace SIGVerse.SampleScenes.Hsr
{
	public enum CollisionType
	{
		Normal,
		WithHsrBase,
	}

	public interface ITransferredCollisionHandler : IEventSystemHandler
	{
		void OnTransferredCollisionEnter(CollisionType collisionType, Collision collision, float collisionVelocity, float effectScale);
	}

	public class CollisionTransferer : MonoBehaviour
	{
		public List<string> exclusionColliderTags = new List<string>{ "NonDeductionCollider" };

		//-----------------------------

		private const string TagRobot = "Robot";
		private const float  InvincibleTime = 1.0f;

		private List<GameObject> destinations;
		private float velocityThreshold;
		private float minimumSendingInterval;
		private AudioSource objectCollisionAudioSource;

		private float lastSendingTime = 1.0f; // Ignore collisions for the first one second.

		private GameObject collisionEffect;

		private List<Collider> hsrBaseColliders;
		private List<Collider> hsrHandColliders;

		private bool hasCollidedWithHsrBase = false;

		private GraspingDetector hsrGraspingDetector;


		protected void Awake()
		{
			this.collisionEffect = (GameObject)Resources.Load(SIGVerseUtils.CollisionEffectPath);

			GameObject robot = GameObject.FindGameObjectWithTag(TagRobot);

			this.hsrBaseColliders = new List<Collider>();
			this.hsrBaseColliders.AddRange(SIGVerseUtils.FindTransformFromChild(robot.transform.root, HSRCommon.BaseName)  .GetComponentsInChildren<Collider>());
			this.hsrBaseColliders.AddRange(SIGVerseUtils.FindTransformFromChild(robot.transform.root, HSRCommon.BumperName).GetComponentsInChildren<Collider>());

			this.hsrHandColliders = new List<Collider>();
			this.hsrHandColliders.AddRange(SIGVerseUtils.FindTransformFromChild(robot.transform.root, Link.wrist_roll_link.ToString()).GetComponentsInChildren<Collider>());

			this.hsrGraspingDetector = robot.GetComponent<GraspingDetector>();
		}


		public void Initialize(List<GameObject> destinations, float velocityThreshold=1.0f, float minimumSendingInterval=0.1f, AudioSource objectCollisionAudioSource=null)
		{
			this.destinations               = destinations;
			this.velocityThreshold          = velocityThreshold;
			this.minimumSendingInterval     = minimumSendingInterval;
			this.objectCollisionAudioSource = objectCollisionAudioSource;
		}

		public void SetExclusionColliderTags(List<string> exclusionColliderTags)
		{
			this.exclusionColliderTags = exclusionColliderTags;
		}


		void OnCollisionEnter(Collision collision)
		{
			// It is run over by HSR base
			if(collision.relativeVelocity.magnitude < this.velocityThreshold)
			{
				if(this.IsRunOverByHsrBase(collision))
				{
					this.ExecCollisionProcess(CollisionType.WithHsrBase, collision);
				}

				return;
			}

			// Ignore when it is collided with hand immediately after release
			if(Time.time - this.hsrGraspingDetector.GetLatestReleaseTime() < InvincibleTime && this.IsCollidedWithHsrHand(collision))
			{
				SIGVerseLogger.Info("Ignore the collision with the HSR hand. Elapsed time since release = " + (Time.time - this.hsrGraspingDetector.GetLatestReleaseTime()));
//				SIGVerseLogger.Info("Ignore the collision with the HSR hand. Velocity="+collision.relativeVelocity.magnitude + ", Elapsed time since release = " + (Time.time - this.hsrGraspingDetector.GetLatestReleaseTime()));
				return;
			}


			// Normal collision
			if(Time.time - this.lastSendingTime < this.minimumSendingInterval){ return; }

			foreach(ContactPoint contactPoint in collision.contacts)
			{
				foreach(string exclusionColliderTag in exclusionColliderTags)
				{
					if(contactPoint.otherCollider.CompareTag(exclusionColliderTag)){ return; }
				}
			}

			this.lastSendingTime = Time.time;

			this.ExecCollisionProcess(CollisionType.Normal, collision);
		}


		private void ExecCollisionProcess(CollisionType collisionType, Collision collision)
		{
			SIGVerseLogger.Info("Object collision occurred. name=" + this.name + " Collided object=" + SIGVerseUtils.GetHierarchyPath(collision.collider.transform) + ", vel="+collision.relativeVelocity);

			// Effect
			GameObject effect = MonoBehaviour.Instantiate(this.collisionEffect);
			
			Vector3 contactPoint = SIGVerseUtils.CalcContactAveragePoint(collision);

			effect.transform.position = contactPoint;
			effect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

			Destroy(effect, 1.0f);

			// Sound
			if(this.objectCollisionAudioSource!=null)
			{
				this.objectCollisionAudioSource.Play();
			}

			foreach(GameObject destination in this.destinations)
			{
				ExecuteEvents.Execute<ITransferredCollisionHandler>
				(
					target: destination,
					eventData: null,
					functor: (reciever, eventData) => reciever.OnTransferredCollisionEnter(collisionType, collision, collision.relativeVelocity.magnitude, 0.1f)
				);
			}
		}

		private bool IsRunOverByHsrBase(Collision collision)
		{
			if(this.hasCollidedWithHsrBase){ return false; }

			foreach(ContactPoint contactPoint in collision.contacts)
			{
				foreach(Collider hsrBaseCollider in this.hsrBaseColliders)
				{
					if(contactPoint.otherCollider==hsrBaseCollider)
					{
						this.hasCollidedWithHsrBase = true;

						return true;
					}
				}
			}

			return false;
		}

		private bool IsCollidedWithHsrHand(Collision collision)
		{
			foreach(ContactPoint contactPoint in collision.contacts)
			{
				if(!this.IsCollidedWithHsrHand(contactPoint))
				{
					return false;
				}
			}

			return true;
		}

		private bool IsCollidedWithHsrHand(ContactPoint contactPoint)
		{
			foreach(Collider hsrHandCollider in this.hsrHandColliders)
			{
				if(contactPoint.otherCollider==hsrHandCollider)
				{
					return true;
				}
			}

			return false;
		}
	}
}

