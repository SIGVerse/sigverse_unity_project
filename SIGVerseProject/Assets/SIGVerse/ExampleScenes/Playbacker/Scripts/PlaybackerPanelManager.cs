using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.ExampleScenes.Playbacker
{
	public class PlaybackerPanelManager : MonoBehaviour, IDragHandler
	{
		public RectTransform hiddingButton;
		public RectTransform mainPanel;

		public void OnDrag(PointerEventData eventData)
		{
			this.hiddingButton.transform.position += (Vector3)eventData.delta;
			this.mainPanel    .transform.position += (Vector3)eventData.delta;
		}

		public void OnHiddingButtonClick()
		{
			if(this.mainPanel.gameObject.activeSelf)
			{
				this.mainPanel.gameObject.SetActive(false);
			}
			else
			{
				this.mainPanel.gameObject.SetActive(true);
			}
		}
	}
}

