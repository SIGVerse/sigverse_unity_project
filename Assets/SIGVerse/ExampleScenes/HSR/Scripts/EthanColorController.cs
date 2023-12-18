using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.ExampleScenes.Hsr
{
	public class EthanColorController : MonoBehaviour
	{
		public float emissionColorBrightness = 0.05f;


		// Use this for initialization
		void Start ()
		{
			SkinnedMeshRenderer meshRenderer = this.GetComponent<SkinnedMeshRenderer>();

			Material[] materials = meshRenderer.materials;

			foreach(Material material in materials)
			{
				material.EnableKeyword("_EMISSION");
				material.SetColor("_EmissionColor", new Color(emissionColorBrightness, emissionColorBrightness, emissionColorBrightness));
			}
		}
	}
}
