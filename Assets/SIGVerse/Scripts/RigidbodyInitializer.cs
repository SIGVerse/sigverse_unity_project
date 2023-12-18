using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SIGVerse.Common
{
	[DisallowMultipleComponent]
	public class RigidbodyInitializer : MonoBehaviour
	{
		public bool    setCenterOfMass = false;
		public Vector3 centerOfMass;

		public bool  setMaxDepenetrationVelocity = false;
		public float maxDepenetrationVelocity;

		//--------------------------------------------------

		private Rigidbody targetRigidbody;

		void Awake()
		{
			if (setCenterOfMass)
			{
				this.targetRigidbody = this.GetComponent<Rigidbody>();

				this.targetRigidbody.centerOfMass = this.centerOfMass;
			}
			if (setMaxDepenetrationVelocity)
			{
				this.targetRigidbody = this.GetComponent<Rigidbody>();

				this.targetRigidbody.maxDepenetrationVelocity = this.maxDepenetrationVelocity;
			}
		}


#if UNITY_EDITOR
		[CustomEditor(typeof(RigidbodyInitializer))]
		public class RigidbodyInitializerEditor : Editor
		{
			SerializedProperty setCenterOfMass;
			SerializedProperty centerOfMass;

			SerializedProperty setMaxDepenetrationVelocity;
			SerializedProperty maxDepenetrationVelocity;

			void OnEnable()
			{
				setCenterOfMass = serializedObject.FindProperty("setCenterOfMass");
				centerOfMass    = serializedObject.FindProperty("centerOfMass");

				setMaxDepenetrationVelocity = serializedObject.FindProperty("setMaxDepenetrationVelocity");
				maxDepenetrationVelocity    = serializedObject.FindProperty("maxDepenetrationVelocity");
			}

			public override void OnInspectorGUI()
			{
				serializedObject.Update();

				EditorGUILayout.PropertyField(setCenterOfMass);

				if (setCenterOfMass.boolValue)
				{
					EditorGUILayout.PropertyField(centerOfMass);
				}

				EditorGUILayout.PropertyField(setMaxDepenetrationVelocity);

				if (setMaxDepenetrationVelocity.boolValue)
				{
					EditorGUILayout.PropertyField(maxDepenetrationVelocity);
				}

				serializedObject.ApplyModifiedProperties();
			}
		}
#endif
	}
}


