namespace SIGVerse.ROSBridge
{
	[System.Serializable]
	public abstract class ROSMessage
	{
		public virtual string GetMessageType()
		{
			return null;
		}

		public static string GetMD5Hash()
		{
			return null;
		}
	}
}
