using System.Collections;
using System.Text;

namespace SIGVerse.RosBridge
{
	public class RosBridgeMsg
	{
		public static string AdvertiseTopic(string messageTopic, string messageType)
		{
			return "{\"op\" : \"advertise\", \"topic\": \"" + messageTopic + "\", \"type\": \"" + messageType + "\"}";
		}

		public static string UnAdvertiseTopic(string messageTopic)
		{
			return "{\"op\" : \"unadvertise\", \"topic\": \"" + messageTopic + "\"}";
		}

		public static string AdvertiseService(string serviceName, string serviceType)
		{
			return @"{""op"" : ""advertise_service"", ""type"" : """ + serviceType + @""", ""service"" : """ + serviceName + @"""}";
		}

		public static string UnadvertiseService(string serviceName)
		{
			return @"{""op"" : ""unadvertise_service"", ""service"" : """ + serviceName + @"""}";
		}

		public static string ServiceResponse(bool success, string serviceName, string id, string resultValues)
		{
			return @"{""op"": ""service_response"", ""id"": """ + id + @""", ""service"": """ + serviceName + @""", ""values"": " + resultValues + @", ""result"": " + success.ToString().ToLower() + @"}";
		}

		public static string Subscribe(string messageTopic)
		{
			return "{\"op\" : \"subscribe\", \"topic\": \"" + messageTopic + "\"}";
		}

		public static string Subscribe(string messageTopic, string messageType)
		{
			return "{\"op\" : \"subscribe\", \"topic\": \"" + messageTopic + "\", \"type\": \"" + messageType + "\"}";
		}

		public static string UnSubscribe(string messageTopic)
		{
			return "{\"op\" : \"unsubscribe\", \"topic\": \"" + messageTopic + "\"}";
		}
	}
}
