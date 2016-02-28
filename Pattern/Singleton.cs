//Created by Leonid [Zanleo] Voitko (2014)
using UnityEngine;

public class Singleton<T> where T : Singleton<T>, new()
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				InitInstance();
			}

			return _instance;
		}
	}

	public static void InitInstance()
	{
		if (_instance != null)
		{
			return;
		}
		_instance = new T();
		_instance.Init();
	}

	protected virtual void Init()
	{
	}
}