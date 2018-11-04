using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SIGVerse.Common;

namespace SIGVerse.SampleScenes.Hsr
{
	public class EnvironmentInitializer : MonoBehaviour
	{
		public List<GameObject> environments;

		//-----------------------------

		private const string TagGraspable         = "Graspable";
		private const string TagGraspablePosition = "GraspablePosition";

		void Awake()
		{
			try
			{
				this.EnableEnvironments(this.environments);

				this.PlaceGraspables();
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

			GameObject activeEnvironment = (from environment in environments where environment.activeSelf==true select environment).FirstOrDefault();

			string environmentName;

			if(activeEnvironment!=null)
			{
				environmentName = activeEnvironment.name;

				SIGVerseLogger.Warn("Selected an active environment. name=" + activeEnvironment.name);
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
			// Get graspable positions
			List<GameObject> graspables = GameObject.FindGameObjectsWithTag(TagGraspable).ToList<GameObject>();

			if (graspables.Count == 0){ throw new Exception("Count of Graspables is zero.");}

			// Check the name conflict of graspables.
			if(graspables.Count != (from graspable in graspables select graspable.name).Distinct().Count())
			{
				throw new Exception("There is the name conflict of graspable objects.");
			}

			SIGVerseLogger.Info("Count of Graspables = " + graspables.Count);

			// Get graspable positions
			List<GameObject> graspablePositions = GameObject.FindGameObjectsWithTag(TagGraspablePosition).ToList<GameObject>();

			if (graspables.Count > graspablePositions.Count)
			{
				throw new Exception("graspables.Count > graspablePositions.Count.");
			}
			else
			{
				SIGVerseLogger.Info("Count of graspablePositions = " + graspablePositions.Count);
			}

			// Deactivate the graspable positions
			foreach (GameObject graspablePosition in graspablePositions)
			{
				graspablePosition.SetActive(false);
			}

			// Shuffle lists
			graspables         = graspables        .OrderBy(i => Guid.NewGuid()).ToList();
			graspablePositions = graspablePositions.OrderBy(i => Guid.NewGuid()).ToList();

			// Place the graspables
			for (int i=0; i<graspables.Count; i++)
			{
				graspables[i].transform.position    = graspablePositions[i].transform.position - new Vector3(0, graspablePositions[i].transform.localScale.y * 0.49f, 0);
				graspables[i].transform.eulerAngles = graspablePositions[i].transform.eulerAngles;
			}
		}
	}
}

