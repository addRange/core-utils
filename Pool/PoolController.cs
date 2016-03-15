// Copyright (c) 2014-2014. All rights reserved.
//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using UnityEngine;

public class PoolController : Singleton<PoolController>
{
	protected override void Init()
	{
		BlocksPool = new DictionaryPool<string, GameObject>();
	}

	public DictionaryPool<string, GameObject> BlocksPool { get; private set; }
}