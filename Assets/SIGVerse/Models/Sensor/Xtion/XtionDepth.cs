using UnityEngine;
using System.Collections;

namespace SIGVerse.Xtion
{
	[ExecuteInEditMode]
	public class XtionDepth : MonoBehaviour
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
