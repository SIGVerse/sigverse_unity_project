using System;
using UnityEngine;

namespace SIGVerse.ROSBridge
{
	/// <summary>
	/// Base class for all Parameters passed to ServiceProviders
	/// Inherit from this using members which match your ROS Service defintion
	/// in both TYPE and NAME.
	/// Can be used as-is for services which do not take parameters.
	/// </summary>
	[Serializable]
	public class ServiceArgs : ROSMessage
	{
	}

	/// <summary>
	/// Base class for all Response values passed from ServiceProviders back to the caller
	/// Inherit from this using members which match your ROS Service definition
	/// in both TYPE and NAME.
	/// Can be used as-is for services which do not generate responses.
	/// </summary>
	[Serializable]
	public class ServiceResponse : ROSMessage
	{
	}

	/// <summary>
	/// Base for all ServiceProviders.
	/// </summary>
	public abstract class ROSBridgeServiceProvider
	{
		protected string name;
		protected string type;

		public string Name
		{
			get { return name; }
		}

		public string Type
		{
			get { return type; }
		}

		public ROSBridgeServiceProvider(string serviceName)
		{
			this.name = serviceName;
		}

		public ROSBridgeServiceProvider(string serviceName, string typeName)
		{
			this.name = serviceName;
			this.type = typeName;
		}

		public abstract ServiceArgs ParseRequest(string request);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TArgs">The ServiceArgs class you intend to use with this Service</typeparam>
	public class ROSBridgeServiceProvider<TArgs> : ROSBridgeServiceProvider where TArgs : ServiceArgs
	{
		public ROSBridgeServiceProvider(string serviceName) : base(serviceName)
		{
		}

		public ROSBridgeServiceProvider(string serviceName, string typeName) : base(serviceName, typeName)
		{
		}

		public override ServiceArgs ParseRequest(string request)
		{
			return JsonUtility.FromJson<TArgs>(request);
		}
	}
}
