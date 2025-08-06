namespace SIGVerse.RosBridge
{
	namespace msg_helpers
	{
		[System.Serializable]
		public class Time
		{
//			public int secs;
//			public int nsecs;
			public int sec;
			public uint nanosec;

			public Time()
			{
				sec = 0;
				nanosec = 0;
			}

			public Time(int seconds, uint nanoseconds)
			{
				sec = seconds;
				nanosec = nanoseconds;
			}
		}

		[System.Serializable]
		public class Duration : Time
		{
			public Duration() : base() { }
			public Duration(int seconds, uint nanoseconds) : base(seconds, nanoseconds) { }
		}
	}
}

