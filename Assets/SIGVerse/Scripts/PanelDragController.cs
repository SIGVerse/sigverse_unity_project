using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace SIGVerse.Common
{
	public class PanelDragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[HeaderAttribute("Panels")]
		public List<GameObject> panels;

		//---------------------------------------------------
		private GameObject draggingPanel;

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.pointerEnter == null) { return; }

			Transform selectedObj = eventData.pointerEnter.transform;

			do
			{
				if (this.IsPanelSelected(selectedObj.gameObject))
				{
					this.draggingPanel = selectedObj.gameObject;
					break;
				}

				selectedObj = selectedObj.transform.parent;

			} while (selectedObj.transform.parent != null);
		}

		private bool IsPanelSelected(GameObject selectedObj)
		{
			foreach(GameObject panel in this.panels)
			{
				if (selectedObj == panel){ return true; }
			}

			return false;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (this.draggingPanel == null) { return; }

			this.draggingPanel.transform.position += (Vector3)eventData.delta;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			this.draggingPanel = null;
		}
	}
}

