// Copyright (c) 2014-2014. All rights reserved.
//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UAssert = UnityEngine.Assertions.Assert;

public class DictionaryPool<ObjKey, ObjType> where ObjType : class
{
	public DictionaryPool(IEqualityComparer<ObjKey> comparer = null)
	{
		Pools = new Dictionary<ObjKey, Pool<ObjType>>(comparer);
	}

	public ObjType GetFromPool(ObjKey key, Func<ObjKey, ObjType> instantiateObjectFunc = null)
	{
		Pool<ObjType> pool;
		if (!Pools.TryGetValue(key, out pool))
		{
			ObjType res = null;
			if (instantiateObjectFunc != null)
			{
				res = instantiateObjectFunc(key);
			}
			return res;
		}

		var result = pool.GetFromPool();
		if (result == null)
		{
			if (instantiateObjectFunc != null)
			{
				result = instantiateObjectFunc(key);
			}
		}
		UAssert.IsNotNull(result, "Some bad for " + key + " found " + result);
		return result;
	}

	public void PushToPool(ObjKey key, ObjType obj)
	{
		if (!Pools.ContainsKey(key))
		{
			Pools[key] = new Pool<ObjType>();
		}

		Pools[key].Push(obj);
		UAssert.IsNotNull(obj, "Some bad for " + key + " found " + obj);
	}

	public void ClearPool(Action<ObjKey, ObjType> clearAction)
	{
		foreach (var pool in Pools)
		{
			var obj = pool.Value.GetFromPool();
			while (obj != null)
			{
				clearAction(pool.Key, obj);
				obj = pool.Value.GetFromPool();
			}
		}
	}

	private Dictionary<ObjKey, Pool<ObjType>> Pools { get; set; }
}