namespace SIGVerse.RosBridge
{
	namespace msg_helpers
	{
		[System.Serializable]
		public class Time
		{
			public int secs;
			public int nsecs;

			public Time()
			{
				secs = 0;
				nsecs = 0;
			}

			public Time(int seconds, int nanoseconds)
			{
				secs = seconds;
				nsecs = nanoseconds;
			}
		}

		[System.Serializable]
		public class Duration : Time
		{
			public Duration() : base() { }
			public Duration(int seconds, int nanoseconds) : base(seconds, nanoseconds) { }
		}
	}
}

