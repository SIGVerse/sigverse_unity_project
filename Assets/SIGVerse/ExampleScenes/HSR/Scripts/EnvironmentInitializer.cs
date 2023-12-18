using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;

namespace SIGVerse.ExampleScenes.Hsr
{
	public class EnvironmentInitializer : MonoBehaviour
	{
		[HeaderAttribute ("Graspable Posture")]
		[Range(0.0f, 1.0f)] public float directionChangingProbability;
		[Range(0.0f, 1.0f)] public float lieDownProbability;

		[HeaderAttribute ("Environment")]
		public List<GameObject> environments;

		//-----------------------------

		private const string TagGraspable         = "Graspable";
		private const string TagGraspablePosition = "GraspablePosition";

		private List<GameObject> graspables;
		private List<GameObject> graspablePositions;


		void Awake()
		{
			try
			{
				UnityEngine.Random.InitState(DateTime.Now.Millisecond);

				this.EnableEnvironments(this.environments);

				// Get graspable objects
				this.graspables = GameObject.FindGameObjectsWithTag(TagGraspable).ToList<GameObject>();

				// Get graspable positions
				this.graspablePositions = GameObject.FindGameObjectsWithTag(TagGraspablePosition).ToList<GameObject>();

				this.PlaceGraspables();

				this.ChangeMaxDepenetrationVelocity();
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				SIGVerseLogger.Error(exception.Message);
				SIGVerseLogger.Error(exception.StackTrace);
			}
		}


		private void EnableEnvironments(List<GameObject> environments)
		{
			if(environments.Count != (from environment in environments select environment.name).Distinct().Count())
			{
				throw new Exception("There is the name conflict of environments.");
			}

			// Get an active environment
			GameObject activeEnvironment = (from environment in environments where environment.activeSelf==true select environment).FirstOrDefault();

			string environmentName;

			// Use active environment preferentially
			if(activeEnvironment!=null)
			{
				environmentName = activeEnvironment.name;

				if(environments.Count != 1) { SIGVerseLogger.Warn("Selected an active environment. name=" + activeEnvironment.name);}
			}
			else
			{
				// Determine the environment randomly
				environmentName = environments[UnityEngine.Random.Range(0, environments.Count)].name;
			}

			// Enable/Disable the environments
			foreach (GameObject environment in environments)
			{
				if(environment.name==environmentName)
				{
					environment.SetActive(true);
				}
				else
				{
					environment.SetActive(false);
				}
			}
		}


		private void PlaceGraspables()
		{
			if (this.graspables.Count == 0){ throw new Exception("Count of Graspables is zero.");}

			// Check the name conflict of graspables.
			if(this.graspables.Count != (from graspable in this.graspables select graspable.name).Distinct().Count())
			{
				throw new Exception("There is the name conflict of graspable objects.");
			}

			SIGVerseLogger.Info("Count of Graspables = "         + this.graspables.Count);
			SIGVerseLogger.Info("Count of GraspablePositions = " + this.graspablePositions.Count);

			if (this.graspables.Count > this.graspablePositions.Count)
			{
				throw new Exception("graspables.Count > graspablePositions.Count.");
			}

			// Deactivate the graspable positions
			foreach (GameObject graspablePosition in this.graspablePositions)
			{
				graspablePosition.SetActive(false);
			}

			// Shuffle lists
			this.graspables         = this.graspables        .OrderBy(i => Guid.NewGuid()).ToList();
			this.graspablePositions = this.graspablePositions.OrderBy(i => Guid.NewGuid()).ToList();

			// Place the graspables
			for (int i=0; i<this.graspables.Count; i++)
			{
				// Create a place point object
				Transform point = (new GameObject("point")).transform; 

				point.parent = this.graspablePositions[i].transform;
				point.localPosition = Vector3.zero;
				point.localRotation = Quaternion.identity;
				point.localScale    = Vector3.one;

				// Decide the position
				BoxCollider boxCollider = this.graspablePositions[i].GetComponent<BoxCollider>();

				float posX = UnityEngine.Random.Range((float)(-0.5*boxCollider.size.x), (float)(+0.5*boxCollider.size.x));
				float posZ = UnityEngine.Random.Range((float)(-0.5*boxCollider.size.z), (float)(+0.5*boxCollider.size.z));
				
				point.localPosition = new Vector3(posX, point.localPosition.y+0.03f, posZ);

				// Rotates with a certain probability
				if(UnityEngine.Random.Range(0.0f, 1.0f) <= this.directionChangingProbability)
				{
					float rotY = UnityEngine.Random.Range(0.0f, 360.0f);

					point.Rotate(0, rotY, 0);
				}

				// Lies down with a certain probability
				if(UnityEngine.Random.Range(0.0f, 1.0f) <= this.lieDownProbability && this.graspablePositions[i].transform.localScale.x > 0.99f)
				{
					Bounds bounds = this.GetBounds(this.graspables[i]);

					// Determine the lying direction
					Vector3 rotDirection = bounds.size.x > bounds.size.z ? new Vector3(90,0,0) : new Vector3(0,0,90);

					if(UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f){ rotDirection *= -1; }

					point.Rotate(rotDirection);

					point.position += new Vector3(0.0f, 0.1f, 0.0f);   // If lying down, float the object a little
				}

				this.graspables[i].transform.position    = point.position;
				this.graspables[i].transform.eulerAngles = point.eulerAngles;

				if(UnityEngine.Random.Range(0.0f, 1.0f) <= this.lieDownProbability && this.graspablePositions[i].transform.localScale.x > 0.99f)
				{
					 // Adjust the position if lying down
					this.graspables[i].transform.Translate(-this.graspables[i].GetComponentInChildren<Rigidbody>().centerOfMass);
				}
			}
		}

		private Bounds GetBounds(GameObject target)
		{
			Quaternion tmpRot = target.transform.rotation;

			// Temporarily match the local coordinates with the world coordinates.
			target.transform.rotation = Quaternion.identity;

			Bounds maxBounds = new Bounds(Vector3.zero, Vector3.zero);

			Collider[] colliders = target.GetComponentsInChildren<Collider>();

			foreach(Collider collider in colliders)
			{
				Bounds bounds = collider.bounds;

				bounds.center -= target.transform.position;

				maxBounds.Encapsulate(bounds);
			}
			
//			Debug.Log("maxBounds size  =" + maxBounds.size.x + ", " + maxBounds.size.y + ", " + maxBounds.size.z);
//			Debug.Log("maxBounds center=" + maxBounds.center.x + ", " + maxBounds.center.y + ", " + maxBounds.center.z);

			target.transform.rotation = tmpRot;

			return maxBounds;
		}


		private void ChangeMaxDepenetrationVelocity()
		{
			foreach (GameObject graspable in this.graspables)
			{
				Rigidbody graspableRigidbody = graspable.GetComponentInChildren<Rigidbody>();

				graspableRigidbody.maxDepenetrationVelocity = 0.1f;

				graspableRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

				StartCoroutine (this.LoosenRigidbodyConstraints(graspableRigidbody));
			}
		}

		private IEnumerator LoosenRigidbodyConstraints(Rigidbody graspableRigidbody)
		{
			yield return new WaitForSeconds (2.0f);

			graspableRigidbody.constraints = RigidbodyConstraints.None;
			graspableRigidbody.AddRelativeTorque(new Vector3(0, 0.01f, 0)); // Give a shock
		}
	}
}

