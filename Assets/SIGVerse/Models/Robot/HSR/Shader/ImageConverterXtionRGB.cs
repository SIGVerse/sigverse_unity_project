using UnityEngine;
using System.Collections;

namespace SIGVerse.ToyotaHSR
{
	[ExecuteInEditMode]
	public class ImageConverterXtionRGB : MonoBehaviour
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
