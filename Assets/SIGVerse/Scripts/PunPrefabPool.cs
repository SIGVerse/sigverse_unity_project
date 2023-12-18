using UnityEngine;
using System.Collections.Generic;

#if SIGVERSE_PUN
using Photon.Pun;
#endif

namespace SIGVerse.Common
{
#if SIGVERSE_PUN
	public class PunPrefabPool : MonoBehaviourPunCallbacks, IPunPrefabPool
#else
	public class PunPrefabPool : MonoBehaviour
#endif
	{
		public List<GameObject> prefabs;

		public void Start()
		{
#if SIGVERSE_PUN
			PhotonNetwork.PrefabPool = this;
#endif
		}

		public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
		{
			foreach (GameObject prefab in prefabs)
			{
				if (prefab.name == prefabId)
				{
					GameObject gameObject = Instantiate(prefab, position, rotation);
					gameObject.SetActive(false);
					return gameObject;
				}
			}

			return null;
		}

		public void Destroy(GameObject gameObject)
		{
			GameObject.Destroy(gameObject);
		}
	}
}

