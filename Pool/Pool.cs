// Copyright (c) 2014-2014. All rights reserved.
//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using System.Collections.Generic;

public class Pool<ObjType> where ObjType : class
{
	public Pool()
	{
		ObjectPool = new List<ObjType>();
	}

	public ObjType GetFromPool()
	{
		if (ObjectPool.Count == 0)
		{
			return null;
		}

		ObjType ret = ObjectPool[0];
		ObjectPool.RemoveAt(0);
		return ret;
	}

	public void Push(ObjType obj)
	{
		ObjectPool.Insert(0, obj);
	}

	private List<ObjType> ObjectPool { get; set; }
}