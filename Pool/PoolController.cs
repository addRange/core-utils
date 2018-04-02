//----------------------------------------------------------------------------------------------
// author Leonid [Zanleo] Voitko
//----------------------------------------------------------------------------------------------

using UnityEngine;

public class PoolController : Singleton<PoolController>
{
	protected override void Init()
	{
		ObjectsPool = new DictionaryPool<string, GameObject>();
	}

	public DictionaryPool<string, GameObject> ObjectsPool { get; private set; }
}