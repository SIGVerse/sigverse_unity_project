using UnityEngine;
using System.Collections;

namespace SIGVerse.TurtleBot3
{
	[ExecuteInEditMode]
	public class RealSenseR200_RGB : MonoBehaviour
	{
		public Material mat;

		void Start()
		{
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Graphics.Blit(source, destination, mat);
		}
	}
}
