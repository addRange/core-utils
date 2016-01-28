// Copyright (c) 2014-2014. All rights reserved.
//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;

public class DictionaryPool<ObjKey, ObjType> where ObjType : class
{
	public DictionaryPool(IEqualityComparer<ObjKey> comparer = null)
	{
		Pools = new Dictionary<ObjKey, Pool<ObjType>>(comparer);
	}

	public ObjType GetFromPool(ObjKey key)
	{
		Pool<ObjType> pool;
		if (!Pools.TryGetValue(key, out pool))
		{
			return null;
		}

		return pool.GetFromPool();
	}

	public void PushToPool(ObjKey key, ObjType obj)
	{
		if (!Pools.ContainsKey(key))
		{
			Pools[key] = new Pool<ObjType>();
		}

		Pools[key].Push(obj);
	}

	private Dictionary<ObjKey, Pool<ObjType>> Pools { get; set; }
}