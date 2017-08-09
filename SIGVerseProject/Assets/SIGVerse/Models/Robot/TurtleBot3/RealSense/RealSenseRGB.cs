using UnityEngine;
using System.Collections;

namespace SIGVerse.TurtleBot3
{
	[ExecuteInEditMode]
	public class RealSenseRGB : MonoBehaviour
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
