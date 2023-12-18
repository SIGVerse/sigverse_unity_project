using UnityEngine;
using System.Collections;

namespace SIGVerse.TurtleBot3
{
	[ExecuteInEditMode]
	public class RealSenseSR300_Depth : MonoBehaviour
	{
		public Material mat;

		void Start()
		{
			GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Graphics.Blit(source, destination, mat);
		}
	}
}
