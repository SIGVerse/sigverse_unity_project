using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.ExampleScenes.Hsr
{
	public class AvatarMeshUpdater : MonoBehaviour
	{
		// Use this for initialization
		void Start ()
		{
			SkinnedMeshRenderer skinnedMeshRenderer = this.GetComponent<SkinnedMeshRenderer>();
			MeshCollider meshCollider = this.GetComponent<MeshCollider>();

			Mesh colliderMesh = new Mesh();
			skinnedMeshRenderer.BakeMesh(colliderMesh);
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = colliderMesh;
		}
	
		//// Update is called once per frame
		//void Update ()
		//{	
		//}
	}
}
