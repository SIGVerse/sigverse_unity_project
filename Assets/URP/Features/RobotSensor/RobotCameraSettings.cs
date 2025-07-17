using System;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SIGVerse.Common
{
	[RequireComponent(typeof(Camera))]
	public class RobotCameraSettings: MonoBehaviour
	{
		public enum SensorType
		{
			Unknown,
			RGB,
			Depth,
			Sonar
		}

		public enum DepthEncoding
		{
			U16C1,
			F32C1
		}

		public SensorType sensorType = SensorType.Unknown;

		public int width  = 640;
		public int height = 480;
		public GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB;

		public DepthEncoding depthEncoding = DepthEncoding.U16C1;

		// The minimum valid depth value (in meters) used to filter out objects that are too close to the camera.
		// To properly handle near objects, ensure the camera's Near plane is set to a sufficiently small value.
		// This value is typically used in shaders to ignore invalid or unreliable depth data at close range.
		public float depthMinValue = 0.4f;

		public float sonarMinValue = 0.02f;

		void Awake()
		{
			CreateAndAssignRenderTexture();
		}

		protected void CreateAndAssignRenderTexture()
		{
			RenderTextureDescriptor desc = new (this.width, this.height);
			desc.dimension = TextureDimension.Tex2D;
			desc.graphicsFormat = this.colorFormat;
			desc.depthStencilFormat = GraphicsFormat.D16_UNorm;
			desc.msaaSamples = 1; // Anti-aliasing: None
			desc.mipCount = 1;
			desc.useMipMap = false;
			desc.autoGenerateMips = false;
			desc.enableRandomWrite = false;
			desc.sRGB = this.sensorType == SensorType.RGB;
			desc.useDynamicScale = false;

			RenderTexture renderTexture = new RenderTexture(desc);
			renderTexture.wrapMode = TextureWrapMode.Clamp;
			renderTexture.filterMode = FilterMode.Point;
			renderTexture.Create();

			this.GetComponent<Camera>().targetTexture = renderTexture;
		}


#if UNITY_EDITOR
		[CustomEditor(typeof(RobotCameraSettings))]
		public class RobotCameraSettingsEditor: Editor
		{
			private RobotCameraSettings robotCameraSettings;

			SerializedProperty sensorTypeProp;
			SerializedProperty widthProp;
			SerializedProperty heightProp;
			SerializedProperty colorFormatProp;
			SerializedProperty depthEncodingProp;
			SerializedProperty depthMinValueProp;
			SerializedProperty sonarMinValueProp;

			void OnEnable()
			{
				this.robotCameraSettings = base.target as RobotCameraSettings;

				this.sensorTypeProp = serializedObject.FindProperty(nameof(sensorType));
				this.widthProp = serializedObject.FindProperty(nameof(width));
				this.heightProp = serializedObject.FindProperty(nameof(height));
				this.colorFormatProp = serializedObject.FindProperty(nameof(colorFormat));
				this.depthEncodingProp = serializedObject.FindProperty(nameof(depthEncoding));
				this.depthMinValueProp = serializedObject.FindProperty(nameof(depthMinValue));
				this.sonarMinValueProp = serializedObject.FindProperty(nameof(sonarMinValue));
			}

			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				EditorGUILayout.PropertyField(sensorTypeProp, new GUIContent("Sensor Type"));

				SensorType sensorType = (SensorType)sensorTypeProp.enumValueIndex;

				if (sensorType == SensorType.Unknown)
				{
					EditorGUILayout.HelpBox("Please select a renderer for the robot sensor on the camera object.", MessageType.Warning);
				}
				else
				{
					EditorGUILayout.LabelField("RenderTexture Settings", EditorStyles.boldLabel);

					EditorGUILayout.PropertyField(widthProp, new GUIContent("Width"));
					EditorGUILayout.PropertyField(heightProp, new GUIContent("Height"));
					EditorGUILayout.PropertyField(colorFormatProp, new GUIContent("Color Format"));

					if (sensorType == SensorType.Depth)
					{
						EditorGUILayout.LabelField("Depth Settings", EditorStyles.boldLabel);
						EditorGUILayout.PropertyField(depthEncodingProp, new GUIContent("Depth Encoding"));
						EditorGUILayout.PropertyField(depthMinValueProp, new GUIContent("Depth Min Value"));
					}
					else if (sensorType == SensorType.Sonar)
					{
						EditorGUILayout.LabelField("Sonar Settings", EditorStyles.boldLabel);
						EditorGUILayout.PropertyField(sonarMinValueProp, new GUIContent("Sonar Min Value"));
					}
				}

				serializedObject.ApplyModifiedProperties();

				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.FlexibleSpace();

					if (GUILayout.Button("[Debug]\nCreate RenderTexture", GUILayout.Width(200), GUILayout.Height(40)))
					{
						Undo.RecordObject(target, "Create RenderTexture");

						CreateAndAssignDebugRenderTexture(this.robotCameraSettings);
					}

					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();
			}

			private void CreateAndAssignDebugRenderTexture(RobotCameraSettings cameraSettings)
			{
				string folderPath = "Assets/AssetStoreTools";
				string fileName = "DebugRenderTexture.renderTexture";

				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
					AssetDatabase.Refresh();
				}

				// Create RenderTexture asset
				cameraSettings.CreateAndAssignRenderTexture();
				RenderTexture debugRT = cameraSettings.GetComponent<Camera>().targetTexture;
				debugRT.name = fileName;

				// Save to asset
				AssetDatabase.CreateAsset(debugRT, Path.Combine(folderPath, fileName));
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}
#endif
	}
}
