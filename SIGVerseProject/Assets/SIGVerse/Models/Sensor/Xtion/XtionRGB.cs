using UnityEngine;
using System.Collections;

namespace SIGVerse.Xtion
{
	[ExecuteInEditMode]
	public class XtionRGB : MonoBehaviour
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
