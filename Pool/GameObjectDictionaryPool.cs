//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

//#define ENABLE_LOGS

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.CoreUtils.Pool
{
	public class GameObjectDictionaryPool<ObjKey, ObjType> : DictionaryPool<ObjKey, ObjType>
		where ObjType : Component
	{
		public override ObjType GetFromPool(ObjKey key, Func<ObjKey, ObjType> instantiateObjectFunc = null)
		{
			var res = base.GetFromPool(key, instantiateObjectFunc);
			if (res != null)
			{
				m_objToKey.Add(res.gameObject, key);
			}
			return res;
		}

		public void PutToPool(ObjType obj)
		{
			ObjKey key;
			if (!m_objToKey.TryGetValue(obj.gameObject, out key))
			{
				Debug.LogAssertion($"Not found {obj} for return to pool by key");
				return;
			}
			PutToPool(key, obj);
		}

		public void PutToPool(GameObject obj)
		{
			PutToPool(obj.GetComponent<ObjType>());
		}

		private Dictionary<GameObject, ObjKey> m_objToKey = new Dictionary<GameObject, ObjKey>();
	}
}