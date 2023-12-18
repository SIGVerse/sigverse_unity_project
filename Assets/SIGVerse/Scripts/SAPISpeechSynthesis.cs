using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_SAPI
using Google.Cloud.Translation.V2;
#endif

namespace SIGVerse.Common
{
	public class SAPISpeechSynthesis : MonoBehaviour
	{
		public const string SAPISpeechSynthesisName = "SAPISpeechSynthesis";

		public enum Language
		{
			English, Japanese,
		}

		[HeaderAttribute("SAPI")]
		public Language language = Language.English;
		public string path = "/../TTS/ConsoleSimpleTTS.exe";
		public string gender = "Female";

		[HeaderAttribute("Guidance message param")]
		public int maxCharactersForSourceLang = 1000;
		public int maxCharactersForTargetLang = 400;

		public bool canSpeak = true;

		private bool isSpeaking;

		private System.Diagnostics.Process speechProcess;

#if USE_SAPI
		TranslationClient translationClient;

		// Use this for initialization
		void Awake()
		{
			this.speechProcess = new System.Diagnostics.Process();

			this.speechProcess.StartInfo.FileName = Application.dataPath + this.path;

			this.speechProcess.StartInfo.CreateNoWindow = true;
			this.speechProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

			SIGVerseLogger.Info("Text-To-Speech: " + this.speechProcess.StartInfo.FileName);

			//this.ResetNotificationDestinations();

			this.isSpeaking = false;

			try
			{
				this.translationClient = TranslationClient.Create();
			}
			catch (Exception)
			{
				this.translationClient = null;
			}
		}

		void Update()
		{
			if (this.isSpeaking && this.speechProcess.HasExited)
			{
				this.isSpeaking = false;
			}
		}

		public bool Speak(string message, string sourceLanguage=null, string targetLanguage=null)
		{
			if (!this.canSpeak) { return false; }

			if (this.isSpeaking)
			{
				SIGVerseLogger.Info("Text-To-Speech: isSpeaking");

				try
				{
					if (/*isTaskFinished &&*/ !this.speechProcess.HasExited)
					{
						this.speechProcess.Kill();
					}
				}
				catch (Exception)
				{
					SIGVerseLogger.Warn("Couldn't terminate the speech process, but do nothing.");
					// Do nothing even if an error occurs
				}

			}

			// Translation
			if ((sourceLanguage == null && targetLanguage != null) || (sourceLanguage != null && targetLanguage == null))
			{
				SIGVerseLogger.Error("Invalid language type. Source Language=" + sourceLanguage + ", Target Language="+ targetLanguage);
				return false;
			}
			
			if (sourceLanguage != null && targetLanguage != null)
			{
				if (message.Length > maxCharactersForSourceLang)
				{
					message.Substring(0, maxCharactersForSourceLang);
					SIGVerseLogger.Info("Length of guidance message(source lang) is over " + this.maxCharactersForSourceLang.ToString() + " charcters.");
				}

				if(this.translationClient!=null)
				{
					message = this.translationClient.TranslateText(message, targetLanguage, sourceLanguage).TranslatedText;
				}
				else
				{
					SIGVerseLogger.Warn("There is no environment for translation.");
				}
			}

			string truncatedMessage;

			if (message.Length > maxCharactersForTargetLang)
			{
				truncatedMessage = message.Substring(0, maxCharactersForTargetLang);
				SIGVerseLogger.Info("Length of guidance message(target lang) is over " + this.maxCharactersForTargetLang.ToString() + " charcters.");
			}
			else
			{
				truncatedMessage = message;
			}

			string languageId = string.Empty;

			switch(language)
			{
				case Language.English : languageId = "409"; break;
				case Language.Japanese: languageId = "411"; break;
				default: throw new Exception("Not supported language. Lang="+language);
			}

			// speak
			string settings = "Language=" + languageId + "; Gender=" + this.gender;

			this.speechProcess.StartInfo.Arguments = "\"" + truncatedMessage + "\" \"" + settings + "\"";

			SIGVerseLogger.Info("Speech Message="+ this.speechProcess.StartInfo.Arguments);

			this.speechProcess.Start();

			this.isSpeaking = true;

			return true;
		}

		public bool IsSpeaking()
		{
			return this.isSpeaking;
		}

#else
		public bool Speak(string message, string sourceLanguage=null, string targetLanguage=null)
		{
			return false;
		}
#endif

	}
}
