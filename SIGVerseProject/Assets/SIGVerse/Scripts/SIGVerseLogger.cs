using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;

namespace SIGVerse.Common
{
	/// <summary>
	/// Logger class of SIGVerse.
	/// 
	/// Basically, you can use the default logger as follows. (Path of log file is ./SIGVerse.log)
	///   SIGVerseLogger.Info("Info log");
	///   SIGVerseLogger.Warn("warn log");
	///   SIGVerseLogger.Error("error log");
	///   
	/// By adding a new log group, you can also write a log to an another file.
	///   string logGroupAAA = "AAA"
	///   SIGVerseLogger.AddLogGroup(logGroupAAA, "./aaa/logAAA.log");
	///   SIGVerseLogger.Info(logGroupAAA, "Info log AAA");
	///   SIGVerseLogger.Warn(logGroupAAA, "warn log AAA");
	///   SIGVerseLogger.Error(logGroupAAA, "error log AAA");
	/// </summary>
	public class SIGVerseLogger : MonoBehaviour
	{
		// Default logger information
		private const string SIGVerseGroupName     = "SIGVerse";
		private const string SIGVerseGroupFilePath = SIGVerseGroupName + ".log";

		private const int LogInterval = 300; // [ms]

		public enum LogLevel
		{
			Info, 
			Warn, 
			Error,
		}

		private static string LogLevelToString(LogLevel loglevel)
		{
			string[] names = { "INFO ", "WARN ", "ERROR" };
			return names[(int)loglevel];
		}

		/// <summary>
		/// Information of one log data.
		/// </summary>
		private class LogData
		{
			public LogLevel LogLevel { get; set; }
			public DateTime Time     { get; set; }
			public object   Message  { get; set; }

			public LogData(LogLevel logLevel, DateTime time, object message)
			{
				this.LogLevel = logLevel;
				this.Time     = time;
				this.Message  = message;
			}

			public LogData(LogLevel logLevel, object message) : this(logLevel, DateTime.Now, message){ }
		}

		/// <summary>
		/// Information of one log group.
		/// </summary>
		private class LogGroup
		{
			public LogLevel       LogLevel    { get; set; }
			public Queue<LogData> DataQueue   { get; set; }
			public string         FilePath    { get; set; }
			public StreamWriter   LogWriter   { get; set; }

			public LogGroup(string filePath)
			{
				this.LogLevel     = LogLevel.Info;
				this.DataQueue    = new Queue<LogData>();
				this.FilePath     = filePath;
				this.LogWriter    = new StreamWriter(filePath, false);
			}
		}

		private static Dictionary<string, LogGroup> logGroupMap = new Dictionary<string, LogGroup>();

		private static bool isFinishing;


		void OnApplicationQuit()
		{
			if (!SIGVerseLogger.isFinishing)
			{
				SIGVerseLogger.isFinishing = true;
			}
		}


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init()
		{
			// Create a new game object and attach this class to the game object.
			GameObject sigverseLoggerObj = new GameObject("SIGVerseLogger");
			sigverseLoggerObj.AddComponent<SIGVerseLogger>();
			DontDestroyOnLoad(sigverseLoggerObj);

			SIGVerseLogger.isFinishing = false;

			if (!AddLogGroup(SIGVerseGroupName, SIGVerseGroupFilePath)) { return; }

			Info("SIGVerseLogger Start");
		}

