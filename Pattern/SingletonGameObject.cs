//Created by Leonid [Zanleo] Voitko (2014)

//#define Debug_SingletonGameObject

using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Assert = Core.Utils.Assert;

public abstract class SingletonGameObject<T> : MonoBehaviour where T : SingletonGameObject<T>
{
	[Conditional("Debug_SingletonGameObject"), DebuggerStepThrough]
	private static void Log(string msg)
	{
		Debug.Log(msg);
	}

	/// Защищённый конструктор необходим для того, чтобы предотвратить создание 
	/// экземпляра класса Singleton. 
	/// Он будет вызван из закрытого конструктора наследственного класса.
	protected SingletonGameObject()
	{
	}

	public static T Instance
	{
		get
		{
			Assert.IsFalse(m_applicationIsQuitting, "AapplicationIsQuitting true while somebody try get instance of " + typeof(T));

			if (s_instance == null)
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					return null;
				}
#endif
				InitInstance();
			}

			return s_instance;
		}
	}

	public static T TryInstance
	{
		get { return s_instance; }
	}

	private void Awake()
	{
		Assert.IsNull(s_instance, "Found not null instance for " + s_instance);
		if (s_instance == null)
		{
			GameObject.DontDestroyOnLoad(gameObject);
			Init();
		}
		else
		{
			GameObject.DestroyImmediate(gameObject);
		}
	}

	public static void InitInstance()
	{
		Log("InitInstance " + typeof(T) + "; " + (s_instance == null));
		if (s_instance != null)
		{
			return;
		}

		var resource = Resources.Load(GetPathToAsset());
#if UNITY_EDITOR
		if (resource == null)
		{
			SingletonGameObjectsConfig.Instance[typeof(T)].Enabled = true;

			var asms = AppDomain.CurrentDomain.GetAssemblies();
			Type sgUtilType = null;
			foreach (var assembly in asms)
			{
				sgUtilType = assembly.GetType("SingletonGameobjectUtility");
				if (sgUtilType != null)
				{
					break;
				}
			}
		
			var checkTypesMethod = sgUtilType.GetMethod("CheckTypes",
				System.Reflection.BindingFlags.Static |
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.FlattenHierarchy);
			try
			{
				checkTypesMethod.Invoke(null, null);
			}
			catch
			{
			}
			resource = Resources.Load(GetPathToAsset());
		}
#endif
		Assert.IsNotNull(resource, "Resource not found for '" + GetPathToAsset() + "'");

		GameObject instanceObject = GameObject.Instantiate(resource) as GameObject;
		instanceObject.name = typeof(T).Name.ToString();
		Assert.IsNotNull(instanceObject, "Failed create Object of the " + typeof(T).ToString());

		//s_instance = instanceObject.GetComponent<T>();
		Assert.IsNotNull(s_instance, "Failed create Instance object of the " + typeof(T).ToString());

		// Init called from Awake!
	}

	// С Unity 5.3 изменена последовательность вызовов, поэтому FreeInstance не вызывается в OnApplicationQuit
	// https://docs.unity3d.com/520/Documentation/Manual/ExecutionOrder.html
	// https://docs.unity3d.com/530/Documentation/Manual/ExecutionOrder.html
	public static void FreeInstance()
	{
		Log("FreeInstance " + typeof(T) + "; " + (s_instance != null));
		if (s_instance == null)
		{
			return;
		}

		var instObj = s_instance.gameObject;
		s_instance.DeInit();
		m_deiniting = true;
		GameObject.DestroyImmediate(instObj);
		m_deiniting = false;
	}

	private void OnDestroy()
	{
		try
		{
			if (!m_deiniting)
			{
				DeInit();
			}
		}
		catch (Exception)
		{
			throw;
		}
		finally
		{
		}
	}

	protected virtual void Init()
	{
		s_instance = this as T;
		Log(GetHashCode() + ". Init " + typeof(T).ToString());
	}

	protected virtual void DeInit()
	{
		m_applicationIsQuitting = true;

		if (s_instance == this)
		{
			s_instance = null;
		}
		m_deiniting = false;
		Log(GetHashCode() + ". DeInit " + typeof(T).ToString());
	}

	public static T GetComponentOnly()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			string path = "Assets/Resources/" + GetPathToAsset() + ".prefab";
			return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
		}
#endif
		T resource = Resources.Load<T>(GetPathToAsset());
		if (resource == null)
		{
			return null;
		}

		return resource;
	}

	public static string GetPathToAsset()
	{
		string pathToRes = typeof(T).ToString().Replace('.', '/');
		pathToRes = PathToPrefabs + pathToRes;
		return pathToRes;
	}

	//Используется Reflection для создания экземпляра класса без публичного конструктора
	private static T s_instance = null;

	protected static string PathToPrefabs = "Prefabs/Scripts/";
	private static bool m_deiniting = false;
	private static bool m_applicationIsQuitting = false;
}