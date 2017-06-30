using UnityEngine;
using UnityEngine.EventSystems;

namespace SIGVerse.SampleScenes.Playbacker
{
	public class PlaybackerPanelManager : MonoBehaviour, IDragHandler
	{
		public RectTransform mainPanel;

		public void OnDrag(PointerEventData eventData)
		{
			this.mainPanel.transform.position += (Vector3)eventData.delta;
		}
	}
}

