using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SIGVerse.SampleScenes.Hsr
{
	public interface IPanelNoticeHandler : IEventSystemHandler
	{
		void OnPanelNoticeChange(PanelNoticeStatus panelNoticeStatus);
	}
	
	public class PanelNoticeStatus
	{
		public static readonly Color Green = new Color(  0/255f, 143/255f, 36/255f, 255/255f);
		public static readonly Color Red   = new Color(255/255f,   0/255f,  0/255f, 255/255f);

		public string Message  { get; set; }
		public int    FontSize { get; set; }
		public Color  Color    { get; set; }
		public float  Duration { get; set; }

		public PanelNoticeStatus(string message, int fontSize, Color color, float duration)
		{
			this.Message  = message;
			this.FontSize = fontSize;
			this.Color    = color;
			this.Duration = duration;
		}

		public PanelNoticeStatus(PanelNoticeStatus panelNoticeStatus)
		{
			this.Message  = panelNoticeStatus.Message;
			this.FontSize = panelNoticeStatus.FontSize;
			this.Color    = panelNoticeStatus.Color;
			this.Duration = panelNoticeStatus.Duration;
		}
	}


	public class PanelNoticeController : MonoBehaviour, IPanelNoticeHandler
	{
		public GameObject noticePanel;

		private Text noticeText;
		private float hideTime = 0.0f;

		void Awake()
		{
			this.noticeText = this.noticePanel.GetComponentInChildren<Text>();
		}

		void Start()
		{
			this.noticePanel.SetActive(false);
		}

		private void ShowNotice(PanelNoticeStatus panelNoticeStatus)
		{
			this.noticePanel.SetActive(true);

			noticeText.text     = panelNoticeStatus.Message;
			noticeText.fontSize = panelNoticeStatus.FontSize;
			noticeText.color    = panelNoticeStatus.Color;

			this.hideTime = UnityEngine.Time.time + panelNoticeStatus.Duration;

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