		/// <summary>
		/// Create the log writing thread.
		/// </summary>
		/// <param name="logGroupName"></param>
		/// <param name="logFilePath"></param>
		/// <returns>Creating thread succeeded or not</returns>
		private static bool CreateLogWritingThread(string logGroupName, string logFilePath)
		{
			try
			{
				string directoryName = Path.GetDirectoryName(logFilePath);

				if (directoryName != string.Empty)
				{
					if (!Directory.Exists(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Cannot create " + logFilePath + ". Exception=" + exception.Message);
				return false;
			}

			try
			{
				LogGroup logGroup = new LogGroup(logFilePath);

				SIGVerseLogger.logGroupMap.Add(logGroupName, logGroup);

				// Create a thread to write logs, and start.
				Thread writingLogThread = new Thread(new ParameterizedThreadStart(WriteLogInAnotherThread));
				writingLogThread.Start(logGroupName);
			}
			catch (Exception exception)
			{
				Debug.LogError("Cannot create the log writing thread. Log group name=" + logGroupName + ". Exception=" + exception.Message);

				if (SIGVerseLogger.logGroupMap.ContainsKey(logGroupName)) { SIGVerseLogger.logGroupMap.Remove(logGroupName); }
				return false;
			}
			return true;
		}


		/// <summary>
		/// Write the logs into the file in an another thread.
		/// This function is passed as an argument to Thread class.
		/// </summary>
		/// <param name="args">Arguments object</param>
		private static void WriteLogInAnotherThread(object args)
		{
			try
			{
				string logGroupName = (string)args;

				DateTime preTime = DateTime.Now;

				LogGroup logGroup = SIGVerseLogger.logGroupMap[logGroupName];

				while (true)
				{
					int sleepTime = SIGVerseLogger.LogInterval - (int)DateTime.Now.Subtract(preTime).TotalMilliseconds;

					if(sleepTime > 0)
					{
						Thread.Sleep(sleepTime);
					}
					preTime = DateTime.Now;

					Queue<LogData> logDataQueue = null;

					lock(SIGVerseLogger.logGroupMap[logGroupName])
					{
						if(logGroup.DataQueue.Count > 0)
						{
							logDataQueue = logGroup.DataQueue;
							logGroup.DataQueue = new Queue<LogData>();
						}
					}

					if(logDataQueue!=null)
					{
						WriteLog(logGroup.LogWriter, logDataQueue);
					}

					// Write a final message and finish the thread.
					if (SIGVerseLogger.isFinishing)
					{
						if (logGroupName == SIGVerseGroupName && logGroup.LogLevel <= LogLevel.Info)
						{
							LogData logData = new LogData(LogLevel.Info, "Application Quit");

							logDataQueue = new Queue<LogData>();
							logDataQueue.Enqueue(logData);
							WriteLog(logGroup.LogWriter, logDataQueue);
						}
						logGroup.LogWriter.Close();
						return;
					}
				}
			}
			catch(Exception exception)
			{
				Debug.LogError("Exception occured in "+MethodBase.GetCurrentMethod().Name+". Exception="+exception.Message);
			}
		}

		/// <summary>
		/// Write a log using StreamWriter and a queue of log data.
		/// </summary>
		/// <param name="streamWriter"></param>
		/// <param name="logDataQueue"></param>
		private static void WriteLog(StreamWriter streamWriter, Queue<LogData> logDataQueue)
		{
			if(logDataQueue.Count == 0) { return; }

			while (logDataQueue.Count != 0)
			{
				LogData logData = logDataQueue.Dequeue();
				streamWriter.WriteLine("[" + logData.Time.ToString("yyyy/MM/dd HH:mm:ss") + "]["+LogLevelToString(logData.LogLevel)+"] "+logData.Message);
			}
			streamWriter.Flush();
		}


		/// <summary>
		/// Add a new log group.
		/// </summary>
		/// <param name="logGroupName"></param>
		/// <param name="logFilePath"></param>
		/// <returns></returns>
		public static bool AddLogGroup(string logGroupName, string logFilePath)
		{
#if (!UNITY_STANDALONE && !UNITY_EDITOR)
			return false;
#endif
			if(SIGVerseLogger.logGroupMap.ContainsKey(logGroupName)){ return false; }

			foreach(KeyValuePair<string, LogGroup> logGroupPair in SIGVerseLogger.logGroupMap)
			{
				if(logFilePath==logGroupPair.Value.FilePath)
				{
					Warn("Cannot execute " + MethodBase.GetCurrentMethod().Name + "! The file path conflicts with another group. Group name=" +logGroupName+", Conflicting Group name="+logGroupPair.Key);
					return false;
				}
			}

			if (!CreateLogWritingThread(logGroupName, logFilePath)) { return false; }

			Info("New LogGroup was added. Group name=["+logGroupName+"] File path=["+logFilePath+"]");

			return true;
		}


		/// <summary>
		/// Change the log level.
		/// </summary>
		/// <param name="logGroupName"></param>
		/// <param name="logLevel"></param>
		/// <returns></returns>
		public static bool ChangeLogLevel(string logGroupName, LogLevel logLevel)
		{
			if (!ExistsLogGroup(logGroupName, MethodBase.GetCurrentMethod().Name)) { return false; }

			if (logLevel==SIGVerseLogger.logGroupMap[logGroupName].LogLevel) { return false; }

			Info(logGroupName, "LogLevel was changed. ["+LogLevelToString(SIGVerseLogger.logGroupMap[logGroupName].LogLevel)+"]->["+LogLevelToString(logLevel)+"]");

			lock(SIGVerseLogger.logGroupMap[logGroupName])
			{
				SIGVerseLogger.logGroupMap[logGroupName].LogLevel = logLevel;
			}

			return true;
		}

		/// <summary>
		/// Change the log level for the default logger.
		/// </summary>
		/// <param name="logLevel"></param>
		/// <returns></returns>
		public static bool ChangeLogLevel(LogLevel logLevel)
		{
			return ChangeLogLevel(SIGVerseGroupName, logLevel);
		}


		/// <summary>
		/// Log info.
		/// </summary>
		/// <param name="logGroupName"></param>
		/// <param name="message"></param>
		public static void Info(string logGroupName, string message)
		{
			if (!ExistsLogGroup(logGroupName, MethodBase.GetCurrentMethod().Name)) { return; }

			if (SIGVerseLogger.logGroupMap[logGroupName].LogLevel > LogLevel.Info) { return; }

			LogData logData = new LogData(LogLevel.Info, message);

			lock(SIGVerseLogger.logGroupMap[logGroupName])
			{
				SIGVerseLogger.logGroupMap[logGroupName].DataQueue.Enqueue(logData);
				Debug.Log(message);
			}
		}

		/// <summary>
		/// Log info for the default logger.
		/// </summary>
		/// <param name="message"></param>
		public static void Info(string message)
		{
			Info(SIGVerseGroupName, message);
		}


		/// <summary>
		/// Log warning info.
		/// </summary>
		/// <param name="logGroupName"></param>
		/// <param name="message"></param>
		public static void Warn(string logGroupName, string message)
		{
			if (!ExistsLogGroup(logGroupName, MethodBase.GetCurrentMethod().Name)) { return; }

			if (SIGVerseLogger.logGroupMap[logGroupName].LogLevel > LogLevel.Warn) { return; }

			LogData logData = new LogData(LogLevel.Warn, message);

			lock(SIGVerseLogger.logGroupMap[logGroupName])
			{
				SIGVerseLogger.logGroupMap[logGroupName].DataQueue.Enqueue(logData);
				Debug.LogWarning(message);
			}
		}

		/// <summary>
		/// Log warning info for the default logger.
		/// </summary>
		/// <param name="message"></param>
		public static void Warn(string message)
		{
			Warn(SIGVerseGroupName, message);
		}


		/// <summary>
		/// Log error info.
		/// </summary>
		/// <param name="logGroupName"></param>
		/// <param name="message"></param>
		public static void Error(string logGroupName, string message)
		{
			if(!ExistsLogGroup(logGroupName, MethodBase.GetCurrentMethod().Name)) { return; }

			LogData logData = new LogData(LogLevel.Error, message);

			lock(SIGVerseLogger.logGroupMap[logGroupName])
			{
				SIGVerseLogger.logGroupMap[logGroupName].DataQueue.Enqueue(logData);
				Debug.LogError(message);
			}
		}

		/// <summary>
		/// Log error info for the default logger.
		/// </summary>
		/// <param name="message"></param>
		public static void Error(string message)
		{
			Error(SIGVerseGroupName, message);
		}


		/// <summary>
		/// Get the log group exists or not.
		/// </summary>
		/// <param name="logGroupName"></param>
		/// <param name="methodName">Method name of caller function</param>
		/// <returns></returns>
		private static bool ExistsLogGroup(string logGroupName, string methodName)
		{
			if(!SIGVerseLogger.logGroupMap.ContainsKey(logGroupName))
			{
				string warningMessage = "Cannot execute " + methodName+"! Have to call AddLogGroup before use. Group name="+logGroupName;

				if(logGroupName!=SIGVerseGroupName)
				{
					Warn(warningMessage);
				}
				else
				{
					Debug.Log(warningMessage);
				}
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}

