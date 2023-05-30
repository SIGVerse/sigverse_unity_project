using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SIGVerse.ExampleScenes.Hsr
{
	public interface IPanelNoticeHandler : IEventSystemHandler
	{
		void OnPanelNoticeChange(PanelNoticeStatus panelNoticeStatus);
	}
	
	public class PanelNoticeStatus
	{
		public static readonly Color Green = new Color(  0/255f, 143/255f, 36/255f, 255/255f);
		public static readonly Color Red   = new Color(255/255f,   0/255f,  0/255f, 255/255f);

		public string Speaker  { get; set; }
		public string Message  { get; set; }
		public Color  Color    { get; set; }
		public PanelNoticeStatus(string speaker, string message, Color color)
		{
			this.Speaker  = speaker;
			this.Message  = message;
			this.Color    = color;
		}

		public PanelNoticeStatus(PanelNoticeStatus panelNoticeStatus)
		{
			this.Speaker  = panelNoticeStatus.Speaker;
			this.Message  = panelNoticeStatus.Message;
			this.Color    = panelNoticeStatus.Color;
		}
	}


	public class PanelNoticeController : MonoBehaviour, IPanelNoticeHandler
	{
		public GameObject noticePanel;

		private TMP_Text speakerText;
		private TMP_Text messageText;

		private float hideTime = 0.0f;

		void Awake()
		{
			this.speakerText = this.noticePanel.transform.Find("SpeakerText").GetComponent<TMP_Text>();
			this.messageText = this.noticePanel.transform.Find("MessageText").GetComponent<TMP_Text>();
		}

		void Start()
		{
			this.noticePanel.SetActive(false);
		}

		private void ShowNotice(PanelNoticeStatus panelNoticeStatus)
		{
			this.noticePanel.SetActive(true);

			int fontSize   = (panelNoticeStatus.Message.Length < 20)? 150 : 80;
			float duration = (panelNoticeStatus.Message.Length < 20)? 2f : 10f;

			this.speakerText.text  = panelNoticeStatus.Speaker;
			this.speakerText.color = panelNoticeStatus.Color;

			this.messageText.text     = panelNoticeStatus.Message;
			this.messageText.fontSize = fontSize;
			this.messageText.color    = panelNoticeStatus.Color;

			this.hideTime = UnityEngine.Time.time + duration;

			StartCoroutine(this.HideNotice()); // Hide
		}

		private IEnumerator HideNotice()
		{
			while(UnityEngine.Time.time < this.hideTime)
			{
				yield return null;
			}

			this.noticePanel.SetActive(false);
		}

		public void OnPanelNoticeChange(PanelNoticeStatus panelNoticeStatus)
		{
			this.ShowNotice(panelNoticeStatus);
		}
	}
}

