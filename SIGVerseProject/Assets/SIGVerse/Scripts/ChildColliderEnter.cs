using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace SIGVerse.Common
{
	public class ChildColliderEnter : MonoBehaviour
	{
		GameObject rootGameObject;

		// Use this for initialization
		void Start()
		{
			rootGameObject = gameObject.transform.root.gameObject;
			//Debug.Log("tename=" + rootGameObject.name);
		}

		void OnCollisionEnter(Collision collision)
		{
			//Debug.Log("tename2=" + rootGameObject.name);
			rootGameObject.SendMessage("RedirectedOnCollisionEnter", collision);
		}
	}
}