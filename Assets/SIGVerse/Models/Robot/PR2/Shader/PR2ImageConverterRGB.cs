using UnityEngine;
using System.Collections;

namespace SIGVerse.PR2
{
	[ExecuteInEditMode]
	public class PR2ImageConverterRGB : MonoBehaviour
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
