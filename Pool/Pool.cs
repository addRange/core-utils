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

	public void PutToPool(ObjType obj)
	{
		ObjectPool.Insert(0, obj);
	}

	public List<ObjType> ObjectPool { get; private set; }
}