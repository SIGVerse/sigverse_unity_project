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
		}

		void OnCollisionEnter(Collision collision)
		{
			rootGameObject.SendMessage("RedirectedOnCollisionEnter", collision);
		}
	}
}