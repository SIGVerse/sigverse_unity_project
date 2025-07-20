using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

namespace SIGVerse.Common
{
	/// <summary>
	/// Only the graspables are hightlighted.
	/// </summary>
	public class GraspableHighlighter : MonoBehaviour
	{
		public const float SelectedEffectSize = 8.0f;
		public static readonly Color SelectedEffectColor = Color.red;

		private const string TagGraspable = "Graspable";
		private const CameraEvent CameraEventTarget = CameraEvent.AfterForwardAlpha;

		public bool visualizeRenderTarget;

		public Camera targetCamera;
		public Shader highlightImageEffect;
		public Shader highlightMesh;

		public float effectSize = 4.0f;
		public Color effectColor = Color.yellow;

		public bool IsDisabled { set; get; } = false;

//		public bool shouldFillIn = true;

		private Material imageEffectMaterial;
		private Material meshRenderMaterial;

		private List<Renderer> highlightedRenderers = new List<Renderer>();

		private GameObject selectedObj = null;
		private Material selectedMaterial;

		private CommandBuffer commandBuffer;
		private int _HighlightTextureId;

		private void Awake()
		{
			this.imageEffectMaterial = new Material(this.highlightImageEffect);
			this.imageEffectMaterial.SetFloat("_Size", this.effectSize);
			this.imageEffectMaterial.SetColor("_ColorMain", this.effectColor);
			this.imageEffectMaterial.SetColor("_ColorOccluded", new Color(0f, 0f, 0f, 0.3f));

			this.meshRenderMaterial = new Material(this.highlightMesh);
			this.meshRenderMaterial.SetFloat("_DepthBias", 0.002f);

			this.selectedMaterial = new Material(this.imageEffectMaterial);
			this.selectedMaterial.SetFloat("_Size", SelectedEffectSize);
			this.selectedMaterial.SetColor("_ColorMain", SelectedEffectColor);
		}

		public bool CanGrasp(GameObject obj)
		{
			if(this.selectedObj==obj)
			{
				return true;
			}
			else
			{
				this.SelectObject(obj);

				return false;
			}
		}

		private bool SelectObject(GameObject obj)
		{
			foreach (Renderer highlightedRenderer in this.highlightedRenderers)
			{
				if (highlightedRenderer.gameObject == obj)
				{
					this.selectedObj = obj;
					this.Rebuild();

//					Debug.Log("Select: " + this.selectedObj.name);
					return true;
				}
			}
			return false;
		}

		public void ClearSelectedObj()
		{
			this.selectedObj = null;
			this.Rebuild();
		}

		public bool ClearAll()
		{
			if ( highlightedRenderers.Count == 0 ) return false;

			highlightedRenderers.Clear();
			this.selectedObj = null;
			return true;
		}

		public bool AddRenderer( Renderer renderer )
		{
			if ( highlightedRenderers.Contains( renderer ) ) return false;

			highlightedRenderers.Add( renderer );
			return true;
		}

		public bool RemoveRenderer( Renderer renderer )
		{
			if ( !highlightedRenderers.Contains( renderer ) ) return false;

			highlightedRenderers.Remove( renderer );
			if (this.selectedObj!=null && renderer==this.selectedObj.GetComponentInChildren<Renderer>()) { this.selectedObj = null; }
			return true;
		}

		public bool Rebuild()
		{
//			if ( isDirty || force )
			{
				RebuildCommandBuffer();
				return true;
			}

//			return false;
		}

		private void Start()
		{
			_HighlightTextureId = Shader.PropertyToID( "_HighlightTexture" );
		}

		/// <summary>
		/// Install the command buffer on enable
		/// </summary>
		void OnEnable()
		{
			RebuildCommandBuffer();
		}


		/// <summary>
		/// Remove the CommandBuffer on disable to disable the effect
		/// </summary>
		private void OnDisable()
		{
			ReleaseAll();
		}

		void RebuildCommandBuffer()
		{
			if (this.IsDisabled) { return; }

			if ( targetCamera == null )
				return;

			//
			// Create the buffer if it doesn't exist
			//
			if ( commandBuffer == null )
			{
				commandBuffer = new CommandBuffer();
				commandBuffer.name = "Highlight Render";
				targetCamera.AddCommandBuffer( CameraEventTarget, commandBuffer );
			}

			if ( this.meshRenderMaterial == null || this.imageEffectMaterial == null )
				return;

			commandBuffer.Clear();
			commandBuffer.GetTemporaryRT( _HighlightTextureId, targetCamera.pixelWidth, targetCamera.pixelHeight, 24, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1 );

			foreach ( var highlightedRenderer in this.highlightedRenderers )
			{
				if ( !highlightedRenderer ) continue;

				commandBuffer.SetRenderTarget( _HighlightTextureId );
				commandBuffer.ClearRenderTarget( true, true, Color.clear);
				commandBuffer.DrawRenderer( highlightedRenderer, this.meshRenderMaterial );

				commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

				if(highlightedRenderer.gameObject==this.selectedObj)
				{
					commandBuffer.Blit(_HighlightTextureId, BuiltinRenderTextureType.CameraTarget, visualizeRenderTarget ? null : this.selectedMaterial);
				}
				else
				{
					commandBuffer.Blit(_HighlightTextureId, BuiltinRenderTextureType.CameraTarget, visualizeRenderTarget ? null : this.imageEffectMaterial);
				}
				
			}

//			commandBuffer.Blit(_HighlightTextureId, _HighlightTextureId, imageEffectMaterial);
//			commandBuffer.Blit(_HighlightTextureId, BuiltinRenderTextureType.CameraTarget);

			commandBuffer.ReleaseTemporaryRT( _HighlightTextureId );
		}


		/// <summary>
		/// Release anything that has been created
		/// </summary>
		public void ReleaseAll()
		{
			if ( targetCamera != null && commandBuffer != null )
			{
				targetCamera.RemoveCommandBuffer( CameraEventTarget, commandBuffer );
				commandBuffer = null;
			}
		}


		/// <summary>
		/// For editor only, refresh shit when messing in the inspector
		/// </summary>
		private void OnValidate()
		{
			if ( targetCamera == null )
			{
				targetCamera = GetComponent<Camera>();
			}

			if ( targetCamera != null && targetCamera.depthTextureMode == DepthTextureMode.None )
			{
				targetCamera.depthTextureMode = DepthTextureMode.Depth;
			}

			ReleaseAll();
			RebuildCommandBuffer();
		}

		void OnTriggerEnter(Collider other)
		{
			if (other.attachedRigidbody == null) { return; }

//			Debug.Log(other.name + ", other.attachedRigidbody="+other.attachedRigidbody);

			if(other.attachedRigidbody.tag==TagGraspable)
			{
				MeshRenderer renderer = other.attachedRigidbody.GetComponentInChildren<MeshRenderer>();

				if(renderer!=null)
				{
//					Debug.Log("AddRenderer Name="+other.name);
					this.AddRenderer(renderer);
					this.Rebuild();
				}
				else
				{
					Debug.LogError("No MeshRenderer. Name="+other.attachedRigidbody.name);
				}
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (other.attachedRigidbody == null) { return; }

			if(other.attachedRigidbody.tag==TagGraspable)
			{
				MeshRenderer renderer = other.attachedRigidbody.GetComponentInChildren<MeshRenderer>();

				if(renderer!=null)
				{
//					Debug.Log("RemoveRenderer Name="+other.name);
					this.RemoveRenderer(renderer);
					this.Rebuild();
				}
			}
		}
	}
}